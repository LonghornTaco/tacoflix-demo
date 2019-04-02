using System;
using System.Collections.Generic;
using System.Text;
using Sitecore.Processing.Engine.Abstractions;

namespace TacoFlix.ProcessingEngine.Extensions
{
    public class MovieRecommendationWorkerOptionsDictionary : DeferredWorkerOptionsDictionary
    {
        public MovieRecommendationWorkerOptionsDictionary() : base(
            typeof(MovieRecommendationWorker).AssemblyQualifiedName, // workerType
            new Dictionary<string, string> // options
            {
                { MovieRecommendationWorker.OptionSourceTableName, "contactMoviesFinal" },
                { MovieRecommendationWorker.OptionTargetTableName, "contactRecommendations" },
                { MovieRecommendationWorker.OptionSchemaName, "recommendation" },
                { MovieRecommendationWorker.OptionLimit, "3" }
            })
        {
            
        }
    }
}
