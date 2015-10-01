using ExigoService;
using System.Collections.Generic;

namespace ReplicatedSite.ViewModels
{
    public class AutoOrderPaymentViewModel
    {
        public int AutoorderID { get; set; }

        public IEnumerable<IPaymentMethod> PaymentMethods { get; set; }
        public AutoOrder AutoOrder { get; set; }

        public string[] Errors { get; set; }
        public int PaymentTypeID { get; set; }
    }
}