To add a database migration to the project with a terminal in the `source` directory run:

`dotnet ef migrations add {MigrationName} --project TheGrid.Postgres\TheGrid.Postgres.csproj --startup-project TheGrid.Server\TheGrid.Server.csproj -- {ProviderName} {ConnectionString}`

**{ProviderName}** can be:
* postgresql
* sqlite

The **{ConnectionString}** must be a valid connection string for the provider.

To generate a migration for the Sqlite provider instead, target `TheGrid.Sqlite\TheGrid.Sqlite.csproj` and set `SystemOptions__DatabaseProvider=Sqlite` in the environment (the design-time context factory selects the provider from `SystemOptions:DatabaseProvider` in configuration):

`dotnet ef migrations add {MigrationName} --project TheGrid.Sqlite\TheGrid.Sqlite.csproj --startup-project TheGrid.Server\TheGrid.Server.csproj -- sqlite {ConnectionString}`