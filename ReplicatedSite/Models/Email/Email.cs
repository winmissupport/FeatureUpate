using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ReplicatedSite;

namespace ReplicatedSite.Models
{
    public class Email
    {
        [DataType(DataType.Text), Required]
        public string FirstName { get; set; }
        [DataType(DataType.Text), Required]
        public string LastName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [Required, DataType(DataType.Text)]
        public string PreferredContact { get; set; }
        [Required]
        public ContactType PreferredContactType { get; set; }
        public string AlternateContact { get; set; }
        public ContactType AlternateContactType { get; set; }
        [DataType(DataType.Text), Required]
        public string Comments { get; set; }
        [DataType(DataType.Text)]
        public string ReferringBrandPartner { get; set; }
        public string MessageType { get; set; }

        public string PageMessage { get; set; }

        public enum ContactType
        {
            Select,
            Email,
            Phone
        }

    }
}