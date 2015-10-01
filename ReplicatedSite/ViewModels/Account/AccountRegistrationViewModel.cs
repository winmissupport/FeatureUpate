using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Common;
using ExigoService;
using ReplicatedSite.Models;
using System.Web.Mvc;

namespace ReplicatedSite.ViewModels
{
    public class AccountRegistrationViewModel
    {
        [Required, Display(Name="First Name:"), DataType(DataType.Text)]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required, Display(Name = "Last Name:"), DataType(DataType.Text)]
        public string LastName { get; set; }

        [Display(Name = "Email / Username:"), DataType(DataType.EmailAddress)]
        [Required, Remote("IsLoginNameAvailable_Retail", "App", ErrorMessage = "This username isn't available - try another one."), EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Username { get; set; }

        [Display(Name = "Confirm Email:"), DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid Email Address"), System.ComponentModel.DataAnnotations.Compare("Username", ErrorMessage="The Email Addresses you entered do not match, please try again.")]
        public string ConfirmEmail { get; set; }

        public bool IsOptedIn { get; set; }

        [Display(Name = "Join as a Smart Shopper")]
        public bool JoinAsSmartShopper { get; set; }

        [Required, Display(Name = "Phone Number:"), DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Password:"), Required, DataType(DataType.Password), RegularExpression(GlobalSettings.RegularExpressions.Password, ErrorMessage = "Password must be between minimum 8 characters with 1 letter and 1 number.")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password:")]
        [Required, System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The passwords you entered do not match, please try again."), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public bool IsOrphan { get; set; }
        public int EnrollerID { get; set; }
        public bool ReturnedError { get; set; }

        public ShoppingCartItemsPropertyBag ShoppingCart { get; set; }
    }
}