## Context

The connector SDK (`TheGrid.Connectors.Abstractions` + `TheGrid.Connectors`) has two real
implementations today: `PostgreSqlConnector` (relational, SQL-text `Query.Command`, `information_schema`
schema discovery) and `MongoDbConnector` (document store, JSON-object `Query.Command`, `$sample`-based
schema discovery). SQL Server is structurally identical to Postgres from the SDK's point of view —
relational, ANSI `INFORMATION_SCHEMA`-queryable, SQL-text `Query.Command` — so it validates almost none
of the SDK's row/column/schema abstractions further. What it *does* add is a genuinely new design
surface: enterprise SQL Server deployments (especially Azure SQL / Azure SQL Managed Instance) are
commonly accessed via Azure AD identities rather than SQL logins, and The Grid's target customers run a
wide variety of cluster topologies (AKS, EKS, on-prem Kubernetes, local dev) — not all of them
Azure-hosted, since being co-located with your data isn't guaranteed for a BI tool.

`ConnectorBase` (primary-constructor form, taking a `ConnectorContext`), `IConnectorFactory`,
`ISchemaDiscovery`, `IConnectionTest`, and `IWriteAccessProbe` are all current (post-P1-3/P1-5)
contracts — this design targets those. `CommonConnectionParameters` already has both `DatabaseName` and
`Database` constants (a pre-existing near-duplicate — see Decision 4); `PostgreSqlConnector.cs` uses
`DatabaseName`, so this connector follows that precedent for consistency with the connector it's closest
to structurally, rather than `MongoDbConnector`'s `Database`.

This design is authoritative for the SQL Server connector's decisions; where it disagrees with the
`add-sqlserver-connector` proposal's summary, this document wins on specifics.

## Goals / Non-Goals

**Goals:**
- Implement a `SqlServerConnector` that mirrors `PostgreSqlConnector`'s shape (plain T-SQL
  `Query.Command`, `SqlParameter`-bound query parameters, `INFORMATION_SCHEMA`-based schema discovery,
  connectivity test, write-access probe) using `Microsoft.Data.SqlClient`.
- Add a genuinely new, well-justified authentication surface: `AuthenticationMode` selecting SQL
  Authentication, Azure AD Service Principal, or Azure AD Default Credential Chain — prioritizing the
  option that works identically across any deployment topology.
- Do this within the existing `[ConnectorParameter]`/`ConnectorBase` contract, without changing the SDK,
  even though the mode-dependent parameter set exposes a real, already-tracked SDK gap
  (`VisibleWhen`/`DependsOn`).

**Non-Goals:**
- Richer, SQL-Server-specific schema metadata via `sys.tables`/`sys.columns` (identity columns,
  computed columns, temporal tables). `INFORMATION_SCHEMA` (ANSI-standard, mirroring
  `PostgreSqlConnector.GetSchemaAsync`) is sufficient for this change; `DatabaseObjectColumn.Attributes`
  already supports arbitrary engine-specific metadata later if that's ever wanted.
- Solving conditional connector-parameter visibility (`VisibleWhen`/`DependsOn`) at the SDK level. This
  connector is that item's first real motivating consumer, not its implementer — see Risks/Trade-offs.
- On-prem Windows/Kerberos authentication (a shared AD service-account identity per Connection, via a
  stored keytab + init/sidecar `kinit` pattern in Kubernetes). This is real, separate infrastructure
  complexity (keytabs, ccache sharing, `krb5.conf`/DNS reachability to on-prem domain controllers) that
  deserves its own future explore+propose, not a fold-in here.
- A new `IParameterizedQuery` capability interface or any change to the deferred query-parameter design
  question (`docs/Roadmap.md` §5.3). This connector's parameter binding (`SqlParameter`/`@paramName`) is
  structurally identical to Postgres's `AddWithValue` pattern, so it doesn't add a new data point to that
  still-open design question the way MongoDB's JSON-command shape did.

## Decisions

### 1. `Query.Command` is plain T-SQL text; parameters bind via `SqlParameter`

