using ExigoService;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Payments
{
    public class NonRedirectPaymentProvider : IPaymentProvider
    {
        public string ID { get { return "NonRedirect"; } }


        public PaymentHandlerType HandlerType { get { return PaymentHandlerType.Local; } }

        public Order Order { get; set; }
        public IOrderConfiguration OrderConfiguration { get; set; }

        public IPaymentRequest GetPaymentRequest(PaymentRequestArgs args)
        {
            return new PaymentRequest(); 
        }

        public IPaymentResponse EndTransaction(object args)
        {
            // Parse the information based on how this bank wants you to handle their response

            return new PaymentResponse()
            {
                Status            = PaymentResultStatus.Success,
                AuthorizationCode = "123456"
            };
        }

        public PaymentResult GetResponse(Uri uri, NameValueCollection form)
        {
            var result = new PaymentResult()
            {
                Uri = uri,
                Form = form
            };


            var querystring = new Dictionary<string, string>();
            foreach (var query in uri.Query.TrimStart('?').Split('&'))
            {
                var queryPart = query.Split('=');
                querystring.Add(queryPart[0], queryPart[1]);
            }


            return result;
        }
    }
}