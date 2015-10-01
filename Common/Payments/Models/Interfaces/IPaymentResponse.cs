using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments
{
    public interface IPaymentResponse
    {
        PaymentResultStatus  Status { get; set; }
        string[] Errors { get; set; }
        string AuthorizationCode { get; set; }
    }
}