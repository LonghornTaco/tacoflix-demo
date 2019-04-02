using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using TacoFlix.Model;
using TacoFlix.Model.Configuration;
using TacoFlix.Model.Logging;
using TacoFlix.Model.Providers;

namespace TacoFlix.Providers.TheMovieDb
{
    public class TheMovieDbMovieInfoProvider : IMovieInfoProvider
    {
        private readonly IMovieInfoProviderConfiguration _movieInfoProviderConfiguration;
        private readonly ILogger _logger;

        private List<Movie> _popularMovies;
        private string _movieJsonData = "App_Data\\movies.json";

        public TheMovieDbMovieInfoProvider(IMovieInfoProviderConfiguration movieInfoProviderConfiguration, ILogger logger)
        {
            _movieInfoProviderConfiguration = movieInfoProviderConfiguration;
            _logger = logger;

            if (File.Exists(_movieJsonData))
            {
                _logger.Info("Loading movies from cache...");
                var jsonData = File.ReadAllText(_movieJsonData);
                _popularMovies = new List<Movie>();
                ConvertMovies(jsonData);
            }
        }

        public List<Genre> GetGenres()
        {
            var jsonData = CallApi("/genre/movie/list");
            var list = JsonConvert.DeserializeObject<GenreList>(jsonData);
            return list.Genres;
        }

        public List<Movie> GetPopularMovies()
        {
            if (_popularMovies == null)
            {
                _logger.Info($"Calling TheMovieDb.org to Loading movies...");
                _popularMovies = new List<Movie>();
                for (var i = 1; i <= 10; i++)
                {
                    var jsonData = CallApi("/movie/popular", i);
                    ConvertMovies(jsonData);
                }

                var cachedData = JsonConvert.SerializeObject(new MovieList() { Results = _popularMovies});
                File.WriteAllText(_movieJsonData, cachedData);
                _logger.Info($"Loading movies complete!");
            }

            return _popularMovies;
        }

        private string CallApi(string resource, int page = 0)
        {
            var content = string.Empty;

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var url = $"https://api.themoviedb.org/3{resource}?api_key={_movieInfoProviderConfiguration.MovieDbApiKey}";
                    if (page > 0)
                    {
                        url = $"{url}&page={page}";
                    }

                    var response = httpClient.GetAsync(url).Result;
                    response.EnsureSuccessStatusCode();
                    content = response.Content.ReadAsStringAsync().Result;
                }
                catch (HttpRequestException ex)
                {
                    _logger.Error("There was an error in HttpClientWrapper.GetContent", ex);
                    throw;
                }
            }

            return content;
        }

        private void ConvertMovies(string jsonData)
        {
            var list = JsonConvert.DeserializeObject<MovieList>(jsonData);
            _popularMovies.AddRange(list.Results);
            list.Results.ForEach(x => _logger.Info($"Loaded {x.Title}"));
        }
    }
}