Like Postgres, `Query.Command` for this connector is executed as-is against the server — no JSON
envelope, no mode selection. `GetDataAsync` builds a `SqlCommand` from the query text directly. When the
caller supplies `queryParameters`, each is bound via `command.Parameters.AddWithValue(key, value ??
DBNull.Value)`, the same shape `PostgreSqlConnector.GetDataAsync` already uses with `NpgsqlCommand`. No
new design work is required here: SQL Server's parameter-binding model (`@paramName` placeholders bound
via `SqlParameter`) is the same shape as Postgres's, so this connector doesn't add a new data point to
the still-deferred `IParameterizedQuery` capability-interface question (`docs/Roadmap.md` §5.3, §3.8) —
that question was meaningfully advanced by MongoDB (a structurally different binding story), not by this
connector.

### 2. Editor syntax highlighting uses `EditorLanguage.Sql`, not a new T-SQL constant

`TheGrid.Shared/Models/EditorLanguage.cs` mirrors the Monaco editor's actual `basic-languages` list,
which includes `Sql`, `PgSql`, `MySql`, and `Redshift` — but no distinct T-SQL grammar. There is nothing
more specific to add for SQL Server; `[Connector("SQL Server", EditorLanguage = EditorLanguage.Sql, ...)]`
is correct and complete, matching how `PostgreSqlConnector` sets `EditorLanguage.PgSql` from the same
enum-like class.

### 3. Schema discovery queries `INFORMATION_SCHEMA.COLUMNS`/`INFORMATION_SCHEMA.TABLES`

