// <copyright file="MongoDbConnector.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using TheGrid.Shared.Models;

namespace TheGrid.Connectors
{
    /// <summary>
    /// Executes MongoDB queries. A query's <c>Command</c> is a JSON object identifying a target
    /// <c>collection</c> plus either a <c>query</c> (find mode) or an <c>aggregate</c> pipeline
    /// (aggregate mode). See <c>openspec/changes/add-mongodb-connector/design.md</c> for the full
    /// contract.
    /// </summary>
    /// <param name="context">Shared infrastructure and connection properties used to initiate the connection to the MongoDB deployment.</param>
    [Connector("MongoDB", IconFileName = "mongodb.png")]
    [ConnectorParameter(CommonConnectionParameters.ConnectionString, "Connection String", ConnectionPropertyType.SingleLineText, Required = true, HelpText = "Standard MongoDB connection string, e.g. mongodb://user:password@host:27017.")]
    [ConnectorParameter(CommonConnectionParameters.Database, "Database", ConnectionPropertyType.SingleLineText, HelpText = "Default database used when a query's command JSON does not specify a 'db' key.")]
    [ConnectorParameter(MongoDbConnector.SchemaSampleSizeParameterKey, "Schema Sample Size", ConnectionPropertyType.Numeric, HelpText = "Number of documents sampled per collection ($sample) when discovering schema. Defaults to 100.")]
    public class MongoDbConnector(ConnectorContext context) : ConnectorBase(context), ISchemaDiscovery, IConnectionTest
    {
        /// <summary>
        /// Connector parameter key for the tunable schema-discovery sample size.
        /// </summary>
        public const string SchemaSampleSizeParameterKey = "schemaSampleSize";

        private const int DefaultSchemaSampleSize = 100;

