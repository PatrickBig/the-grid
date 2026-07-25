# Creating a new connector

Adding support for a new data source is done by adding a new connector class to the `TheGrid.Connectors` project.

## Getting started

`TheGrid.Connectors` includes a `GlobalUsings.cs` file with global `using` statements for `TheGrid.Connectors.Attributes`, `TheGrid.Connectors.Models`, and `TheGrid.Shared.Models` — the namespaces most connector code needs.

All connectors must meet the following minimum criteria:

* Inherit from `TheGrid.Connectors.ConnectorBase`.
* Apply a `TheGrid.Connectors.Attributes.ConnectorAttribute` to the class.

Connectors are discovered automatically by `ConnectorDiscoveryService` — no manual registration step is needed. **Current limitation:** discovery only reflects over the single assembly that defines `IConnector` (`TheGrid.Connectors` itself, via `Assembly.GetAssembly(typeof(IConnector))`) — a connector class must live in that project to be found. A connector defined in a separate assembly is invisible to the platform today.

### Define the connector

Rather than inventing a fictional example class, this guide walks through the real `PostgreSqlConnector` (`source/TheGrid.Connectors/PostgreSqlConnector.cs`), which ships with The Grid today. Quoting a real, compiling connector means this guide can't silently drift out of sync the way a hand-written example can — if the excerpts below stop matching the file, the file is the one that's authoritative.

Here is the class declaration, attributes, and constructor, quoted directly from `PostgreSqlConnector.cs`:

```csharp
[Connector("PostgreSQL", EditorLanguage = EditorLanguage.PgSql, IconFileName = "postgresql.png")]
[ConnectorParameter(CommonConnectionParameters.ConnectionString, ConnectionPropertyType.SingleLineText, Required = true, HelpText = "Standard [PostgreSQL connection string](https://www.connectionstrings.com/postgresql/).")]
[ConnectorParameter(CommonConnectionParameters.DatabaseName, ConnectionPropertyType.SingleLineText, Required = true)]
[ConnectorParameter(CommonConnectionParameters.Username, ConnectionPropertyType.SingleLineText, Required = true)]
[ConnectorParameter(CommonConnectionParameters.Password, ConnectionPropertyType.ProtectedText, Required = true)]
public class PostgreSqlConnector : ConnectorBase, ISchemaDiscovery, IConnectionTest, IPermissionTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlConnector"/> class.
    /// </summary>
    /// <param name="connectorParameters">Properties used to initiate the connection to the PostgreSQL database.</param>
    public PostgreSqlConnector(Dictionary<string, string> connectorParameters)
        : base(connectorParameters)
    {
    }

    // ... GetSchemaAsync, GetDataAsync, TestConnectionAsync, HasWritePermissionAsync (see below)
}
```

Breaking this down we have four elements: the `Connector` attribute, one or more `ConnectorParameter` attributes, the constructor, and the capability interfaces (`ISchemaDiscovery`, `IConnectionTest`, `IPermissionTest`) implemented on top of the required `ConnectorBase`/`GetDataAsync` contract.

#### The Connector attribute

The `[Connector]` attribute (`TheGrid.Connectors.Attributes.ConnectorAttribute`) applied to the class defines metadata used to identify the connector and hint the UI when users create connections or write queries.

```csharp
[Connector("PostgreSQL", EditorLanguage = EditorLanguage.PgSql, IconFileName = "postgresql.png")]
```

**ConnectorAttribute properties**
|Attribute property |Description                                                                                   |Required |
|-------------------|-----------------------------------------------------------------------------------------------|---------|
|`Name`             |Display name for the connector.                                                                |Yes      |
|`EditorLanguage`   |Language used by the query editor component. Common values are defined as constants on `TheGrid.Shared.Models.EditorLanguage` (e.g. `EditorLanguage.PgSql`). |No       |
|`IconFileName`     |Icon used in the user interface for the connector. Defaults to `"undefined.png"`.               |No       |

