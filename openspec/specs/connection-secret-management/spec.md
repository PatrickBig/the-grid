# connection-secret-management Specification

## Purpose

TBD - created by archiving change encrypt-connection-secrets. Update Purpose after archive.
## Requirements
### Requirement: Secret connection parameters are encrypted at rest
Connection parameter values whose parameter is flagged as secret SHALL be stored encrypted,
separately from non-secret connection parameter values. Non-secret parameter values SHALL continue
to be stored as plaintext.

#### Scenario: Creating a connection with a secret parameter
- **WHEN** a connection is created with a value for a parameter flagged as secret (e.g. a
  `ProtectedText` password parameter)
- **THEN** the stored value for that parameter is ciphertext, not the plaintext the client sent

#### Scenario: Creating a connection with a non-secret parameter
- **WHEN** a connection is created with a value for a parameter not flagged as secret
- **THEN** the stored value for that parameter is plaintext, unchanged from what the client sent

### Requirement: Secret parameter keys are resolved from connector metadata
The system SHALL determine which connection parameter keys are secret by inspecting the
connector's declared parameter metadata — a parameter is secret if its `IsSecret` is `true`, its
`Type` is `ProtectedText`, or both — not from a value supplied by the API caller. The returned keys
SHALL be each parameter's `Key`, not its `Name`.

#### Scenario: A ProtectedText parameter is treated as secret
- **WHEN** a connector declares a parameter with type `ProtectedText`
- **THEN** that parameter's `Key` is included in the set of secret parameter keys, and values
  submitted for it are treated as secret and encrypted at rest

#### Scenario: A non-ProtectedText parameter is treated as non-secret
- **WHEN** a connector declares a parameter with a type other than `ProtectedText` and `IsSecret`
  unset (`false`)
- **THEN** that parameter's `Key` is not included in the set of secret parameter keys, and values
  submitted for it are stored as plaintext

#### Scenario: An IsSecret parameter of a non-ProtectedText type is treated as secret
- **WHEN** a connector declares a parameter with `IsSecret = true` and a type other than
  `ProtectedText`
- **THEN** that parameter's `Key` is included in the set of secret parameter keys, and values
  submitted for it are treated as secret and encrypted at rest

### Requirement: Connection API responses never expose secret values
Reading a connection through the API SHALL NOT return a secret parameter's plaintext value or its
ciphertext. It SHALL indicate only whether a value is present for that parameter.

#### Scenario: Reading a connection with a secret value set
- **WHEN** a connection with a stored secret parameter value is fetched via the API
- **THEN** the response indicates that the secret parameter has a value set, without including the
  value or its ciphertext

#### Scenario: Reading a connection's non-secret values
- **WHEN** a connection is fetched via the API
- **THEN** the response includes the real, plaintext values for all non-secret parameters

### Requirement: Updating a connection preserves secrets that are not explicitly changed
The system SHALL support updating an existing connection. A secret parameter key that is omitted
from an update request SHALL leave that parameter's stored value unchanged. A secret parameter key
included with a value SHALL replace the stored value, encrypted.

#### Scenario: Updating a connection without touching a secret parameter
- **WHEN** an update request for a connection omits a secret parameter key that already has a
  stored value
- **THEN** the previously stored encrypted value for that parameter is unchanged after the update

#### Scenario: Updating a connection's secret parameter value
- **WHEN** an update request for a connection includes a new value for a secret parameter key
- **THEN** the stored value for that parameter is replaced with the encrypted new value

#### Scenario: Updating a connection's non-secret parameters
- **WHEN** an update request for a connection includes values for non-secret parameter keys
- **THEN** the stored plaintext values for those parameters are replaced with the submitted values

### Requirement: Connector execution receives decrypted connection parameters
When a connection is used to execute a query, the connector SHALL receive one combined set of
connection parameter values with secret parameters decrypted, in the same form as non-secret
parameters. The connector implementation itself SHALL NOT be responsible for any decryption.

#### Scenario: Executing a query against a connection with a secret parameter
- **WHEN** a query is executed using a connection that has a stored encrypted secret parameter
  value
- **THEN** the connector is constructed with that parameter's decrypted plaintext value combined
  with the connection's non-secret parameter values

### Requirement: Connection read authorization uses a read operation
Fetching a single connection SHALL be authorized against a read-level operation, not a
create-level operation.

#### Scenario: Reading a connection is authorized for read access
- **WHEN** a user who has read access but not create access to a connection's organization fetches
  that connection
- **THEN** the request is authorized and the connection is returned

