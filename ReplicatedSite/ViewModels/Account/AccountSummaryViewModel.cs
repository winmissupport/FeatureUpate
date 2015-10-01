using Common;
using ExigoService;
using ReplicatedSite.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public class AccountSummaryViewModel
    {
        public int CustomerID { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }


        [Required, System.Web.Mvc.Remote("IsValidLoginName", "Account")]
        public string LoginName { get; set; }

        [Required, RegularExpression(GlobalSettings.RegularExpressions.Password, ErrorMessage = "Password must be between minimum 8 characters with 1 letter and 1 number."), Display(Name = "Password")]
        public string Password { get; set; }

        [Required, RegularExpression(GlobalSettings.RegularExpressions.Password, ErrorMessage = "Password must be between minimum 8 characters with 1 letter and 1 number."), Display(Name = "Password")]
        public string NewPassword { get; set; }

        [Required, Compare("Password", ErrorMessage = "Your passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public int LanguageID { get; set; }

        [Display(Name = "Primary Phone")]
        public string PrimaryPhone { get; set; }
        [Display(Name = "Secondary Phone")]
        public string SecondaryPhone { get; set; }
        public string MobilePhone { get; set; }
        public string Fax { get; set; }
        public IEnumerable<Address> Addresses { get; set; }

        public CustomerSite CustomerSite { get; set; }

        public bool IsOptedIn { get; set; }


        public IEnumerable<Language> Languages { get; set; }
    }
}