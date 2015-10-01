using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments
{
    public class PaymentService
    {
        public static IPaymentProvider GetPaymentProvider(Uri uri)
        {
            var queryString       = uri.ToString();
            var substring         = queryString.Substring(queryString.IndexOf('?')).Split('#')[0];
            var parsedQueryString = HttpUtility.ParseQueryString(substring);
            var providerID        = parsedQueryString["_p"];

            return GetPaymentProviderByID(providerID);
        }
        public static IPaymentProvider GetPaymentProvider(string countryCode)
        {
            switch (countryCode)
            {
                case "NL":
                case "DE":
                case "BE":
                    return new IngenicoPaymentProvider();                    
                default:
                    return new IngenicoPaymentProvider();
            }            
        }

        public static IPaymentProvider GetPaymentProviderByID(string providerID)
        {
            switch (providerID)
            {
                case "Ingenico":                
                    return new IngenicoPaymentProvider();
                default:
                    return new IngenicoPaymentProvider();
            }       
        }
    }
}