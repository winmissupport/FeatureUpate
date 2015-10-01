using System;
using ExigoService;
using Common.Api.ExigoWebService;

namespace ReplicatedSite.Models
{
    public class ShoppingCartCheckoutPropertyBag : BasePropertyBag
    {
        private string version = "1.0.3";
        private int expires    = 31;

        #region Constructors
        public ShoppingCartCheckoutPropertyBag()
        {
            this.CustomerID = (Identity.Customer != null) ? Identity.Customer.CustomerID : 0;
            this.Expires = expires;
        }
        #endregion

        #region Properties
        public int CustomerID { get; set; }

        public ShippingAddress ShippingAddress { get; set; }
        public ShippingAddress AutoOrderShippingAddress { get; set; }
        public string BillingName { get; set; }
        public Address BillingAddress { get; set; }

        public DateTime AutoOrderStartDate { get; set; }
        public FrequencyType AutoOrderFrequencyType { get; set; }

        public int ShipMethodID { get; set; }
        public int AutoOrderShipMethodID { get; set; }

        public IPaymentMethod PaymentMethod { get; set; }
        public IPaymentMethod AutoOrderPaymentMethod { get; set; }

        public bool UsePointPaymentForOrder { get; set; }
        public bool UsePointPaymentForAutoOrder { get; set; }

        //Added to use for checkbox to toggle to wholesale price for new customers and add auto order to theri cart to join as a Smart Shopper
        public bool GetSmartShopperPrice { get; set; }
        public bool BillingSameAsShipping { get; set; }

        public decimal PointAmountOrder { get; set; }
        public decimal CustomersAvailablePoints { get; set; }

        public bool IsRedirectPayment { get; set; }
        #endregion

        #region Methods
        public override T OnBeforeUpdate<T>(T propertyBag)
        {
            propertyBag.Version = version;

            return propertyBag;
        }
        public override bool IsValid()
        {
            var currentCustomerID = (Identity.Customer != null) ? Identity.Customer.CustomerID : 0;
            return this.Version == version && this.CustomerID == currentCustomerID;
        }
        #endregion
    }
}