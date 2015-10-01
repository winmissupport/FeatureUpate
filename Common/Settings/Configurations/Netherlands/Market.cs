using ExigoService;
using System.Collections.Generic;

namespace Common
{
    public class NetherlandsMarket : Market
    {
        public NetherlandsMarket()
            : base()
        {
            Name = MarketName.Netherlands;
            Description = "Netherlands";
            CookieValue = "NL";
            CultureCode = "nl-NL";
            Countries = new List<string> { "NL" };
        }

        public override IMarketConfiguration GetConfiguration()
        {
            return new NetherlandsConfiguration();
        }
    }
}