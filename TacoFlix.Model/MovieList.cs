using System;
using System.Collections.Generic;
using System.Text;

namespace TacoFlix.Model
{
    public class MovieList
    {
        public int Page { get; set; }
        public int TotalResults { get; set; }
        public int TotalPages { get; set; }
        public List<Movie> Results { get; set; }
    }
}
