using Common;
using Common.Filters;
using Common.ModelBinders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web;



namespace ExigoService
{
    public class Customer : ICustomer
    {
        public Customer()
        {
            this.MainAddress = new Address() { AddressType = AddressType.Main };
            this.MailingAddress = new Address() { AddressType = AddressType.Mailing };
            this.OtherAddress = new Address() { AddressType = AddressType.Other };
        }



        public int CustomerID { get; set; }

        [Required(ErrorMessage = "First Name is required"), Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required"), Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Publish by Name is required"), Display(Name = "Publish by Name")]
        public string PublicName { get; set; }

        [Display(Name = "Company")]
        public string Company { get; set; }

        public int CustomerTypeID { get; set; }
        public int CustomerStatusID { get; set; }
        public int DefaultWarehouseID { get; set; }

        [DataType("Languages")]
        public int LanguageID { get; set; }
        public DateTime CreatedDate { get; set; }

        [PropertyBinder(typeof(BirthDateModelBinder)), DataType("BirthDate")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "SSN/TaxID is required"), Display(Name = "SSN/Tax ID"), DataType("TaxID"), Remote("IsTaxIDAvailable", "App", ErrorMessage = "This Tax ID is already taken in our system, if you have not already created an account please contact customer service.")]
        public string TaxID { get; set; }

        [Required]
        public string PayableToName { get; set; }

        [Required]
        public int PayableTypeID { get; set; }

        public bool IsOptedIn { get; set; }

        [Required, DataType(DataType.EmailAddress), Remote("IsEmailAvailable", "App", ErrorMessage = "This email isn't available - try another one."), RegularExpression(GlobalSettings.RegularExpressions.EmailAddresses, ErrorMessage = "This email doesn't look right - can you check it again?"), Display(Name = "Email")]
        public string Email { get; set; }

        [Required, DataType(DataType.PhoneNumber), Display(Name = "Evening Phone")]
        public string PrimaryPhone { get; set; }

        [DataType(DataType.PhoneNumber), Display(Name = "Daytime Phone")]
        public string SecondaryPhone { get; set; }

        [DataType(DataType.PhoneNumber), Display(Name = "Mobile Phone")]
        public string MobilePhone { get; set; }

        [DataType(DataType.PhoneNumber), Display(Name = "Fax Number")]
        public string Fax { get; set; }

        [DataType("Address")]
        public Address MainAddress { get; set; }

        [DataType("Address")]
        public Address MailingAddress { get; set; }

        [DataType("Address")]
        public Address OtherAddress { get; set; }

        public List<Address> Addresses
        {
            get
            {
                var collection = new List<Address>();
                if (this.MainAddress != null && this.MainAddress.IsComplete) collection.Add(this.MainAddress);
                if (this.MailingAddress != null && this.MailingAddress.IsComplete) collection.Add(this.MailingAddress);
                if (this.OtherAddress != null && this.OtherAddress.IsComplete) collection.Add(this.OtherAddress);
                return collection;
            }
            set { }
        }

        [Required, DataType("LoginName"), Remote("IsLoginNameAvailable", "App", ErrorMessage = "This username isn't available - try another one."), RegularExpression(GlobalSettings.RegularExpressions.LoginName, ErrorMessage = "Make sure your username doesn't contain any spaces or special characters."), Display(Name = "Username")]
        public string LoginName { get; set; }

        [Required, DataType("Password"), RegularExpression(GlobalSettings.RegularExpressions.Password, ErrorMessage = "Password must be between minimum 8 characters with 1 letter and 1 number."), Display(Name = "Password")]
        public string Password { get; set; }

        [Required, System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The passwords you entered do not match, please try again."), DataType(DataType.Password), Display(Name = "Confirm Password")]  
        public string ConfirmPassword { get; set; }

        public int? EnrollerID { get; set; }
        public int? SponsorID { get; set; }
        public Rank HighestAchievedRank { get; set; }

        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public string Field6 { get; set; }
        public string Field7 { get; set; }
        public string Field8 { get; set; }
        public string Field9 { get; set; }
        public string Field10 { get; set; }
        public string Field11 { get; set; }
        public string Field12 { get; set; }
        public string Field13 { get; set; }
        public string Field14 { get; set; }
        public string Field15 { get; set; }

        public DateTime? Date1 { get; set; }
        public DateTime? Date2 { get; set; }
        public DateTime? Date3 { get; set; }
        public DateTime? Date4 { get; set; }
        public DateTime? Date5 { get; set; }



        public CustomerType CustomerType { get; set; }
        public CustomerStatus CustomerStatus { get; set; }
        public Customer Enroller { get; set; }
        public Customer Sponsor { get; set; }



        public string FullName
        {
            get { return string.Join(" ", this.FirstName, this.LastName); }
        }
        public string AvatarUrl
        {
            get
            {
                return GlobalUtilities.GetCustomerAvatarUrl(this.CustomerID);
            }
        }

    }
}