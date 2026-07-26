## Context

`IConnectionTest.TestConnectionAsync` (`Task<bool>`) and `IPermissionTest.HasWritePermissionAsync`
(`Task<bool>`) are both implemented only by `PostgreSqlConnector`. Grepping the whole solution confirms
neither is called anywhere except that one implementation and its own tests — no controller endpoint,
no client code, invokes either. `ConnectorDiscoveryService` computes `SupportsConnectionTest`/
`SupportsSchemaDiscovery` discovery flags (persisted on `Connector`, itself an EF entity — confirmed via
migration files showing them as real table columns) but has no equivalent flag for `IPermissionTest` at
all.

`PostgreSqlConnector.TestConnectionAsync`'s current body:
```csharp
public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
{
    await using var connection = GetConnection(ConnectorParameters);
    await connection.OpenAsync(cancellationToken);
    await using var command = new NpgsqlCommand("select 1", connection);
    return true;
}
```
The `command` is constructed but never executed, and a connection failure during `OpenAsync` isn't
caught — it propagates as an uncaught exception. This is confirmed by the existing test
`TestConnection_Fails_Test`, which asserts `Assert.ThrowsAnyAsync<Exception>`, not that the method
returns `false`. The bare-`bool` contract was never actually honored for the failure case.

## Goals / Non-Goals

**Goals:**
- `TestConnectionAsync` actually executes its test query and reports failures as data
  (`ConnectionTestResult.Success == false` + a message), not as an uncaught exception.
- `IPermissionTest`/`HasWritePermissionAsync` renamed to read correctly (`IWriteAccessProbe`/
  `HasWriteAccessAsync`).
- Discovery-flag parity: all three capability interfaces (`ISchemaDiscovery`, `IConnectionTest`,
  `IWriteAccessProbe`) get an equivalent `Supports*` flag on `Connector`.

**Non-Goals:**
- No controller endpoint or client UI — see proposal.md's "Explicitly out of scope." Building the
  actual "test this connection" feature (API + UI badge) is separate, future feature work.
- No change to `ISchemaDiscovery` — out of scope for this item.

## Decisions

**`ConnectionTestResult` lives in `TheGrid.Connectors.Abstractions`, alongside the capability
interfaces it belongs to** — same placement reasoning as `DatabaseSchema` (P1-2): it's part of the
connector-authoring contract, not something the client/server host defines independently.

**Rename the method along with the interface (`HasWritePermissionAsync` → `HasWriteAccessAsync`), not
just the interface.**
`docs/roadmap/ChangeSpecs.md` only explicitly says rename the interface; leaving the method named
`HasWritePermissionAsync` on an interface called `IWriteAccessProbe` would read inconsistently
("permission" vs "access") for no benefit — this is a small enough rename to do in the same breaking
change rather than leaving a naming mismatch behind.

**`PostgreSqlConnector.TestConnectionAsync` catches `Exception` broadly (not a narrower Npgsql-specific
exception type) around the open+execute, returning `ConnectionTestResult(false, ex.Message, elapsed)`.**
A connection test's entire purpose is to report *any* failure back as data rather than crash the caller
— narrowing the catch to specific exception types would silently let some failure modes (e.g. a DNS
resolution error vs an auth error, which may not share a common Npgsql-specific base) continue to
propagate uncaught, defeating the point of this change.

**Migration: add a new incremental migration, don't re-squash.**
P0-1 squashed all pre-existing migrations into a single `Initial` per provider, and one incremental
migration (`ExecutionLimitsAndTruncation`) has already been added on top of that squash. Continuing to
squash on every small schema change isn't a sustainable practice — this change adds one more ordinary
incremental migration (e.g. `ConnectorWriteAccessProbeSupport`) in both `TheGrid.Postgres` and
`TheGrid.Sqlite`, following normal EF Core migration workflow.

## Risks / Trade-offs

- **[Risk]** The behavior flip (throws → returns `Success: false`) for `TestConnection_Fails_Test`
  could be missed if the task list isn't explicit that this is intentional, not a regression.
  **[Mitigation]** tasks.md calls this out explicitly as an assertion change, not just a compile fix.
- **[Risk]** Adding a new EF migration requires both provider assemblies to regenerate correctly and the
  model snapshot to match.
  **[Mitigation]** tasks.md requires verifying `TheGridContextFactory` design-time creation still works
  for both providers after adding the migration (same check `docs/AddingMigrations.md`/prior migration
  work already established).

## Migration Plan

One new EF Core migration per provider for `Connector.SupportsWriteAccessProbe`. No data migration
needed — `ConnectorDiscoveryService.RefreshConnectorsAsync()` recomputes all `Connector` rows from
scratch on each run (existing behavior, unaffected by this change), so the new column populates itself
correctly the next time discovery runs.

## Open Questions

None.
