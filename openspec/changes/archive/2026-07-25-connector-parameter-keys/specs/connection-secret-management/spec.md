## MODIFIED Requirements

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
