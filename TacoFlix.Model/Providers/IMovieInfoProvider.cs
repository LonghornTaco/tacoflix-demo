using System;
using System.Collections.Generic;
using System.Text;

namespace TacoFlix.Model.Providers
{
    public interface IMovieInfoProvider
    {
        List<Genre> GetGenres();
        List<Movie> GetPopularMovies();
    }
}