`GetSchemaAsync` mirrors `PostgreSqlConnector.GetSchemaAsync`'s shape: a single query joining
`INFORMATION_SCHEMA.COLUMNS` to `INFORMATION_SCHEMA.TABLES`, producing one `DatabaseObject` per
table/view (`TABLE_TYPE` normalized the same way Postgres's `BASE TABLE` → `TABLE` mapping is), each with
its columns. This is the ANSI-standard surface every mainstream relational engine exposes, deliberately
chosen over SQL Server's richer `sys.tables`/`sys.columns` catalog views.

**Alternative considered**: `sys.tables`/`sys.columns`, which would surface SQL-Server-specific metadata
(identity columns via `sys.identity_columns`, computed columns, temporal/system-versioned tables).
Rejected for this change — going after `sys.*` views doesn't prove anything new about the SDK the way
`INFORMATION_SCHEMA` parity with Postgres already does, and richer metadata has an existing escape hatch
(`DatabaseObjectColumn.Attributes`, a free-form dictionary) if a future change wants it. Keeping this
connector's first cut ANSI-standard keeps the Postgres/SQL-Server schema-discovery code visibly parallel,
which is useful for anyone maintaining both.

### 4. Connection parameters reuse `CommonConnectionParameters`; note the `DatabaseName`/`Database` wart

`SqlServerConnector` declares `ConnectionString`, `DatabaseName`, `Username`, `Password`, `PortNumber` —
the same required/shape pattern `PostgreSqlConnector` uses for its SQL-authentication baseline.
`CommonConnectionParameters.cs` currently has both a `DatabaseName` constant (`"databaseName"`, used by
`PostgreSqlConnector`) and a `Database` constant (`"database"`, used by `MongoDbConnector`) — a
pre-existing near-duplicate that predates this change. This connector uses `DatabaseName`, matching
`PostgreSqlConnector` (the connector it's structurally closest to), not `MongoDbConnector`. This wart is
noted here as a fact to be aware of, not something this change fixes — collapsing the two constants would
be a breaking rename affecting an existing shipped connector's stored connections, out of scope for an
additive change.

### 5. `AuthenticationMode` selects SQL Authentication, Azure AD Service Principal, or Azure AD Default Credential Chain

A new `AuthenticationMode` connector parameter (a small fixed set of string values, e.g.
`SqlAuthentication` / `ServicePrincipal` / `DefaultCredential`) selects between three ways of
establishing the `SqlConnection`:

1. **SQL Authentication** (baseline) — `Username`/`Password`, exactly like Postgres. No token
   acquisition; standard SQL login.
2. **Azure AD Service Principal** — new `TenantId`, `ClientId`, `ClientSecret` connector parameters
   (`ClientSecret` marked `IsSecret = true`). The connector builds a `ClientSecretCredential(tenantId,
   clientId, clientSecret)` (`Azure.Identity`) and sets `SqlConnection.AccessTokenCallback` to acquire a
   token for scope `https://database.windows.net/.default` on each connection open.
3. **Azure AD Default Credential Chain** — the connector builds a `DefaultAzureCredential()`
   (`Azure.Identity`), using the same `AccessTokenCallback` mechanism, with zero secrets stored in Grid.

Both Azure AD modes use `SqlConnection.AccessTokenCallback` with an explicitly constructed
`Azure.Identity` credential — **not** the connection-string `Authentication=` keywords (`Active Directory
Service Principal`, `Active Directory Default`, etc.). Confirmed against `Microsoft.Data.SqlClient`'s own
current documentation (Context7 `/dotnet/sqlclient`, `Microsoft.Data.SqlClient.Extensions.Azure`'s
package readme): the connection-string keyword approach requires the separate
`Microsoft.Data.SqlClient.Extensions.Azure` NuGet package and is less explicit about which credential
type is actually in use, whereas `AccessTokenCallback` lets the connector construct the exact credential
type per mode directly:

```csharp
using Microsoft.Data.SqlClient;
using Azure.Identity;

var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
using var connection = new SqlConnection(connectionString);
connection.AccessTokenCallback = async (ctx, cancellationToken) =>
{
    var token = await credential.GetTokenAsync(
        new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }),
        cancellationToken);
    return new SqlAuthenticationToken(token.Token, token.ExpiresOn);
};
await connection.OpenAsync(cancellationToken);
```

`DefaultAzureCredential()` follows the identical shape, just with a different credential constructed.

**Decision rationale — why Service Principal is "the" primary Azure AD path, not Workload Identity:**
The Grid's product goal is to be enterprise-generic and eventually sellable, not tailored to any one
company's deployment. Real target customers run a wide variety of topologies — Azure-managed data
services (Azure SQL, Azure SQL Managed Instance) accessed from clusters that aren't necessarily AKS or
even in Azure at all (EKS, on-prem Kubernetes), since this is a BI tool and being co-located with your
data isn't guaranteed. An authentication option that only works when The Grid happens to be running
inside AKS (Workload Identity) under-serves that breadth — it would work for one topology and silently
not for others, with no portable fallback for those customers short of dropping to SQL auth. A portable,
connection-scoped credential (Service Principal: static app-registration credentials stored as an
encrypted Grid connection secret, exactly the same storage mechanism as a password today) serves every
topology identically, regardless of what cluster or cloud The Grid itself runs in. That's why Service
Principal is treated as the primary/recommended Azure AD path here, with Default Credential Chain offered
as a secondary, zero-secret alternative for deployments whose ops team has specifically wired the
right federation (AKS Workload Identity, Managed Identity, or an externally-federated OIDC credential —
Azure AD supports federating with external issuers, not just AKS, so this can work on EKS/on-prem too if
a platform team configures that trust). The Grid's code doesn't need to know or care which mechanism
`DefaultAzureCredential` resolves to at runtime — that's purely an ops-side wiring choice per deployment.

**Alternative considered**: making Default Credential Chain (Workload Identity-first) the primary/default
mode, with Service Principal as a fallback. Rejected — that ordering implicitly optimizes for the subset
of customers running on AKS with Workload Identity configured, which is exactly the narrower-topology
assumption this design is trying to avoid baking into the connector's primary path.

### 6. Conditional parameter requiredness is validated in connector code, not the SDK

`TenantId`/`ClientId`/`ClientSecret` are only meaningful when `AuthenticationMode = ServicePrincipal`,
but `ConnectorParameterAttribute`/`ConnectorBase.ValidateParameters` has no "required only when
parameter X has value Y" mechanism (`VisibleWhen`/`DependsOn` — tracked as a deferred item in
`docs/roadmap/ChangeSpecs.md` Phase 2+ and `docs/Roadmap.md` §3.2; this connector is that item's first
real motivating consumer, not its implementer). For this change:

- `TenantId`, `ClientId`, `ClientSecret` are declared with `Required = false` at the attribute level, so
  `ConnectorBase`'s blanket required-parameter check never blocks a connection that isn't using
  `ServicePrincipal` mode from being saved.
- `SqlServerConnector` performs its own mode-specific validation in code (e.g. in the method that builds
  the connection/credential) — when `AuthenticationMode == ServicePrincipal` and any of the three
  parameters is missing or empty, it throws `ConnectorParameterException` directly, naming the missing
  parameters, matching the shape (if not the trigger point) of `ConnectorBase`'s own exception.
- The connection-creation UI shows all declared parameters flatly regardless of the selected
  `AuthenticationMode` — a known, accepted rough edge for this change, not a blocker. A user selecting SQL
  Authentication will still see `TenantId`/`ClientId`/`ClientSecret` fields (unused, and not required to
  fill in); a user selecting Service Principal will still see `Username`/`Password` fields (unused).

### 7. Capabilities: `ISchemaDiscovery`, `IConnectionTest`, and `IWriteAccessProbe`

Unlike `MongoDbConnector` (which skipped `IWriteAccessProbe` because Mongo's role-based permission model
has no single cheap "can I write here" query), SQL Server has a direct analog to Postgres's
`has_table_privilege`: the built-in `HAS_PERMS_BY_NAME` system function. `HasWriteAccessAsync` iterates
tables from `INFORMATION_SCHEMA.TABLES` and checks `HAS_PERMS_BY_NAME` for `INSERT`/`UPDATE`/`DELETE`
permission on each, following the same "any table grants any write permission → flagged write-capable"
logic `PostgreSqlConnector.HasWriteAccessAsync` uses. `TestConnectionAsync` mirrors Postgres's pattern: open
the connection (honoring whichever `AuthenticationMode` is configured) and run a trivial `SELECT 1`,
returning a `ConnectionTestResult` (`Success`, `Message`, `Elapsed`) per the existing
`connector-capability-results` contract.

## Risks / Trade-offs

- **[Risk] The connection-creation UI shows `TenantId`/`ClientId`/`ClientSecret` and
  `Username`/`Password` simultaneously and flatly, regardless of selected `AuthenticationMode`, which is
  confusing UX** → Mitigation: accepted for this change per Decision 6; the underlying SDK gap
  (`VisibleWhen`/`DependsOn`) is tracked in `docs/roadmap/ChangeSpecs.md` Phase 2+ and `docs/Roadmap.md`
  §3.2, with this connector now cited there as the first real motivating consumer. Pick that item up once
  a second connector also wants conditional visibility, or when this rough edge becomes annoying enough
  to fix on its own.
- **[Risk] Mode-specific required-parameter validation lives in connector code instead of the declarative
  `[ConnectorParameter]` metadata, so a future connector with the same need will likely duplicate this
  pattern rather than reuse shared validation** → Accepted: duplicating a small `if` block across two
  connectors is cheaper than generalizing a validation abstraction from a single data point; revisit if a
  third connector needs the same shape.
- **[Trade-off] `INFORMATION_SCHEMA`-only schema discovery omits identity/computed/temporal-table
  metadata SQL-Server-specific `sys.*` catalog views would expose** → Accepted per Decision 3;
  `DatabaseObjectColumn.Attributes` is available as a free-form escape hatch for a future change that
  wants it, without needing a schema-discovery redesign.
- **[Trade-off] Default Credential Chain's actual behavior depends entirely on ops-side configuration The
  Grid's code cannot see or validate at connector-build time** (e.g. whether Workload Identity is properly
  federated for the running cluster) → Accepted: this is an inherent property of `DefaultAzureCredential`,
  not something this connector can meaningfully guard against; `TestConnectionAsync` is the mechanism a
  user has to discover a misconfiguration, same as it would be for any other connection issue.
