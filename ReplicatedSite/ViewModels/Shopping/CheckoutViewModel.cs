using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExigoService;
using ReplicatedSite.Models;
using ReplicatedSite.ViewModels;

namespace ReplicatedSite.ViewModels
{
    public class CheckoutViewModel : IShoppingViewModel
    {
        public IEnumerable<IItem> Items { get; set; }
        public IEnumerable<IItem> AutoOrderItems { get; set; }
        public OrderCalculationResponse OrderTotals { get; set; }
        public OrderCalculationResponse AutoOrderTotals { get; set; }
        public IEnumerable<IShipMethod> ShipMethods { get; set; }
        public int ShipMethodID { get; set; }

        public IEnumerable<ShippingAddress> Addresses { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
        public Address BillingAddress { get; set; }
        public string BillingName { get; set; }
        public bool BillingSameAsShipping { get; set; }

        public IEnumerable<IPaymentMethod> PaymentMethods { get; set; }
        public int PaymentTypeID { get; set; }

        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public string[] Errors { get; set; }

        public PaymentMethodsViewModel PaymentMethodModel { get; set; }
    }
}