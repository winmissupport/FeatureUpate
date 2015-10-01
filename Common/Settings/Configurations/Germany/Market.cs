using ExigoService;
using System.Collections.Generic;

namespace Common
{
    public class GermanyMarket : Market
    {
        public GermanyMarket()
            : base()
        {
            Name = MarketName.Germany;
            Description = "Germany";
            CookieValue = "DE";
            CultureCode = "de-DE";
            Countries = new List<string> { "DE" };
        }

        public override IMarketConfiguration GetConfiguration()
        {
            return new GermanyConfiguration();
        }
    }
}