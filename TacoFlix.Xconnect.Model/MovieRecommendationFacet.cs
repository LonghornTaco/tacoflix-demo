using System;
using System.Collections.Generic;
using System.Text;
using Sitecore.XConnect;
using TacoFlix.Model;

namespace TacoFlix.Xconnect.Model
{
    public class MovieRecommendationFacet : Facet
    {
        public static string DefaultFacetName = "MovieRecommendationFacet";

        public List<Movie> MovieRecommendations { get; set; } = new List<Movie>();
    }
}
