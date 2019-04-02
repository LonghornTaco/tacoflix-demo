using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Engine.Storage.Abstractions;
using Sitecore.XConnect;
using TacoFlix.Model;
using TacoFlix.Xconnect.Model;

namespace TacoFlix.ProcessingEngine.Extensions
{
    public class RecommendationFacetStorageWorker : IDeferredWorker
    {
        public const string OptionTableName = "tableName";
        public const string OptionSchemaName = "schemaName";

        private readonly string _tableName;
        private readonly ITableStore _tableStore;
        private readonly IXdbContext _xdbContext;

        public RecommendationFacetStorageWorker(ITableStoreFactory tableStoreFactory, IXdbContext xdbContext, IReadOnlyDictionary<string, string> options)
        {
            _tableName = options[OptionTableName];
            var schemaName = options[OptionSchemaName];

            _tableStore = tableStoreFactory.Create(schemaName);

            _xdbContext = xdbContext;
        }

        public void Dispose()
        {
            _tableStore.Dispose();
        }

        public async Task RunAsync(CancellationToken token)
        {
            var rows = await _tableStore.GetRowsAsync(_tableName, CancellationToken.None);

            while (await rows.MoveNext())
            {
                foreach (var row in rows.Current)
                {
                    var contactId = row.GetGuid(0);
                    var movieId = row.GetInt64(1);
                    var title = row.GetString(2);
                    var overview = row.GetString(3);
                    var posterPath = row.GetString(4);

                    var contact = await _xdbContext.GetContactAsync(contactId,
                        new ContactExpandOptions(MovieRecommendationFacet.DefaultFacetName));

                    var facet = contact.GetFacet<MovieRecommendationFacet>(MovieRecommendationFacet.DefaultFacetName) ??
                                new MovieRecommendationFacet();

                    if (facet.MovieRecommendations.All(x => x.Id != movieId))
                    {
                        facet.MovieRecommendations.Add(new Movie
                        {
                            Id = movieId,
                            Title = title,
                            Overview = overview,
                            PosterPath = posterPath
                        });

                        _xdbContext.SetFacet(contact, MovieRecommendationFacet.DefaultFacetName, facet);
                        await _xdbContext.SubmitAsync(CancellationToken.None);
                    }
                }
            }

            await _tableStore.RemoveAsync(_tableName, CancellationToken.None);
        }
    }
}
