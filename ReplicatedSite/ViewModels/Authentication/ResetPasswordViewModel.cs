using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public int CustomerID { get; set; }

        [Required]
        public int CustomerType { get; set; }

        [Required, Display(Name = "New Password")]
        [RegularExpression("(?=^.{8,}$)(?=.*[0-9])(?=.*[a-zA-Z])([a-zA-Z0-9]+)$", ErrorMessage = "Password must be between minimum 8 characters with 1 letter and 1 number.")]
        public string Password { get; set; }

        [Required, Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}