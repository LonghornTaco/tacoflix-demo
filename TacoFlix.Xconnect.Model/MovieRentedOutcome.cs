using System;
using System.Collections.Generic;
using System.Text;
using Sitecore.XConnect;
using TacoFlix.Model;

namespace TacoFlix.Xconnect.Model
{
    public class MovieRentedOutcome : Outcome
    {
        public Movie Movie { get; set; }

        public MovieRentedOutcome(Guid definitionId, DateTime timestamp, string currencyCode, decimal monetaryValue)
            : base(definitionId, timestamp, currencyCode, monetaryValue)
        {
        }
    }
}
