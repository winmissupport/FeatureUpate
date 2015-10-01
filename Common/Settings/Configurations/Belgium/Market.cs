using ExigoService;
using System.Collections.Generic;

namespace Common
{
    public class BelgiumMarket : Market
    {
        public BelgiumMarket()
            : base()
        {
            Name = MarketName.Belgium;
            Description = "Belgium";
            CookieValue = "BE";
            CultureCode = "nl-BE ";
            Countries = new List<string> { "BE" };
        }

        public override IMarketConfiguration GetConfiguration()
        {
            return new BelgiumConfiguration();
        }
    }
}