using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments
{
    public class POSTPaymentRequest : PaymentRequest
    {
        public POSTPaymentRequest()
        {
            base.Method = PaymentRequestMethod.Post;

        }

        public string RequestForm { get; set; }
    }
}