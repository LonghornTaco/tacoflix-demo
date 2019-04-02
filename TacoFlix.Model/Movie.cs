using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TacoFlix.Model
{
    public class Movie
    {
        public long Id { get; set; }
        public string Title { get; set; }
        [JsonProperty("poster_path")]
        public string PosterPath { get; set; }
        public string Overview { get; set; }
    }
}
