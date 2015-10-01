﻿using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web.Security;
using ReplicatedSite.Services;
using ExigoService;
using Common;

namespace ReplicatedSite
{
    public class CustomerIdentity : IIdentity
    {
        #region Constructors
        public CustomerIdentity(System.Web.Security.FormsAuthenticationTicket ticket)
        {
            Name        = ticket.Name;
            Expires     = ticket.Expiration;

            // Populate this object with the properties
            DeserializeProperties(ticket.UserData);
        }
        public CustomerIdentity()
        {

        }
        #endregion

        #region Properties
        /// <summary>
        /// These are the properties that will be transferred from the FormsAuthenticationTicket to the Identity. 
        /// If the property is not listed here, it will not be accounted for or auto-populated from the ticket when DeserializeProperties() is invoked.
        /// </summary>
        private List<string> SerializableFields = new List<string>()
        {
            "CustomerID",
            "FirstName",
            "LastName",
            "Company",
            "LoginName",
            "Country",
            "CustomerTypeID",
            "CustomerStatusID",
            "LanguageID",
            "DefaultWarehouseID",
            "CurrencyCode",
            "EnrollerID",
            "WebAlias"
        };

        public int CustomerID   { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company   { get; set; }
        public string LoginName { get; set; }
        public string Country { get; set; }
        public int CustomerTypeID { get; set; }
        public int CustomerStatusID { get; set; }
        public int LanguageID { get; set; }
        public int DefaultWarehouseID { get; set; }
        public string CurrencyCode { get; set; }
        public int EnrollerID { get; set; }
        public string WebAlias { get; set; }

        // Easy-access Properties
        public string FullName 
        {
            get { return this.FirstName + " " + this.LastName; }
        }
        public string DisplayName
        {
            get { return GlobalUtilities.Coalesce(this.Company, this.FirstName + " " + this.LastName); }
        }
        public Market Market
        {
            get { return Utilities.GetCurrentMarket(); }
        }
        public Language Language
        {
            get { return Utilities.GetCurrentLanguage(); }
        }
        public string PublicName
        {
            get { return Exigo.GetCustomerSite(this.CustomerID).Company; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Refreshes the current identity by fetching a fresh identity and saving it to the autnehtication cookie.
        /// </summary>
        public void Refresh()
        {
            var service = new IdentityService();
            service.CreateFormsAuthenticationTicket(this.CustomerID);
        }
        #endregion

        #region Serialization
        public string SerializeProperties()
        {
            // Get the string format
            var formatter = string.Empty;
            for(var i = 0; i < SerializableFields.Count; i++)
            {
                if(!string.IsNullOrEmpty(formatter)) formatter += "|";
                formatter += "{" + i + "}";
            }

            // Get the field data using reflection
            var fieldData = new List<object>();
            var type = typeof(CustomerIdentity);

            foreach(var field in SerializableFields)
            {
                foreach(var property in type.GetProperties())
                {
                    if(property.Name.Equals(field, StringComparison.InvariantCultureIgnoreCase))
                    {
                        fieldData.Add(property.GetValue(this));
                        break;
                    }
                }
            }

            // Return the formatted data
            return string.Format(formatter, fieldData.ToArray());
        }
        public void DeserializeProperties(string data)
        {
            var counter = 0;
            var dataArray = data.Split('|');


            // Re-populate this object using reflection
            var type = typeof(CustomerIdentity);
            foreach(var field in SerializableFields)
            {
                foreach(var property in type.GetProperties())
                {
                    if(property.Name.Equals(field, StringComparison.InvariantCultureIgnoreCase))
                    {
                        property.SetValue(this, Convert.ChangeType(dataArray[counter], property.PropertyType));
                        counter++;
                        break;
                    }
                }
            }
        }

        public static CustomerIdentity Deserialize(string data)
        {
            try
            {
                var ticket = FormsAuthentication.Decrypt(data);
                return new CustomerIdentity(ticket);
            }
            catch
            {
                var service = new IdentityService();
                service.SignOut();
                return null;
            }
        }
        #endregion

        #region IIdentity Settings
        string IIdentity.AuthenticationType
        {
            get { return "Custom"; }
        }
        bool IIdentity.IsAuthenticated
        {
            get { return true; }
        }
        public string Name { get; set; }
        public DateTime Expires { get; set; }
        #endregion
    }
}