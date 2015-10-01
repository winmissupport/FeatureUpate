using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments
{
    public class PaymentRequest : IPaymentRequest
    {
        public PaymentRequestMethod Method { get; set; }
        public string RequestUrl { get; set; }
    }
}