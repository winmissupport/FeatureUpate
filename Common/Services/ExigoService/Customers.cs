using Common;
using Common.Api.ExigoWebService;
using Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static Customer GetCustomer(int customerID)
        {
            var customer = Exigo.OData().Customers.Expand("CustomerStatus,CustomerType,Rank,Enroller,Sponsor")
                .Where(c => c.CustomerID == customerID)
                .FirstOrDefault();
            if (customer == null) return null;

            return (Customer)customer;
        }
        public static IEnumerable<CustomerWallItem> GetCustomerRecentActivity(GetCustomerRecentActivityRequest request)
        {
            var query = Exigo.OData().CustomerWall
                 .Where(c => c.CustomerID == request.CustomerID);

            if (request.StartDate != null)
            {
                query = query.Where(c => c.EntryDate >= request.StartDate);
            }

            var items = query
                .OrderByDescending(c => c.EntryDate)
                .Select(c => c)
                .Skip(request.Skip)
                .Take(request.Take);


            foreach (var item in items)
            {
                var wallItem = (CustomerWallItem)item;
                yield return wallItem;
            }
        }

        public static CustomerStatus GetCustomerStatus(int customerStatusID)
        {
            var customerStatus = Exigo.OData().CustomerStatuses
                .Where(c => c.CustomerStatusID == customerStatusID)
                .FirstOrDefault();
            if (customerStatus == null) return null;

            return (CustomerStatus)customerStatus;
        }
        public static CustomerType GetCustomerType(int customerTypeID)
        {
            var customerType = Exigo.OData().CustomerTypes
                .Where(c => c.CustomerTypeID == customerTypeID)
                .FirstOrDefault();
            if (customerType == null) return null;

            return (CustomerType)customerType;
        }

        public static void SetCustomerPreferredLanguage(int customerID, int languageID)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = customerID,
                LanguageID = languageID
            });

            var language = GlobalSettings.Globalization.AvailableLanguages.Where(c => c.LanguageID == languageID).FirstOrDefault();
            if (language != null)
            {
                GlobalUtilities.SetCurrentUICulture(language.CultureCode);
            }
        }
        public static bool IsEmailAvailable(string email)
        {
            // Validate the email address
            return Exigo.OData().Customers
                .Where(c => c.Email == email)
                .Count() == 0;
        }
        public static bool IsEmailAvailable(int customerID, string email)
        {
            // Validate the email address
            return Exigo.OData().Customers
                .Where(c => c.CustomerID != customerID)
                .Where(c => c.Email == email)
                .Count() == 0;
        }
        public static bool IsLoginNameAvailable(string loginname)
        {
            // Validate the email address
            return Exigo.OData().Customers
                .Where(c => c.Email == loginname)
                .Count() == 0;
        }
        public static bool IsLoginNameAvailable(string loginname, int customerID)
        {
            if (customerID != 0)
            {
                // Get the current login name to see if it matches what we passed. If so, it's still valid.
                var currentLoginName = Exigo.GetCustomer(customerID).LoginName;
                if (loginname.Equals(currentLoginName, StringComparison.InvariantCultureIgnoreCase)) return true;
            }

            // Validate the login name
            return Exigo.WebService().Validate(new IsLoginNameAvailableValidateRequest
            {
                LoginName = loginname
            }).IsValid;
        }
        public static bool IsTaxIDAvailable(string taxid)
        {
            // Validate the login name
            return Exigo.WebService().Validate(new IsTaxIDAvailableValidateRequest
            {
                TaxID = taxid
            }).IsValid;
        }

        public static void SendEmailVerification(int customerID, string email)
        {
            // Create the publicly-accessible verification link
            string sep = "&";
            if (!GlobalSettings.Emails.VerifyEmailUrl.Contains("?")) sep = "?";

            string encryptedValues = Security.Encrypt(new
            {
                CustomerID = customerID,
                Email = email,
                Date = DateTime.Now
            });

            var verifyEmailUrl = GlobalSettings.Emails.VerifyEmailUrl + sep + "token=" + encryptedValues;


            // Send the email
            Exigo.SendEmail(new SendEmailRequest
            {
                To                = new[] { email },
                From              = GlobalSettings.Emails.NoReplyEmail,
                ReplyTo           = new[] { GlobalSettings.Emails.NoReplyEmail },
                SMTPConfiguration = GlobalSettings.Emails.SMTPConfigurations.Default,
                Subject           = "{0} - Verify your email".FormatWith(GlobalSettings.Company.Name),
                Body              = @"
                    <p>
                        {1} has received a request to enable this email account to receive email notifications from {1} and your upline.
                    </p>

                    <p> 
                        To confirm this email account, please click the following link:<br />
                        <a href='{0}'>{0}</a>
                    </p>

                    <p>
                        If you did not request email notifications from {1}, or believe you have received this email in error, please contact {1} customer service.
                    </p>

                    <p>
                        Sincerely, <br />
                        {1} Customer Service
                    </p>"
                    .FormatWith(verifyEmailUrl, GlobalSettings.Company.Name)
            });
        }
        public static void OptInCustomer(string token)
        {
            var session = Security.Decrypt(token);

            var customerID = Convert.ToInt32(session.CustomerID);
            var email = session.Email.ToString();

            OptInCustomer(customerID, email);
        }
        public static void OptInCustomer(int customerID, string email)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID             = customerID,
                Email                  = email,
                SubscribeToBroadcasts  = true,
                SubscribeFromIPAddress = GlobalUtilities.GetClientIP()
            });
        }
        public static void OptOutCustomer(int customerID)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID            = customerID, 
                SubscribeToBroadcasts = false
            });
        }
    }
}