using ExigoService;
using System.Collections.Generic;

namespace Common
{
    public class UnitedKingdomMarket : Market
    {
        public UnitedKingdomMarket()
            : base()
        {
            Name = MarketName.UnitedKingdom;
            Description = "United Kingdom";
            CookieValue = "GB";
            CultureCode = "en-GB";
            Countries = new List<string> { "GB" };
        }

        public override IMarketConfiguration GetConfiguration()
        {
            return new UnitedKingdomConfiguration();
        }
    }
}