`[Connector]` may only be applied once per class (`AllowMultiple = false`).

#### The constructor

Every connector must have a constructor that accepts a `Dictionary<string, string> connectorParameters` and passes it to the `ConnectorBase` constructor:

```csharp
public PostgreSqlConnector(Dictionary<string, string> connectorParameters)
    : base(connectorParameters)
{
}
```

`ConnectorBase`'s constructor stores the dictionary on the protected `ConnectorParameters` property and immediately calls `ValidateParameters`, which checks that every `[ConnectorParameter]` marked `Required = true` has a non-empty value in the dictionary — throwing `ConnectorParameterException` if any are missing. This validation runs before your own constructor body (if you add one), so by the time your code runs, required parameters are guaranteed present.

#### The GetDataAsync method

The only method required by `IConnector` (and left abstract on `ConnectorBase`) is `GetDataAsync`. Its real signature, from `ConnectorBase.cs`:

```csharp
public abstract IAsyncEnumerable<ConnectorRow> GetDataAsync(string query, Dictionary<string, object?>? queryParameters, [EnumeratorCancellation] CancellationToken cancellationToken = default);
```

This is a **streaming** method, not one that builds up an in-memory result object and returns it. You implement it as an `async IAsyncEnumerable<ConnectorRow>` iterator method using `yield return` once per row, so a consumer using `await foreach` can start processing rows before the entire result set has been read from the data source (and can stop early / cancel without you having buffered rows that are never used).

Here is `PostgreSqlConnector.GetDataAsync` in full, quoted from `PostgreSqlConnector.cs`:

```csharp
/// <inheritdoc/>
public override async IAsyncEnumerable<ConnectorRow> GetDataAsync(string query, Dictionary<string, object?>? queryParameters, [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    await using var connection = GetConnection(ConnectorParameters);

    await connection.OpenAsync(cancellationToken);

    Dictionary<string, QueryResultColumn>? columns = null;

    // Run the query
    await using var command = new NpgsqlCommand(query, connection);

    if (queryParameters != null && queryParameters.Count != 0)
    {
        foreach (var parameter in queryParameters.Where(p => p.Value != null))
        {
            command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
        }
    }

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);

    // Iterate over the results
    while (await reader.ReadAsync(cancellationToken))
    {
        columns ??= GetColumns(reader);

        var row = new Dictionary<string, object?>();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            row.Add(reader.GetName(i), reader.GetValue(i));
        }

        yield return new ConnectorRow(columns, row);
    }
}
```

A few things worth calling out about this pattern:

* Declaring the method with the `async IAsyncEnumerable<ConnectorRow>` return type and using `[EnumeratorCancellation] CancellationToken cancellationToken` lets the framework properly propagate cancellation into the iterator — match this signature exactly (including the attribute) when overriding.
* `columns` is built exactly once, from the first row read (`columns ??= GetColumns(reader)`), and that same `Dictionary<string, QueryResultColumn>` instance is reused for every subsequent `ConnectorRow` yielded in the stream — column metadata isn't recomputed per row.
* `queryParameters` (when supplied) are added to the underlying command's parameter collection, enabling [parameterized queries](https://learn.microsoft.com/en-us/aspnet/web-forms/overview/data-access/accessing-the-database-directly-from-an-aspnet-page/using-parameterized-queries-with-the-sqldatasource-cs) when your driver supports them.
* Each `yield return new ConnectorRow(columns, row)` streams one row at a time to the caller.

##### ConnectorRow

`GetDataAsync` yields `TheGrid.Shared.Models.ConnectorRow`, defined as:

```csharp
public sealed record ConnectorRow(IReadOnlyDictionary<string, QueryResultColumn> Columns, IReadOnlyDictionary<string, object?> Data);
```

