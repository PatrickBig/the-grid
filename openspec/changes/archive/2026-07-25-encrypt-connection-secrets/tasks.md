## 1. Secret protection primitives

- [x] 1.1 Add `ISecretProtector` interface (`Protect(string plaintext) : string`,
      `Unprotect(string protectedValue) : string`) — decide final home (`TheGrid.Services` vs
      `TheGrid.Data`, see design.md Open Questions) and place it there.
- [x] 1.2 Implement `AesGcmSecretProtector : ISecretProtector` — random 12-byte nonce per call,
      packs `v1:<keyId>:<base64(nonce ++ ciphertext ++ tag)>`.
- [x] 1.3 Add a configuration-bound options class exposing the encryption key (single base64 key
      for now), sourced via standard `IOptions` binding (env var recommended, documented as such).
- [x] 1.4 Register `ISecretProtector` and its options in DI (`TheGridContextServices.cs` or
      equivalent).
- [x] 1.5 Unit tests for `AesGcmSecretProtector`: round-trip encrypt/decrypt, distinct nonces
      across calls, tamper-detection (corrupted ciphertext/tag fails to decrypt).

## 2. Secret-key resolution

- [x] 2.1 Add a shared helper (e.g. in `TheGrid.Connectors` or `TheGrid.Services`) that, given a
      connector type, reflects `[ConnectorParameter(..., ConnectionPropertyType.ProtectedText)]`
      and returns the set of secret parameter keys — same reflection approach
      `ConnectorDiscoveryService.DiscoverConnectors` already uses.
- [x] 2.2 Unit tests: a `ProtectedText` parameter is resolved as secret; other types are not;
      `PostgreSqlConnector` resolves exactly `{"Password"}`; `TestConnector` resolves `{}`.

## 3. Data model & persistence

- [x] 3.1 Add `Connection.SecretProperties : Dictionary<string,string?>` to
      `TheGrid.Models/Connection.cs`.
- [x] 3.2 Map `SecretProperties` in `TheGridDbContext.OnModelCreating` using the existing
      `JsonColumnConverter<Dictionary<string,string?>>` (unmodified).
- [x] 3.3 Correct the XML doc comment on `Connection.ConnectionProperties` (it stays plaintext now
      that secrets live in `SecretProperties`) and add doc comments to the new property.

## 4. EF migration squash

- [x] 4.1 Delete all files under `TheGrid.Postgres/Migrations` and `TheGrid.Sqlite/Migrations`.
- [x] 4.2 Regenerate a single `Initial` migration for the Postgres provider from the updated model.
- [x] 4.3 Regenerate a single `Initial` migration for the Sqlite provider from the updated model.
- [x] 4.4 Verify `TheGridContextFactory` design-time creation still works for both providers.
- [x] 4.5 Update `docs/AddingMigrations.md` to document the Sqlite provider flag alongside
      `postgresql`.

## 5. Connection API

- [x] 5.1 In `ConnectionsController.Post`, split the incoming `ConnectionProperties` using the
      secret-key resolver (§2): non-secret keys → `Connection.ConnectionProperties` as-is; secret
      keys → encrypted via `ISecretProtector` into `Connection.SecretProperties`.
- [x] 5.2 Add a `GetConnectionResponse` DTO (`TheGrid.Shared/Models/`) containing the connection's
      non-secret data plus real `ConnectionProperties` and presence-only
      `SecretProperties : Dictionary<string,bool>`.
- [x] 5.3 Update `ConnectionsController.Get` to map to `GetConnectionResponse` instead of returning
      the raw `Connection` entity, and fix its authorization check to use `GridOperations.Read`
      instead of `GridOperations.Create`.
- [x] 5.4 Add `UpdateConnectionRequest` DTO with non-secret `ConnectionProperties` (overwritten
      wholesale) and secret `SecretProperties` (only keys present are changed).
- [x] 5.5 Add `PUT /connections/{id}` to `ConnectionsController`: apply non-secret properties
      directly; for each secret key present in the request, encrypt and overwrite; for secret keys
      not present, leave the stored value untouched. Authorize using the same resource-based
      pattern as `Post`/`Get`.
- [x] 5.6 Integration/controller tests: create with a secret value stores ciphertext; get returns
      presence-only secrets and real non-secret values; get is authorized on read access; update
      omitting a secret key preserves it; update including a secret key replaces it; update
      changes non-secret values wholesale.

## 6. Query execution

- [x] 6.1 Update `QueryExecutor.GetConnector` to decrypt `Connection.SecretProperties` (via
      `ISecretProtector` and the secret-key resolver) and merge with `Connection.ConnectionProperties`
      into one flat dictionary before `Activator.CreateInstance`.
- [x] 6.2 Test: executing a query against a connection with a stored encrypted secret constructs
      the connector with the correct decrypted value alongside plaintext values.

## 7. Client

- [x] 7.1 Update `ConnectionPropertyEditor.razor`(`.cs`) so `ProtectedText` fields render blank
      when editing an existing connection (never pre-filled from `hasValue`/presence data).
- [x] 7.2 Ensure the editor only includes a `ProtectedText` field's key in the submitted
      update payload if the user actually entered a value.
- [x] 7.3 Wire up the client call to the new `PUT /connections/{id}` endpoint for saving edits.
      Added `EditConnection.razor`(`.cs`) (route `/Connections/{id}/Edit`), linked from
      `ConnectionList.razor` — beyond the proposal's stated client scope but needed for 7.3 to have
      a UI to wire into (see conversation: user chose to build this now rather than defer).

## 8. Verification

- [x] 8.1 Run full test suite (`dotnet test TheGrid.sln`) and confirm green. Note:
      `dotnet test TheGrid.sln` fails to restore (pre-existing, unrelated to this change —
      `docker-compose.dcproj` isn't restorable via this CLI/SDK). Ran each test project
      individually instead: Services 17, Shared 3, Client 20, Server 25, Connectors 9 — all green
      (74/74).
- [x] 8.2 Manual check: create a Postgres connection with a password via the running app, inspect
      the `connectionproperties`/`secretproperties` columns directly in the database to confirm
      ciphertext, then run a query against it to confirm execution still works end-to-end.
      Verified against a real Postgres instance via docker-compose + the running server: created a
      connection with `POST /connections` (password `thegrid123`); `Connections.SecretProperties`
      stored `{"Password":"v1:1:<ciphertext>"}` (not plaintext), `ConnectionProperties` held only
      the non-secret values; `GET /connections/{id}` returned presence-only
      `{"Password":true}`; a query executed against the connection completed successfully
      (`Status=Complete`) and returned real rows, confirming the connector received the correctly
      decrypted password. Containers torn down afterward; working tree left clean (no stray
      files). Incidentally found and worked around (not fixed — out of scope) two pre-existing,
      unrelated bugs blocking this manual path: (1) `SetupHostedService` is a singleton depending
      on scoped `TheGridDbContext`, so `/setup` crashes under DI validation when
      `ASPNETCORE_ENVIRONMENT=Development` (masked today because `docker-compose.yml`'s
      `thegrid.setup` service never sets that env var); (2) `GroupManager.CreateGroupAsync` passes
      permissions as a `Select` iterator into a navigation property typed `ICollection<>`, so
      `OrganizationsController.Post` (and thus any new-organization creation) throws
      `InvalidOperationException` after inserting the `Organization` row but before its default
      group. Both are worth separate follow-up changes.
