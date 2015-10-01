using System;
using System.Collections.Generic;

namespace ExigoService
{
    public interface IOrderConfiguration
    {
        int WarehouseID { get; set; }
        string CurrencyCode { get; set; }
        int PriceTypeID { get; set; }
        int LanguageID { get; set; }
        string DefaultCountryCode { get; set; }
        int DefaultShipMethodID { get; set; }
        int CategoryID { get; set; }
        int FeaturedProductsCategoryID { get; set; }

        List<int> AvailableShipMethods { get; set; }
    }
}