|Property |Type                                              |Description                                                                          |
|---------|---------------------------------------------------|--------------------------------------------------------------------------------------|
|`Columns`|`IReadOnlyDictionary<string, QueryResultColumn>`   |Column metadata for the result set, keyed by column name. Built once and the same reference is shared across every row in the stream. |
|`Data`   |`IReadOnlyDictionary<string, object?>`             |The row's values, keyed by column name.                                              |

`QueryResultColumn` (`TheGrid.Shared.Models`) currently exposes a single `Type` property (`QueryResultColumnType`, e.g. `Text`, `Integer`, `Long`, `Decimal`, `DateTime`, `Time`, `Guid`, `Binary`, `Json`, `Boolean`, `Unknown`) describing the value's data type. `PostgreSqlConnector`'s private `GetColumns` helper builds this dictionary from the `NpgsqlDataReader`'s field names and types (via a `GetQueryResultColumnTypeForType()` extension method) the first time a row is read.

### Use connector parameters to provide connection information

A connector's constructor receives a `Dictionary<string, string> connectorParameters` containing whatever values a user supplied when setting up a connection. You declare what parameters exist — and how they should be rendered in the UI — using one `[ConnectorParameter]` attribute per parameter, applied to the connector class (`AllowMultiple = true`, so you can stack as many as you need).

Some common ones for a typical relational database are **Connection String**, **Database Name**, **Username**, and **Password**. It's recommended to keep secrets like passwords as a separate parameter from the connection string so they can be handled specially in the UI (e.g. masked/hidden).

**ConnectorParameterAttribute properties**
|Attribute property |Description                                                                                       |Required |
|-------------------|--------------------------------------------------------------------------------------------------|---------|
|`Name`             |Name used to render the label for the control, and (currently) the literal key this parameter's value is stored/looked up under — see the note below. May only contain letters, numbers, spaces, underscores, and hyphens (enforced in the attribute constructor). |Yes      |
|`Type`             |Input type used in the UI for the parameter, a `ConnectionPropertyType` value.                    |Yes      |
|`RenderOrder`      |Used when rendering the UI to show the items in a specific order. Defaults to `100`.               |No       |
|`HelpText`         |Short text for the property to display to the user. Must be 200 characters or fewer (enforced by the attribute; longer values throw `ArgumentException`).|No       |
|`Required`         |Set to `true` if the user must supply a value when setting up a connection using this connector. Enforced by `ConnectorBase`'s constructor via `ValidateParameters`. |No       |

`Type` is a `TheGrid.Shared.Models.ConnectionPropertyType` enum with the following values:

**ConnectionPropertyType** values
|Enum Value          |Description                                                       |
|---------------------|-------------------------------------------------------------------|
|`SingleLineText`     |Single line of text for input.                                     |
|`MultipleLineText`   |Multiple lines of text for input.                                  |
|`ProtectedText`      |Password-style input; users will not be able to view this content. |
|`Numeric`            |Numeric input only.                                                |
|`Boolean`            |Checkbox yes/no style input.                                       |

`PostgreSqlConnector` declares its four parameters like this:

```csharp
[ConnectorParameter(CommonConnectionParameters.ConnectionString, ConnectionPropertyType.SingleLineText, Required = true, HelpText = "Standard [PostgreSQL connection string](https://www.connectionstrings.com/postgresql/).")]
[ConnectorParameter(CommonConnectionParameters.DatabaseName, ConnectionPropertyType.SingleLineText, Required = true)]
[ConnectorParameter(CommonConnectionParameters.Username, ConnectionPropertyType.SingleLineText, Required = true)]
[ConnectorParameter(CommonConnectionParameters.Password, ConnectionPropertyType.ProtectedText, Required = true)]
```

For the `Name` argument you can pass any string meeting the character restriction above, but it's recommended to use the constants on `TheGrid.Connectors.CommonConnectionParameters` (`ConnectionString`, `DatabaseName`, `Username`, `Password`, `PortNumber`, `Database`) for commonly-needed parameters, both to avoid typos and to keep naming consistent across connectors.

