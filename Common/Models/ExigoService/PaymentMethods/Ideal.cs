using Common.Api.ExigoWebService;
using System;
using System.ComponentModel.DataAnnotations;

namespace ExigoService
{
    public class Ideal : IPaymentMethod, IAutoOrderPaymentMethod
    {
        public int Attempts { get; set; }

        public int[] AutoOrderIDs { get; set; }

        public bool IsUsedInAutoOrders { get { return true; } }

        // Not sure if this is the correct Payment type : Mike M.
        public AutoOrderPaymentType AutoOrderPaymentType { get { return Common.Api.ExigoWebService.AutoOrderPaymentType.WillSendPayment; } }

        public bool IsComplete
        {
            get
            {
                return true;
            }
        }
        public bool IsValid
        {
            get
            {
                return true;
            }
        }
        public bool Selected { get; set; } 
    }
}