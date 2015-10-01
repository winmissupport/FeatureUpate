using Common.Api.ExigoWebService;
using ExigoService;
using System.Collections.Generic;

namespace Common
{
    public class BelgiumConfiguration : IMarketConfiguration
    {
        private MarketName marketName = MarketName.Belgium;

        public MarketName MarketName
        {
            get
            {
                return marketName;
            }
        }

        // Shopping
        public IOrderConfiguration Orders
        {
            get
            {
                return new OrderConfiguration();
            }
        }
        public IAutoOrderConfiguration AutoOrders
        {
            get
            {
                return new AutoOrderConfiguration();
            }
        }

        public IOrderConfiguration BackOfficeOrders
        {
            get
            {
                return new BackOfficeOrderConfiguration();
            }
        }
        public IAutoOrderConfiguration BackOfficeAutoOrders
        {
            get
            {
                return new BackOfficeAutoOrderConfiguration();
            }
        }

        // Enrollment Shopping
        public IOrderConfiguration EnrollmentOrders
        {
            get
            {
                return new EnrollmentShoppingConfiguration();
            }
        }
        // Base Order Configuration
        public class BaseOrderConfiguration : IOrderConfiguration
        {
            public BaseOrderConfiguration()
            {
                WarehouseID = Warehouses.BelgiumWarehouse;
                CurrencyCode = CurrencyCodes.Euro;
                PriceTypeID = PriceTypes.Retail;
                LanguageID = Languages.English;
                DefaultCountryCode = "BE";
                DefaultShipMethodID = 5;

                AvailableShipMethods = new List<int> { 18, 19 };
                CategoryID = 33;
                FeaturedProductsCategoryID = 32;
            }

            public int WarehouseID { get; set; }
            public string CurrencyCode { get; set; }
            public int PriceTypeID { get; set; }
            public int LanguageID { get; set; }
            public string DefaultCountryCode { get; set; }
            public int DefaultShipMethodID { get; set; }
            public List<int> AvailableShipMethods { get; set; }
            public int CategoryID { get; set; }
            public int FeaturedProductsCategoryID { get; set; }
        }

        // Base Auto Order Configuration
        public class BaseAutoOrderConfiguration : IAutoOrderConfiguration
        {
            public BaseAutoOrderConfiguration()
            {
                WarehouseID = Warehouses.BelgiumWarehouse;
                CurrencyCode = CurrencyCodes.Euro;
                PriceTypeID = PriceTypes.Autoship;
                LanguageID = Languages.English;
                DefaultCountryCode = "BE";
                CategoryID = 33;
                DefaultShipMethodID = 18;

                AvailableShipMethods = new List<int> { 18, 19 };
                DefaultFrequencyType = FrequencyType.Monthly;
                FeaturedProductsCategoryID = 32;
            }


            public int FeaturedProductsCategoryID { get; set; }
            public int WarehouseID { get; set; }
            public string CurrencyCode { get; set; }
            public int PriceTypeID { get; set; }
            public int LanguageID { get; set; }
            public string DefaultCountryCode { get; set; }
            public int DefaultShipMethodID { get; set; }
            public List<int> AvailableShipMethods { get; set; }
            public int CategoryID { get; set; }
            public FrequencyType DefaultFrequencyType { get; set; }
        }

        // Shopping Order
        public class OrderConfiguration : BaseOrderConfiguration
        {
            public OrderConfiguration()
            {
                CategoryID = 33;
            }
        }

        // Shopping Auto Order
        public class AutoOrderConfiguration : BaseAutoOrderConfiguration
        {
            public AutoOrderConfiguration()
            {
                CategoryID = 33;
            }
        }

        // Enrollment Shopping Order
        public class EnrollmentShoppingConfiguration : BaseOrderConfiguration
        {
            public EnrollmentShoppingConfiguration()
            {
                CategoryID = 26;
            }
        }


        // Back Office Shopping
        public class BackOfficeOrderConfiguration : BaseOrderConfiguration
        {
            public BackOfficeOrderConfiguration()
            {
                PriceTypeID = PriceTypes.Wholesale;
                CategoryID = 19;
            }
        }

        // Back Office Auto Order
        public class BackOfficeAutoOrderConfiguration : BaseAutoOrderConfiguration
        {
            public BackOfficeAutoOrderConfiguration()
            {
                CategoryID = 41;
            }
        }
    }
}