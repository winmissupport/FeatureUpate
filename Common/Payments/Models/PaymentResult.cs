using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Payments
{
    public class PaymentResult
    {
        public PaymentResult()
        {

        }

        public PaymentStatus Status { get; set; }
        public string ErrorMessage { get; set; }
        public string Token { get; set; }
        public int Attempt { get; set; }
        public int OrderID { get; set; }
        public string TransactionRecord { get; set; }
        public string ProviderType { get; set; }
        public string PaymentType { get; set; }
        public int CustomerID { get; set; }
        public Uri Uri { get; set; }
        public NameValueCollection Form { get; set; }
        public Dictionary<string, string> QueryString { get; set; }
    }

    public enum PaymentStatus
    {
        Fail,
        Success
    }    
}