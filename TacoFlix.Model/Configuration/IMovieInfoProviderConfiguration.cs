using System;
using System.Collections.Generic;
using System.Text;

namespace TacoFlix.Model.Configuration
{
    public interface IMovieInfoProviderConfiguration
    {
        string MovieDbApiKey { get; }
    }
}
