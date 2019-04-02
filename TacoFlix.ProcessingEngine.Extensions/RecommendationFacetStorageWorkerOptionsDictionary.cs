using System;
using System.Collections.Generic;
using System.Text;
using Sitecore.Processing.Engine.Abstractions;

namespace TacoFlix.ProcessingEngine.Extensions
{
    public class RecommendationFacetStorageWorkerOptionsDictionary : DeferredWorkerOptionsDictionary
    {
        public RecommendationFacetStorageWorkerOptionsDictionary() : base(
            typeof(RecommendationFacetStorageWorker).AssemblyQualifiedName, // workerType
            new Dictionary<string, string> // options
            {
                { RecommendationFacetStorageWorker.OptionTableName, "contactRecommendations" },
                { RecommendationFacetStorageWorker.OptionSchemaName, "recommendation" }
            })
        {
            
        }
    }
}
