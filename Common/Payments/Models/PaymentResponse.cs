using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments
{
    public class PaymentResponse : IPaymentResponse
    {
        public PaymentResultStatus Status { get; set; }
        public string[] Errors { get; set; }
        public string AuthorizationCode { get; set; }
    }
}