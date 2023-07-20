# Creating a new query runner

Adding support for new query runners can be done in the `TheGrid.QueryRunners` project.



## Getting started

The `TheGrid.QueryRunners` includes a file named `GlobalUsings.cs` which include most of the required attributes and base classes needed to create new runners.

All query runners must meet the following minimum criteria:

* Inherit from `TheGrid.QueryRunners.QueryRunnerBase`
* Apply a `TheGrid.QueryRunners.Attributes.QueryRunnerAttribute`




### Define the runner

For this exercise we will create an example runner that will produce some results by connect to a fictional database engine, run a query against it and return the results.


First lets setup our base class and inherit from `QueryRunnerBase`.

```csharp
using DatabaseProvider.Driver;

namespace TheGrid.QueryRunners
{
    /// <summary>
    /// Connects to a MyDatabase instance and runs a query against the specified datasource.
    /// </summary>
    [QueryRunner("My Database")]
    public class MyDatabaseRunner : QueryRunnerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MyDatabaseRunner"/> class.
        /// </summary>
        /// <param name="runnerParameters">Properties used to connect to the file as a source.</param>
        public MyDatabaseRunner(Dictionary<string, string> runnerParameters)
            : base(runnerParameters)
        {
        }

        /// <summary>
        /// Runs a query using the runner properties.
        /// </summary>
        /// <param name="query">Query to be executed.</param>
        /// <param name="queryParameters">Parameters to pass to the query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Results from the execution of the query.</returns>
        public override async Task<QueryResult> RunQueryAsync(string query, Dictionary<string, object>? queryParameters, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
```

This is the minimum level of code needed to define a new runner. Since we throw a `NotImplementedException` it will just error out when it is used.

Breaking this down we have three elements.

#### The QueryRunner attribute

The `QueryRunner` attribute applied to the `MyDatabaseRunner` class is used to define metadata about the runner and can be useful for hints in the UI for users when editing queries or defining the data source.

```csharp
[QueryRunner("My Text File")]
```

**QueryRunnerAttribute properties**
|Attribute property |Description                                       |Required |
|-------------------|--------------------------------------------------|---------|
|`Name`             |Display name for the query runner.                |Yes      |
|`EditorLanguage`   |Language used by the IDE / editor component.      |No       |
|`IconFileName`     |Icon used in the user interface for the runner.   |No       |

#### The constructor

All classes must use and call the constructor in the base class. The `runnerParameters` will contain all of the required information defined in your data source for connecting to whatever it uses to produce data. For most database sources this might include a connection string, authentication information, etc.

```csharp
public MyTextFileRunner(Dictionary<string, string> runnerParameters)
    : base(runnerParameters)
{
}
```

#### The RunQueryAsync method

The main functionality for implementing your runner will be in the `RunQueryAsync` method.

This is where you would write the code to actually connect to your data source, read the results, and provide the output.
```csharp
public override async Task<QueryResult> RunQueryAsync(string query, Dictionary<string, object>? queryParameters, CancellationToken cancellationToken = default)
```

It is recommended to use `async` methods whenever possible, and pass the cancellation token where appropriate.

The `queryParameters` provides any additional parameters posted to the query by the execution engine.

