using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments
{
    public class PaymentRequestArgs
    {
        public PaymentRequestArgs()
        {
            Attempt = 1;
            QueryStringArgs = new Dictionary<string, object>();
        }

        public string ReturnUrl { get; set; }
        public int Attempt { get; set; }
        public Dictionary<string, object> QueryStringArgs { get; set; }
        public string WebAlias { get; set; }
        public Address BillingAddress { get; set; }
        public string BillingName { get; set; }
        // If we see the web alias is present, we want to format the url a bit differently. We use this lookup to determine if we passed a WebAlias in our request.
        public bool RequiresWebAlias { get { return !this.WebAlias.IsEmpty(); } }
    }
}