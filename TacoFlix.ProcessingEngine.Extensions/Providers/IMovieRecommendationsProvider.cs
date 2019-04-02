using System;
using System.Collections.Generic;
using System.Text;
using TacoFlix.Model;

namespace TacoFlix.ProcessingEngine.Extensions.Providers
{
    public interface IMovieRecommendationsProvider
    {
        List<Movie> GetRecommendations(long movieId);
    }
}
