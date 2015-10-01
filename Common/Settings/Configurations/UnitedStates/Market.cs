using ExigoService;
using System.Collections.Generic;

namespace Common
{
    public class UnitedStatesMarket : Market
    {
        public UnitedStatesMarket()
            : base()
        {
            Name        = MarketName.UnitedStates;
            Description = "United States";
            CookieValue = "US";
            CultureCode = "en-US";
            IsDefault   = true;
            Countries   = new List<string> { "US" };
        }

        public override IMarketConfiguration GetConfiguration()
        {
            return new UnitedStatesConfiguration();
        }
    }
}