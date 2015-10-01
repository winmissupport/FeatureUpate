using ExigoService;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Payments
{
    public interface IPaymentProvider
    {
        string ID { get; }

        PaymentHandlerType HandlerType { get; }

        Order Order { get; set; }
        IOrderConfiguration OrderConfiguration { get; set; }
     

        IPaymentRequest GetPaymentRequest(PaymentRequestArgs args);
        IPaymentResponse EndTransaction(object args);

        PaymentResult GetResponse(Uri uri, NameValueCollection form);
    }
}