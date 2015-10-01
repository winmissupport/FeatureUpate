using ExigoService;
using ReplicatedSite.Models;
using System.Collections.Generic;

namespace ReplicatedSite.ViewModels
{
    public class EnrollmentReviewViewModel : IEnrollmentViewModel
    {       
        public IEnumerable<IItem> Items { get; set; }
        public OrderCalculationResponse Totals { get; set; }
        public IEnumerable<IShipMethod> ShipMethods { get; set; }
        public int ShipMethodID { get; set; }
        public EnrollmentPropertyBag PropertyBag { get; set; }
        public ShoppingCartItemsPropertyBag Cart { get; set; }
        public string[] Errors { get; set; }
        public IEnumerable<IPaymentMethod> PaymentMethods { get; set; }

        public int PaymentTypeID { get; set; }
        public IPaymentMethod PaymentMethod { get; set; }

        public IOrderConfiguration Configuration { get; set; }
    }
}