#### Current behavior: parameters are keyed by their display `Name`

There is currently no separate "machine key" distinct from the display `Name` — the string you pass as `[ConnectorParameter]`'s `Name` argument is **the same string** used as the dictionary key in `ConnectorParameters` (and in the `connectorParameters` dictionary passed to your constructor) at runtime. For example, `CommonConnectionParameters.Password` is the literal string `"Password"`, and that literal string is exactly what you look up:

```csharp
if (properties.TryGetValue(CommonConnectionParameters.Password, out string? password))
{
    builder.Password = password;
}
```

This is current behavior, not a bug this guide is glossing over: **renaming a `[ConnectorParameter]`'s `Name` is a breaking change** for any connection that already has a stored value under the old name, since the stored dictionary key won't match the new attribute's `Name` anymore. Use the `CommonConnectionParameters` constants where they apply so you don't have to invent (and later rename) your own literal strings.

Consuming parameters inside your connector means reading from the protected `ConnectorParameters` dictionary (set by `ConnectorBase`'s constructor). `PostgreSqlConnector`'s private `GetConnection` helper shows the pattern:

```csharp
private static NpgsqlConnection GetConnection(Dictionary<string, string> properties)
{
    // Attempt to build a connection based on the information
    var builder = new NpgsqlConnectionStringBuilder(properties[CommonConnectionParameters.ConnectionString]);

    // If there is a username or password, try using those to update the settings.
    if (properties.TryGetValue(CommonConnectionParameters.Password, out string? password))
    {
        builder.Password = password;
    }

    if (properties.TryGetValue(CommonConnectionParameters.Username, out string? username))
    {
        builder.Username = username;
    }

    if (properties.TryGetValue(CommonConnectionParameters.DatabaseName, out string? databaseName))
    {
        builder.Database = databaseName;
    }

    return new NpgsqlConnection(builder.ConnectionString);
}
```

## Adding database schema discovery support

If your connector talks to a data source with a defined schema — tables, columns, and their attributes, for example — implement `TheGrid.Connectors.ISchemaDiscovery` on your connector class:

```csharp
public interface ISchemaDiscovery
{
    public Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default);
}
```

`PostgreSqlConnector` implements this by querying `information_schema.columns`/`information_schema.tables`, quoted here from `PostgreSqlConnector.cs`:

```csharp
/// <inheritdoc/>
public async Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
{
    await using var connection = GetConnection(ConnectorParameters);

    await connection.OpenAsync(cancellationToken);

    var results = new DatabaseSchema
    {
        DatabaseName = connection.Database,
    };

    // List all the tables
    var tables = new List<DatabaseObject>();

    await using var command = new NpgsqlCommand(
        @"select
        t.table_schema,
        t.table_name,
        t.table_type,
        c.column_name,
        c.data_type,
        c.udt_name,
        c.is_nullable,
        c.character_maximum_length,
        c.is_identity
        from information_schema.columns as c
        inner join information_schema.tables as t on t.table_name = c.table_name
        where t.table_schema not in ('pg_catalog', 'information_schema')
        order by t.table_catalog, t.table_schema, t.table_name, c.column_name",
        connection);

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);

    // Iterate over the results
    var currentTable = new DatabaseObject();
    while (await reader.ReadAsync(cancellationToken))
    {
        var schemaName = reader.GetFieldValue<string?>(reader.GetOrdinal("table_schema"));
        var tableName = reader.GetFieldValue<string>(reader.GetOrdinal("table_name"));

        if (currentTable.Name != tableName || currentTable.Schema != schemaName)
        {
            var objectTypeName = reader.GetFieldValue<string>(reader.GetOrdinal("table_type"));

            // Setup a new table.
            currentTable = new()
            {
                Schema = schemaName,
                Name = tableName,
                ObjectTypeName = objectTypeName == "BASE TABLE" ? "TABLE" : objectTypeName,
            };

            tables.Add(currentTable);
        }

        // Add a new column
        var column = new DatabaseObjectColumn
        {
            Name = reader.GetFieldValue<string>(reader.GetOrdinal("column_name")),
        };

        // ... (data type / attribute handling omitted here for brevity — see PostgreSqlConnector.cs)

        currentTable.Fields.Add(column);
    }

    results.DatabaseObjects = tables;

    return results;
}
```

The important shape to notice: the reader is iterated once, in order, and a new `DatabaseObject` (table/view) is only appended to `tables` when the `(schema, table_name)` pair changes from the previous row — every row belonging to the same table just adds another `DatabaseObjectColumn` to `currentTable.Fields`. `DatabaseSchema`, `DatabaseObject`, and `DatabaseObjectColumn` live in `TheGrid.Connectors.Models`.

## Additional capability interfaces

Beyond `ISchemaDiscovery`, a connector can opt into two more capabilities by implementing additional interfaces from `TheGrid.Connectors`. As of this writing, implementing these records the capability on the connector's metadata (see below) for future UI/API use — there is no endpoint yet that actually invokes `TestConnectionAsync` or `HasWritePermissionAsync` against a live connection.

### IConnectionTest

```csharp
public interface IConnectionTest
{
    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
```

Implement this to let users verify a connection's parameters are valid before saving/using it. `PostgreSqlConnector`'s implementation simply opens the connection and runs a trivial query:

```csharp
/// <inheritdoc/>
public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
{
    await using var connection = GetConnection(ConnectorParameters);

    await connection.OpenAsync(cancellationToken);

    await using var command = new NpgsqlCommand("select 1", connection);

    return true;
}
```

### IPermissionTest

```csharp
public interface IPermissionTest
{
    public Task<bool> HasWritePermissionAsync(CancellationToken cancellationToken = default);
}
```

Implement this to let The Grid warn users when a connection's credentials have write access — connections used purely for querying/reporting should ideally be read-only. `PostgreSqlConnector`'s implementation checks `information_schema.tables` via PostgreSQL's `has_table_privilege` function:

```csharp
/// <inheritdoc/>
public async Task<bool> HasWritePermissionAsync(CancellationToken cancellationToken = default)
{
    // This should work for any version of PostgreSQL after 7.2
    const string permissionQuery =
        @"SELECT 
          table_name,
          has_table_privilege(quote_ident(table_name), 'INSERT') as has_insert_permission,
          has_table_privilege(quote_ident(table_name), 'UPDATE') as has_update_permission,
          has_table_privilege(quote_ident(table_name), 'DELETE') as has_delete_permission
        FROM information_schema.tables
        WHERE table_schema = current_schema";

    await using var connection = GetConnection(ConnectorParameters);

    await connection.OpenAsync(cancellationToken);

    await using var command = new NpgsqlCommand(permissionQuery, connection);

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);

    // Iterate over the results
    while (await reader.ReadAsync(cancellationToken))
    {
        if (reader.GetFieldValue<bool>(reader.GetOrdinal("has_insert_permission")) ||
            reader.GetFieldValue<bool>(reader.GetOrdinal("has_update_permission")) ||
            reader.GetFieldValue<bool>(reader.GetOrdinal("has_delete_permission")))
        {
            return true;
        }
    }

    return false;
}
```

Both interfaces are entirely optional — a minimal connector only needs `ConnectorBase` + `GetDataAsync`. Implement whichever capability interfaces make sense for your data source. `ConnectorDiscoveryService` reflects over each connector type at discovery time (`type.ImplementsInterface<ISchemaDiscovery>()`, `type.ImplementsInterface<IConnectionTest>()`) and records the result as `SupportsSchemaDiscovery`/`SupportsConnectionTest` flags on the connector's metadata — there is currently no equivalent discovery-time flag for `IPermissionTest`.