This could be used for running [parameterized queries](https://learn.microsoft.com/en-us/aspnet/web-forms/overview/data-access/accessing-the-database-directly-from-an-aspnet-page/using-parameterized-queries-with-the-sqldatasource-cs) when supported by your database driver.


### Add functionality to produce data

Since our example runner doesn't really do much besides throw an exception lets mock up some functionality.

In order to do this we need to build our `QueryResult` object to return.

|Property Name  |Type                               |Description                                                      |
|---------------|-----------------------------------|-----------------------------------------------------------------|
|Columns        |`List<string>`                     |Provides a list of column names available in the result set.     |
|Rows           |`List<Dictionary<string, object>>` |A list of key/values for each column. Each list item is a row with each key/value pair representing the column name and the corresponding value.|

```csharp
[QueryRunner("My Database")]
public class MyDatabaseRunner : QueryRunnerBase
{
    // ... constructor

    /// <inheritdoc />
    public override async Task<QueryResult> RunQueryAsync(string query, Dictionary<string, object>? queryParameters, CancellationToken cancellationToken = default)
    {
        // Build our connection
        using var connection = new MyDatabaseConnection("host=localhost;port=1234;database=test_db;");

        await connection.OpenAsync(cancellationToken);

        bool firstReadDone = false;
        var results = new QueryResult();

        await using (var command = new MyDatabaseCommand(query, connection))
        {
            if (queryParameters != null && queryParameters.Any())
            {
                foreach (var parameter in queryParameters)
                {
                    command.Parameters.Add(parameter.Key, parameter.Value);
                }
            }

            await using var reader = await command.ExecuteQueryAsync(cancellationToken);

            // Iterate over the results and add them to the results
            while (await reader.ReadStreamAsync(cancellationToken))
            {
                if (!firstReadDone)
                {
                    results.Columns = reader.Fields.Select(f => f.Name).ToList();
                    firstReadDone = true;
                }

                // Create each row
                var row = new Dictionary<string, object>();

                foreach (var fieldName in results.Columns)
                {
                    row.Add(fieldName, reader.GetValue(fieldName));
                }

                results.Add(row);
            }
        }

        return results;
    }
}
```


Now our query runner can execute a query and return the results.


### Use runner parameters to provide connection information

In our last step you might notice that it we have a connection string hard coded into the runner. This is really only helpful to our users if the database will always have the same host.

We can make use of the `QueryRunnerParameterAttribute` to let the query engine know what parameters are available for the data source.

Some common ones for a typical relational database might be **Connection String**, **Database**, **Username**, and **Password**.

It is recommended to split authentication parameters (especially passwords) from the connection string. This way it can be hidden from users who are viewing the data source.

**QueryRunnerParameterAttribute properties**
|Attribute property |Description                                                                                       |Required |
|-------------------|--------------------------------------------------------------------------------------------------|---------|
|`Name`             |Name for parameter.                                                                               |Yes      |
|`Type`             |Input type used in the UI for the parameter.                                                      |Yes      |
|`RenderOrder`      |Used when rendering the UI to show the items in a specific order if desired.                      |No       |
|`HelpText`         |Short text for the property to display to the user. Supports markdown, limited to 200 characters. |No       |
|`Required`         |Set to true if the user must supply a value when setting up the data source using this runner.    |No       |

The `Type` property is an enum with the following values

**QueryRunnerParameterType** values
|Enum Value                  |Description                                                       |
|----------------------------|------------------------------------------------------------------|
|`SingleLineText`            |Single line of text for input.                                    |
|`MultipleLineText`          |Multiple lines of text for input.                                 |
|`ProtectedText`             |Password type input, users will not be able to view this content. |
|`Numeric`                   |Numeric input allowed only.                                       |
|`Boolean`                   |Checkbox yes/no style input.                                      |


Lets add our attributes to the `MyDatabaseRunner` class so we can accept the common **Connection String**, **Database**, **Username**, and **Password** parameters.

For the `Name` property on the `QueryRunnerParameterAttribute` you can either enter a string, or make use of the `CommonConnectionParameters` class which contain some constants for commonly used connection properties.

```csharp
[QueryRunner("My Database")]
[QueryRunnerParameter(CommonConnectionParameters.ConnectionString, QueryRunnerParameterType.SingleLineText, Required = true)]
[QueryRunnerParameter(CommonConnectionParameters.Username, QueryRunnerParameterType.SingleLineText, Required = true)]
[QueryRunnerParameter(CommonConnectionParameters.Password, QueryRunnerParameterType.SingleLineText, Required = true)]
public class MyDatabaseRunner : QueryRunnerBase
{
}
```

In order to make use of these parameters in the `RunQueryAsync` method you can consume the `RunnerParameters` property, which is a protected property on the `QueryRunnerBase` class.


Lets build our connection string using these parameters.


```csharp
[QueryRunner("My Database")]
[QueryRunnerParameter(CommonConnectionParameters.ConnectionString, QueryRunnerParameterType.SingleLineText, Required = true)]
[QueryRunnerParameter(CommonConnectionParameters.Username, QueryRunnerParameterType.SingleLineText, Required = true)]
[QueryRunnerParameter(CommonConnectionParameters.Password, QueryRunnerParameterType.SingleLineText, Required = true)]
public class MyDatabaseRunner : QueryRunnerBase
{
    // ... constructor

    /// <inheritdoc />
    public override async Task<QueryResult> RunQueryAsync(string query, Dictionary<string, object>? queryParameters, CancellationToken cancellationToken = default)
    {
        var connection = GetConnection();

        // ... implementation goes here.

        return results;
    }

    private MyDatabaseConnection GetConnection()
    {
        var connectionStringBuilder = new MyDatabaseConnectionStringBuilder(RunnerParameters[CommonConnectionParameters.ConnectionString]);

        // If there is a username or password, try using those to update the settings.
        if (properties.TryGetValue(CommonConnectionParameters.Password, out string? password))
        {
            builder.Password = password;
        }

        if (properties.TryGetValue(CommonConnectionParameters.Username, out string? username))
        {
            builder.Username = username;
        }

        return new MyDatabaseConnection(connectionStringBuilder.ConnectionString);
    }
}
```

# Adding database schema discovery support

If your query runner connects to a database it might have a defined schema. For things like SQL database providers this would be things like tables, fields/columns, and attributes for those objects.

For the query runner framework to know your runner supports discoverying database schema you should implement the `ISchemaDiscovery` interface.

This will introduce the `public async Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default)` method.

The `GetSchemaAsync` method should return a `DatabaseSchema` object.

```csharp
[QueryRunner("My Database")]
[QueryRunnerParameter(CommonConnectionParameters.ConnectionString, QueryRunnerParameterType.SingleLineText, Required = true)]
[QueryRunnerParameter(CommonConnectionParameters.Username, QueryRunnerParameterType.SingleLineText, Required = true)]
[QueryRunnerParameter(CommonConnectionParameters.Password, QueryRunnerParameterType.SingleLineText, Required = true)]
public class MyDatabaseRunner : QueryRunnerBase, ISchemaDiscovery
{
    /// ... implementation

    public async Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
    {
        var connection = GetConnection();

        await connection.OpenAsync(cancellationToken);

        var result = new DatabaseShema();
        
        var tables = new List<DatabaseObject>();

        await using (var command = new MyDatabaseCommand("SELECT name, type FROM system.schema WHERE type = 'TABLE' OR type = 'VIEW'", connection))
        {
            await using var reader = await command.ExecuteQueryAsync(cancellationToken);

            while (await reader.ReadStreamAsync(cancellationToken))
            {
                var table = new DatabaseObject
                {
                    Name = reader.GetValue("name"),
                    ObjectTypeName = reader.GetValue("type"),
                };

                // You can also get fields/columns and add them to the DatabaseObject
                table.Fields = GetFieldsForTable(tableName);

                tables.Add(table);
            }
        }

        result.DatabaseObjects = tables;

        return result;
    }
}
```