        /// <inheritdoc/>
        public override async IAsyncEnumerable<ConnectorRow> GetDataAsync(string query, Dictionary<string, object?>? queryParameters, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var command = ParseCommand(query);
            var client = GetClient();
            var database = client.GetDatabase(ResolveQueryDatabaseName(command));
            var collection = database.GetCollection<BsonDocument>(command.Collection);

            if (command.Count)
            {
                var count = await GetCountAsync(collection, command, cancellationToken);

                var countColumns = new Dictionary<string, QueryResultColumn>
                {
                    { "count", new QueryResultColumn { Type = QueryResultColumnType.Long } },
                };

                yield return new ConnectorRow(countColumns, new Dictionary<string, object?> { { "count", count } });

                yield break;
            }

            Dictionary<string, QueryResultColumn>? columns = null;

            if (command.Aggregate != null)
            {
                var pipeline = BuildPipeline(command.Aggregate);
                var aggregateOptions = new AggregateOptions { AllowDiskUse = command.AllowDiskUse };

                using var cursor = await collection.AggregateAsync(pipeline, aggregateOptions, cancellationToken);

                while (await cursor.MoveNextAsync(cancellationToken))
                {
                    foreach (var document in cursor.Current)
                    {
                        columns ??= GetColumns(document);
                        yield return new ConnectorRow(columns, GetRowData(document));
                    }
                }
            }
            else
            {
                FilterDefinition<BsonDocument> filter = command.Query ?? new BsonDocument();
                var findFluent = collection.Find(filter);

                if (command.Projection != null)
                {
                    ProjectionDefinition<BsonDocument, BsonDocument> projection = command.Projection;
                    findFluent = findFluent.Project<BsonDocument>(projection);
                }

                if (command.Sort != null)
                {
                    SortDefinition<BsonDocument> sort = command.Sort;
                    findFluent = findFluent.Sort(sort);
                }

                if (command.Skip is int skip)
                {
                    findFluent = findFluent.Skip(skip);
                }

                if (command.Limit is int limit)
                {
                    findFluent = findFluent.Limit(limit);
                }

                using var cursor = await findFluent.ToCursorAsync(cancellationToken);

                while (await cursor.MoveNextAsync(cancellationToken))
                {
                    foreach (var document in cursor.Current)
                    {
                        columns ??= GetColumns(document);
                        yield return new ConnectorRow(columns, GetRowData(document));
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            var client = GetClient();
            var databaseName = ResolveDefaultDatabaseName();
            var database = client.GetDatabase(databaseName);
            var sampleSize = GetSchemaSampleSize();

            using var namesCursor = await database.ListCollectionNamesAsync(cancellationToken: cancellationToken);

            var collectionNames = new List<string>();
            while (await namesCursor.MoveNextAsync(cancellationToken))
            {
                collectionNames.AddRange(namesCursor.Current);
            }

            var databaseObjects = new List<DatabaseObject>();

            foreach (var collectionName in collectionNames)
            {
                var collection = database.GetCollection<BsonDocument>(collectionName);
                var sample = await SampleCollectionAsync(collection, sampleSize, cancellationToken);

                databaseObjects.Add(BuildDatabaseObject(collectionName, sample));
            }

            return new DatabaseSchema
            {
                DatabaseName = databaseName,
                DatabaseObjects = databaseObjects,
            };
        }

        /// <inheritdoc/>
        public async Task<ConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var client = GetClient();

                var databaseName = ConnectorParameters.TryGetValue(CommonConnectionParameters.Database, out var configuredDatabase) && !string.IsNullOrWhiteSpace(configuredDatabase)
                    ? configuredDatabase
                    : "admin";

                var database = client.GetDatabase(databaseName);

                await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);

                return new ConnectionTestResult(true, null, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                return new ConnectionTestResult(false, ex.Message, stopwatch.Elapsed);
            }
        }

        private static async Task<List<BsonDocument>> SampleCollectionAsync(IMongoCollection<BsonDocument> collection, int sampleSize, CancellationToken cancellationToken)
        {
            var pipeline = PipelineDefinition<BsonDocument, BsonDocument>.Create(new[]
            {
                new BsonDocument("$sample", new BsonDocument("size", sampleSize)),
            });

            using var cursor = await collection.AggregateAsync(pipeline, cancellationToken: cancellationToken);

            var sample = new List<BsonDocument>();
            while (await cursor.MoveNextAsync(cancellationToken))
            {
                sample.AddRange(cursor.Current);
            }

            return sample;
        }

        private static DatabaseObject BuildDatabaseObject(string collectionName, IReadOnlyCollection<BsonDocument> sample)
        {
            var databaseObject = new DatabaseObject
            {
                Name = collectionName,
                ObjectTypeName = "Collection",
            };

            var fieldOrder = new List<string>();
            var fieldTypes = new Dictionary<string, HashSet<BsonType>>();
            var fieldPresence = new Dictionary<string, int>();

            foreach (var document in sample)
            {
                foreach (var element in document.Elements)
                {
                    if (!fieldTypes.TryGetValue(element.Name, out var observedTypes))
                    {
                        observedTypes = new HashSet<BsonType>();
                        fieldTypes[element.Name] = observedTypes;
                        fieldOrder.Add(element.Name);
                    }

                    observedTypes.Add(element.Value.BsonType);
                    fieldPresence[element.Name] = fieldPresence.GetValueOrDefault(element.Name) + 1;
                }
            }

            foreach (var fieldName in fieldOrder)
            {
                var observedTypes = fieldTypes[fieldName];

                var column = new DatabaseObjectColumn
                {
                    Name = fieldName,
                };

                if (observedTypes.Count == 1)
                {
                    column.TypeName = GetBsonTypeName(observedTypes.Single());
                }
                else
                {
                    column.TypeName = "Mixed";
                    column.Attributes["ObservedTypes"] = string.Join(", ", observedTypes.Select(GetBsonTypeName));
                }

                column.Attributes["Presence"] = $"{fieldPresence[fieldName]}/{sample.Count}";

                databaseObject.Fields.Add(column);
            }

            return databaseObject;
        }

        private static string GetBsonTypeName(BsonType type)
        {
            return type switch
            {
                BsonType.Document => "Object",
                BsonType.Array => "Array",
                _ => type.ToString(),
            };
        }

        private static async Task<long> GetCountAsync(IMongoCollection<BsonDocument> collection, MongoQueryCommand command, CancellationToken cancellationToken)
        {
            if (command.Aggregate != null)
            {
                var stages = command.Aggregate.Select(s => s.AsBsonDocument).Append(new BsonDocument("$count", "count"));
                var pipeline = PipelineDefinition<BsonDocument, BsonDocument>.Create(stages);

                using var cursor = await collection.AggregateAsync(pipeline, cancellationToken: cancellationToken);

                if (await cursor.MoveNextAsync(cancellationToken))
                {
                    var result = cursor.Current.FirstOrDefault();
                    if (result != null && result.TryGetValue("count", out var countValue))
                    {
                        return countValue.ToInt64();
                    }
                }

                return 0;
            }

            return await collection.CountDocumentsAsync(command.Query ?? new BsonDocument(), cancellationToken: cancellationToken);
        }

        private static PipelineDefinition<BsonDocument, BsonDocument> BuildPipeline(BsonArray stages)
        {
            return PipelineDefinition<BsonDocument, BsonDocument>.Create(stages.Select(s => s.AsBsonDocument));
        }

        private static Dictionary<string, QueryResultColumn> GetColumns(BsonDocument document)
        {
            var columns = new Dictionary<string, QueryResultColumn>();

            foreach (var element in document.Elements)
            {
                columns[element.Name] = new QueryResultColumn { Type = GetColumnType(element.Value) };
            }

            return columns;
        }

        private static Dictionary<string, object?> GetRowData(BsonDocument document)
        {
            var row = new Dictionary<string, object?>();

            foreach (var element in document.Elements)
            {
                row[element.Name] = ConvertBsonValue(element.Value);
            }

            return row;
        }

        private static QueryResultColumnType GetColumnType(BsonValue value)
        {
            return value.BsonType switch
            {
                BsonType.Document => QueryResultColumnType.Json,
                BsonType.Array => QueryResultColumnType.Json,
                BsonType.String => QueryResultColumnType.Text,
                BsonType.Boolean => QueryResultColumnType.Boolean,
                BsonType.Int32 => QueryResultColumnType.Integer,
                BsonType.Int64 => QueryResultColumnType.Long,
                BsonType.Double => QueryResultColumnType.Decimal,
                BsonType.Decimal128 => QueryResultColumnType.Decimal,
                BsonType.DateTime => QueryResultColumnType.DateTime,
                BsonType.ObjectId => QueryResultColumnType.Text,
                BsonType.Binary => QueryResultColumnType.Binary,
                _ => QueryResultColumnType.Unknown,
            };
        }

        private static object? ConvertBsonValue(BsonValue value)
        {
            if (value.IsBsonNull)
            {
                return null;
            }

            return value.BsonType switch
            {
                BsonType.Document or BsonType.Array => ToJsonElement(value),
                BsonType.String => value.AsString,
                BsonType.Boolean => value.AsBoolean,
                BsonType.Int32 => value.AsInt32,
                BsonType.Int64 => value.AsInt64,
                BsonType.Double => value.AsDouble,
                BsonType.Decimal128 => (decimal)value.AsDecimal128,
                BsonType.DateTime => value.ToUniversalTime(),
                BsonType.ObjectId => value.AsObjectId.ToString(),
                BsonType.Binary => value.AsBsonBinaryData.Bytes,
                _ => value.ToString(),
            };
        }

        private static JsonElement ToJsonElement(BsonValue value)
        {
            var json = value.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson });

