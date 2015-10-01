using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    //public class DistributorForgotPasswordViewModel
    //{
    //    [Required, Display(Name = "Customer ID")]
    //    [DataType(DataType.Text, ErrorMessage = "Please enter a valid customer ID")]
    //    public int CustomerID { get; set; }
    //}

    public class DistributorForgotPasswordViewModel
    {
        [Required, Display(Name = "User Name")]
        [DataType(DataType.Text, ErrorMessage = "Please enter a valid User Name")]
        public string LoginName { get; set; }
    }
}