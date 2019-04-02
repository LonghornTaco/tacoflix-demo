using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TacoFlix.Model;

namespace TacoFlix.ProcessingEngine.Extensions.Providers
{
    public class TheMovieDbMovieRecommendationsProvider : IMovieRecommendationsProvider
    {
        private readonly string _apiKey;
        private readonly ILogger<IMovieRecommendationsProvider> _logger;

        public TheMovieDbMovieRecommendationsProvider(IConfiguration configuration, ILogger<IMovieRecommendationsProvider> logger)
        {
            _logger = logger;
            _apiKey = configuration.GetValue<string>("theMovieDb:apiKey");
        }

        public List<Movie> GetRecommendations(long movieId)
        {
            var movies = new List<Movie>();

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var url = $"https://api.themoviedb.org/3/movie/{movieId}/recommendations?api_key={_apiKey}";

                    var response = httpClient.GetAsync(url).Result;
                    response.EnsureSuccessStatusCode();
                    var jsonData = response.Content.ReadAsStringAsync().Result;
                    var list = JsonConvert.DeserializeObject<MovieList>(jsonData);
                    movies.AddRange(list.Results);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError("There was an error in HttpClientWrapper.GetContent", ex);
                }
            }

            return movies;
        }
    }
}
