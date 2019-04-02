using System;
using System.Collections.Generic;
using System.Text;

namespace TacoFlix.Model.Configuration
{
    public interface IXconnectConfiguration
    {
        string XconnectUrl { get; }
        string ContactSource { get; }
        string InteractionUserAgent { get; }
        string MovieRenterEraOutcomeId { get; }
        string MovieRentedOutcomeId { get; }
    }
}
