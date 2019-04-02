using System;
using System.Collections.Generic;
using System.Text;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;

namespace TacoFlix.Xconnect.Model
{
    public class MovieModel
    {
        private static XdbModel _model;

        public static XdbModel Model => _model ?? (_model = InitModel());

        private static XdbModel InitModel()
        {
            var builder = new XdbModelBuilder("Movie", new XdbModelVersion(1, 0));

            builder.ReferenceModel(CollectionModel.Model);
            builder.DefineEventType<MovieRentedOutcome>(true);
            builder.DefineFacet<Contact, MovieRecommendationFacet>(MovieRecommendationFacet.DefaultFacetName);

            return builder.BuildModel();
        }
    }
}
