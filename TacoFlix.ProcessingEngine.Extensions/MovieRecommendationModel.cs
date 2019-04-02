using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sitecore.Processing.Engine.ML.Abstractions;
using Sitecore.Processing.Engine.Projection;
using Sitecore.XConnect;
using TacoFlix.Xconnect.Model;
using System.Linq;

namespace TacoFlix.ProcessingEngine.Extensions
{
    public class MovieRecommendationModel : IModel<Contact>
    {
        public const string OptionTableName = "tableName";
        private readonly string _tableName;

        public MovieRecommendationModel(IReadOnlyDictionary<string, string> options)
        {
            _tableName = options[OptionTableName];
        }
        
        public IProjection<Contact> Projection =>
            Sitecore.Processing.Engine.Projection.Projection.Of<Contact>().CreateTabular(
                _tableName,
                contact =>
                    contact.Interactions.Select(interaction =>
                        new
                        {
                            Contact = contact,
                            LastMovie = interaction.Events.OfType<MovieRentedOutcome>().Last().Movie
                        }
                    ).Last(),
                cfg => cfg
                    .Key("ContactId", x => x.Contact.Id)
                    .Attribute("LastMovieId", x => x.LastMovie.Id)
                // key, attribute, measure
            );

        public Task<ModelStatistics> TrainAsync(string schemaName, CancellationToken cancellationToken, params TableDefinition[] tables)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<object>> EvaluateAsync(string schemaName, CancellationToken cancellationToken, params TableDefinition[] tables)
        {
            throw new NotImplementedException();
        }
    }
}