- **[Non-goal, not a risk] On-prem Windows/Kerberos authentication is not designed here** — see
  Goals/Non-Goals; a real future need, deliberately deferred to its own explore+propose given its
  independent infrastructure complexity (keytabs, ccache sharing, `krb5.conf`/DNS reachability to on-prem
  domain controllers).

## Migration Plan

None — this is a purely additive connector; no existing connection, schema, or migration is touched.
`ConnectorDiscoveryService`'s existing reflection-based discovery picks up the new
`[Connector]`-decorated class automatically on next discovery run, the same way `MongoDbConnector` was
picked up.

## Open Questions

- Should `AuthenticationMode`'s allowed values be validated at the `[ConnectorParameter]` level (e.g. via
  a future `Validation`/allowed-values metadata addition) rather than only in connector code? Currently
  out of scope — `Validation` sub-object metadata was deliberately scoped out of P1-1 (`docs/Roadmap.md`
  §3.2) for lack of a consumer; this connector could become that consumer later, but doesn't need to be
  for a correct first implementation (connector code validates the mode value itself).
- Should the connection-creation UI eventually group `AuthenticationMode`-dependent parameters under a
  visual "Authentication" section even before `VisibleWhen`/`DependsOn` lands, using the already-deferred
  `Group` parameter metadata? Left open — not needed for a correct implementation, purely a UX
  nice-to-have that would require its own small scoping pass.
