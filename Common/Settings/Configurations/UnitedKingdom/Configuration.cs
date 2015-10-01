using Common.Api.ExigoWebService;
using ExigoService;
using System.Collections.Generic;

namespace Common
{
    public class UnitedKingdomConfiguration : IMarketConfiguration
    {
        private MarketName marketName = MarketName.UnitedKingdom;

        public MarketName MarketName
        {
            get
            {
                return marketName;
            }
        }

        #region Properties
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
        #endregion

        // Base Order Configuration
        public class BaseOrderConfiguration : IOrderConfiguration 
        {
            public BaseOrderConfiguration()
            {
                WarehouseID = Warehouses.UKWarehouse; 
                CurrencyCode = CurrencyCodes.Euro; 
                PriceTypeID = PriceTypes.Retail; 
                LanguageID = Languages.English; 
                DefaultCountryCode = "GB"; 
                DefaultShipMethodID = 20;

                AvailableShipMethods = new List<int> { 20 , 21 };
                CategoryID = 48;
                FeaturedProductsCategoryID = 47;
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
                WarehouseID = Warehouses.UKWarehouse;
                CurrencyCode = CurrencyCodes.Euro;
                PriceTypeID = PriceTypes.Autoship;
                LanguageID = Languages.English;
                DefaultCountryCode = "GB";
                DefaultShipMethodID = 20;
                CategoryID = 48;
                AvailableShipMethods = new List<int> { 20, 21 };
                DefaultFrequencyType = FrequencyType.Monthly;
                FeaturedProductsCategoryID = 47;
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
                CategoryID = 51;
            }
        }
        
        // Back Office Shopping
        public class BackOfficeOrderConfiguration : BaseOrderConfiguration
        {
            public BackOfficeOrderConfiguration()
            {
                PriceTypeID = PriceTypes.Wholesale;
                CategoryID = 49;
            }
        }

        // Back Office Auto Order
        public class BackOfficeAutoOrderConfiguration : BaseAutoOrderConfiguration
        {
            public BackOfficeAutoOrderConfiguration()
            {
                CategoryID = 45; 
            }
        }
    }
}