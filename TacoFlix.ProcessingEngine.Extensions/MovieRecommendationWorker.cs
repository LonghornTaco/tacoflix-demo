using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Engine.Projection;
using Sitecore.Processing.Engine.Storage.Abstractions;
using TacoFlix.Model.Providers;
using TacoFlix.ProcessingEngine.Extensions.Providers;

namespace TacoFlix.ProcessingEngine.Extensions
{
    public class MovieRecommendationWorker : IDeferredWorker
    {
        public const string OptionSourceTableName = "sourceTableName";
        public const string OptionTargetTableName = "targetTableName";
        public const string OptionSchemaName = "schemaName";
        public const string OptionLimit = "limit";

        private readonly ITableStore _tableStore;
        private readonly IMovieRecommendationsProvider _movieRecommendationsProvider;
        private readonly string _sourceTableName;
        private readonly string _targetTableName;
        private readonly int _limit = 1;

        public MovieRecommendationWorker(ITableStoreFactory tableStoreFactory, IMovieRecommendationsProvider movieRecommendationsProvider, IReadOnlyDictionary<string, string> options)
        {
            _sourceTableName = options[OptionSourceTableName];
            _targetTableName = options[OptionTargetTableName];
            _limit = int.Parse(options[OptionLimit]);

            _movieRecommendationsProvider = movieRecommendationsProvider;

            var schemaName = options[OptionSchemaName];
            _tableStore = tableStoreFactory.Create(schemaName);
        }

        public void Dispose()
        {
            _tableStore.Dispose();
        }

        public async Task RunAsync(CancellationToken token)
        {
            var sourceRows = await _tableStore.GetRowsAsync(_sourceTableName, CancellationToken.None);
            var targetRows = new List<DataRow>();
            var targetSchema = new RowSchema(
                new FieldDefinition("ContactId", FieldKind.Key, FieldDataType.Guid),
                new FieldDefinition("MovieId", FieldKind.Key, FieldDataType.Int64),
                new FieldDefinition("Title", FieldKind.Attribute, FieldDataType.String),
                new FieldDefinition("Overview", FieldKind.Attribute, FieldDataType.String),
                new FieldDefinition("PosterPath", FieldKind.Attribute, FieldDataType.String)
            );

            while (await sourceRows.MoveNext())
            {
                foreach (var row in sourceRows.Current)
                {
                    var movieId = int.Parse(row["LastMovieId"].ToString());
                    var recommendations = _movieRecommendationsProvider.GetRecommendations(movieId);

                    if (recommendations.Any())
                    {
                        for (var i = 0; i < _limit; i++)
                        {
                            if (i < recommendations.Count)
                            {
                                var targetRow = new DataRow(targetSchema);
                                targetRow.SetGuid(0, row.GetGuid(0));
                                targetRow.SetInt64(1, recommendations[i].Id);
                                targetRow.SetString(2, recommendations[i].Title);
                                targetRow.SetString(3, recommendations[i].Overview);
                                targetRow.SetString(4, recommendations[i].PosterPath);

                                targetRows.Add(targetRow);
                            }
                        }
                    }
                }
            }

            var tableDefinition = new TableDefinition(_targetTableName, targetSchema);
            var targetTable = new InMemoryTableData(tableDefinition, targetRows);
            await _tableStore.PutTableAsync(targetTable, TimeSpan.FromMinutes(30), CancellationToken.None);
        }
    }
}
