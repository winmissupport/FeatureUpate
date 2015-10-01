﻿using Common;
using System.Collections.Generic;

namespace ExigoService
{
    public class Market : IMarket
    {
        public Market()
        {
            this.Configuration = GetConfiguration();
        }

        public MarketName Name { get; set; }
        public string Description { get; set; }
        public string CookieValue { get; set; }
        public string CultureCode { get; set; }
        public bool IsDefault { get; set; }
        public IEnumerable<string> Countries { get; set; }

        public IMarketConfiguration Configuration { get; set; }
        public virtual IMarketConfiguration GetConfiguration()
        {
            return new UnitedStatesConfiguration();
        }
    }
}