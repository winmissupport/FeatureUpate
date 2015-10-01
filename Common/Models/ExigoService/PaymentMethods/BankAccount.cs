using Common.Api.ExigoWebService;
using System.ComponentModel.DataAnnotations;

namespace ExigoService
{
    public class BankAccount : IBankAccount
    {
        public BankAccount()
        {
            this.Type = BankAccountType.New;
            this.BillingAddress = new Address();
            this.AutoOrderIDs = new int[0];
        }
        public BankAccount(BankAccountType type)
        {
            Type = type;
            BillingAddress = new Address();
        }

        [Required]
        public BankAccountType Type { get; set; }

        [Required, Display(Name = "Name on Account")]
        public string NameOnAccount { get; set; }

        [Required, Display(Name = "Bank Name")]
        public string BankName { get; set; }

        [Required, Display(Name = "Account Number")]
        public string AccountNumber { get; set; }

        [Required, Display(Name = "Routing Number")]
        public string RoutingNumber { get; set; }

        [Required, DataType("Address")]
        public Address BillingAddress { get; set; }

        public int[] AutoOrderIDs { get; set; }


        public bool IsComplete
        {
            get
            {
                if (string.IsNullOrEmpty(NameOnAccount)) return false;
                if (string.IsNullOrEmpty(BankName)) return false;
                if (string.IsNullOrEmpty(AccountNumber)) return false;
                if (string.IsNullOrEmpty(RoutingNumber)) return false;

                return true;
            }
        }
        public bool IsValid
        {
            get
            {
                if (!IsComplete) return false;

                return true;
            }
        }
        public bool IsUsedInAutoOrders
        {
            get { return this.AutoOrderIDs.Length > 0; }
        }

        public AutoOrderPaymentType AutoOrderPaymentType
        {
            get
            {
                switch (this.Type)
                {
                    case BankAccountType.Primary:
                    default: return Common.Api.ExigoWebService.AutoOrderPaymentType.CheckingAccount;
                }
            }
        }
    }
}