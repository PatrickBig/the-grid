To add a database migration to the project with a terminal in the `source` directory run:

`dotnet ef migrations add {MigrationName} --project TheGrid.Postgres\TheGrid.Postgres.csproj -- {ProviderName} {ConnectionString}`

**{ProviderName}** can be:
* postgresql

The **{ConnectionString}** must be a valid connection string for the provider.