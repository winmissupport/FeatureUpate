using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments
{
    public interface IPaymentRequest
    {
        PaymentRequestMethod Method { get; set; }
        string RequestUrl { get; set; }
    }

    public enum PaymentRequestMethod
    {
        Get,
        Post
    }
}