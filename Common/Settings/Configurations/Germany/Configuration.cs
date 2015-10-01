using Common.Api.ExigoWebService;
using ExigoService;
using System.Collections.Generic;

namespace Common
{
    public class GermanyConfiguration : IMarketConfiguration
    {
        private MarketName marketName = MarketName.Germany;

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
                WarehouseID = Warehouses.GermanyWarehouse;
                CurrencyCode = CurrencyCodes.Euro;
                PriceTypeID = PriceTypes.Retail;
                LanguageID = Languages.English;
                DefaultCountryCode = "DE";
                DefaultShipMethodID = 16;

                AvailableShipMethods = new List<int> { 16, 17 };
                CategoryID = 36;
                FeaturedProductsCategoryID = 37;
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
        }

        // Base Auto Order Configuration
        public class BaseAutoOrderConfiguration : IAutoOrderConfiguration
        {
            public BaseAutoOrderConfiguration()
            {
                WarehouseID = Warehouses.GermanyWarehouse;
                CurrencyCode = CurrencyCodes.Euro;
                PriceTypeID = PriceTypes.Autoship;
                LanguageID = Languages.English;
                DefaultCountryCode = "DE";
                DefaultShipMethodID = 17;

                AvailableShipMethods = new List<int> { 16, 17 };
                CategoryID = 36;
                FeaturedProductsCategoryID = 37;
                DefaultFrequencyType = FrequencyType.Monthly;
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
            }
        }

        // Shopping Auto Order
        public class AutoOrderConfiguration : BaseAutoOrderConfiguration
        {
            public AutoOrderConfiguration()
            {
            }
        }

        // Enrollment Shopping Order
        public class EnrollmentShoppingConfiguration : BaseOrderConfiguration
        {
            public EnrollmentShoppingConfiguration()
            {
                CategoryID = 28;
            }
        }

        // Back Office Shopping
        public class BackOfficeOrderConfiguration : BaseOrderConfiguration
        {
            public BackOfficeOrderConfiguration()
            {
                PriceTypeID = PriceTypes.Wholesale;
                CategoryID = 25;
            }
        }

        // Back Office Auto Order
        public class BackOfficeAutoOrderConfiguration : BaseAutoOrderConfiguration
        {
            public BackOfficeAutoOrderConfiguration()
            {
                CategoryID = 38;
            }
        }

    }
}