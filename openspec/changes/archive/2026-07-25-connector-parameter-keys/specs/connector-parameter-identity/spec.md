## ADDED Requirements

### Requirement: Connector parameters have a stable machine Key distinct from their display Name
Every connector parameter (declared via `ConnectorParameterAttribute` and surfaced as
`ConnectionProperty`) SHALL have a `Key` that identifies it for storage and lookup purposes, separate
from its `Name`, which is presentation-only. `Key` SHALL be validated as a machine identifier (starts
with a letter, followed by letters, digits, or underscores only); `Name` retains its existing broader
character allowance (letters, digits, spaces, underscores, hyphens).

#### Scenario: A parameter's Key differs in format from its Name
- **WHEN** a connector declares `[ConnectorParameter("connectionString", "Connection String",
  ConnectionPropertyType.SingleLineText)]`
- **THEN** the resulting `ConnectionProperty`'s `Key` is `"connectionString"` and `Name` is
  `"Connection String"`

#### Scenario: An invalid Key is rejected at declaration time
- **WHEN** a `[ConnectorParameter]` attribute is constructed with a `key` argument containing a space
  or a leading digit
- **THEN** the attribute constructor throws `ArgumentException`

### Requirement: Runtime parameter lookups use Key, not Name
All runtime dictionaries keyed by connector parameter identity — `ConnectorParameters` (inside a
connector), `Connection.ConnectionProperties`, `Connection.SecretProperties`, and client-side
in-progress edit state — SHALL be keyed by each parameter's `Key`, not its `Name`.

#### Scenario: A connector reads a parameter value by Key
- **WHEN** a connector's constructor or method reads a value from `ConnectorParameters`
- **THEN** it looks up the value using the parameter's `Key`, not its `Name`

#### Scenario: Renaming a parameter's display Name does not affect stored data
- **WHEN** a connector's `[ConnectorParameter]` attribute's `name` argument is changed but its `key`
  argument is unchanged
- **THEN** existing connections' stored values for that parameter remain accessible under the same
  `Key`

### Requirement: A parameter's secrecy is IsSecret OR ProtectedText
A connector parameter SHALL be treated as secret if its declared `IsSecret` is `true`, or if its
`Type` is `ConnectionPropertyType.ProtectedText`, or both. `IsSecret` defaults to `false` and is
independently settable regardless of `Type`.

#### Scenario: A ProtectedText parameter is secret even without explicit IsSecret
- **WHEN** a connector declares a parameter with `Type = ConnectionPropertyType.ProtectedText` and does
  not set `IsSecret`
- **THEN** the parameter's key is included in the connector's set of secret parameter keys

#### Scenario: An explicitly-flagged non-ProtectedText parameter is also secret
- **WHEN** a connector declares a parameter with `IsSecret = true` and a `Type` other than
  `ProtectedText`
- **THEN** the parameter's key is included in the connector's set of secret parameter keys