            using var document = JsonDocument.Parse(json);

            return document.RootElement.Clone();
        }

        private static MongoQueryCommand ParseCommand(string commandJson)
        {
            if (string.IsNullOrWhiteSpace(commandJson))
            {
                throw new InvalidOperationException("The query command must not be empty.");
            }

            BsonDocument root;

            try
            {
                root = BsonDocument.Parse(commandJson);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new InvalidOperationException("The query command must be valid JSON.", ex);
            }

            if (!root.TryGetValue("collection", out var collectionValue) || !collectionValue.IsString)
            {
                throw new InvalidOperationException("The query command must specify a 'collection' string.");
            }

            return new MongoQueryCommand
            {
                Collection = collectionValue.AsString,
                Query = GetDocumentOrDefault(root, "query"),
                Aggregate = GetArrayOrDefault(root, "aggregate"),
                Projection = GetDocumentOrDefault(root, "projection"),
                Sort = GetDocumentOrDefault(root, "sort"),
                Skip = GetInt32OrDefault(root, "skip"),
                Limit = GetInt32OrDefault(root, "limit"),
                Count = GetBooleanOrDefault(root, "count"),
                Database = GetStringOrDefault(root, "db"),
                AllowDiskUse = GetBooleanOrDefault(root, "allowDiskUse"),
            };
        }

        private static BsonDocument? GetDocumentOrDefault(BsonDocument root, string key)
        {
            return root.TryGetValue(key, out var value) && value.IsBsonDocument ? value.AsBsonDocument : null;
        }

        private static BsonArray? GetArrayOrDefault(BsonDocument root, string key)
        {
            return root.TryGetValue(key, out var value) && value.IsBsonArray ? value.AsBsonArray : null;
        }

        private static int? GetInt32OrDefault(BsonDocument root, string key)
        {
            return root.TryGetValue(key, out var value) && value.IsNumeric ? value.ToInt32() : null;
        }

        private static bool GetBooleanOrDefault(BsonDocument root, string key)
        {
            return root.TryGetValue(key, out var value) && value.ToBoolean();
        }

        private static string? GetStringOrDefault(BsonDocument root, string key)
        {
            return root.TryGetValue(key, out var value) && value.IsString ? value.AsString : null;
        }

        private MongoClient GetClient()
        {
            return new MongoClient(ConnectorParameters[CommonConnectionParameters.ConnectionString]);
        }

        private int GetSchemaSampleSize()
        {
            if (ConnectorParameters.TryGetValue(SchemaSampleSizeParameterKey, out var value) && int.TryParse(value, out var size) && size > 0)
            {
                return size;
            }

            return DefaultSchemaSampleSize;
        }

        private string ResolveDefaultDatabaseName()
        {
            if (ConnectorParameters.TryGetValue(CommonConnectionParameters.Database, out var databaseName) && !string.IsNullOrWhiteSpace(databaseName))
            {
                return databaseName;
            }

            throw new InvalidOperationException("Schema discovery requires the connection's 'Database' parameter to be set.");
        }

        private string ResolveQueryDatabaseName(MongoQueryCommand command)
        {
            if (!string.IsNullOrWhiteSpace(command.Database))
            {
                return command.Database;
            }

            if (ConnectorParameters.TryGetValue(CommonConnectionParameters.Database, out var defaultDatabase) && !string.IsNullOrWhiteSpace(defaultDatabase))
            {
                return defaultDatabase;
            }

            throw new InvalidOperationException("No database was specified. Set a default 'Database' connection parameter or include a 'db' key in the query command.");
        }

        /// <summary>
        /// Parsed representation of a MongoDB connector <c>Query.Command</c> JSON document.
        /// </summary>
        private sealed class MongoQueryCommand
        {
            /// <summary>
            /// Target collection name.
            /// </summary>
            public required string Collection { get; init; }

            /// <summary>
            /// Find-mode filter document.
            /// </summary>
            public BsonDocument? Query { get; init; }

            /// <summary>
            /// Aggregate-mode pipeline stages. When present, selects aggregate mode over find mode.
            /// </summary>
            public BsonArray? Aggregate { get; init; }

            /// <summary>
            /// Find-mode projection document.
            /// </summary>
            public BsonDocument? Projection { get; init; }

            /// <summary>
            /// Find-mode sort document, in native MongoDB sort-object syntax.
            /// </summary>
            public BsonDocument? Sort { get; init; }

            /// <summary>
            /// Find-mode number of documents to skip.
            /// </summary>
            public int? Skip { get; init; }

            /// <summary>
            /// Find-mode maximum number of documents to return.
            /// </summary>
            public int? Limit { get; init; }

            /// <summary>
            /// When true, a count is returned instead of documents.
            /// </summary>
            public bool Count { get; init; }

            /// <summary>
            /// Per-query database override. When absent, the connector's default <c>Database</c> parameter is used.
            /// </summary>
            public string? Database { get; init; }

            /// <summary>
            /// Aggregate-mode <c>allowDiskUse</c> flag.
            /// </summary>
            public bool AllowDiskUse { get; init; }
        }
    }
}
