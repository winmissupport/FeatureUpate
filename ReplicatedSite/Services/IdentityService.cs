using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using ReplicatedSite.Models;
using Common;
using ExigoService;
using Common.Services;
using Common.Helpers;
using Common.Api.ExigoWebService;
using Common.Providers;
using System.Web.Mvc;

namespace ReplicatedSite.Services
{
    public class IdentityService
    {
        IIdentityAuthenticationProvider authProvider = new ODataIdentityAuthenticationProvider();

        public IdentityService() { }
        public IdentityService(IIdentityAuthenticationProvider provider)
        {
            authProvider = provider;
        }


        // Owner Identities
        public static ReplicatedSiteIdentity GetIdentity(string webAlias)
        {
            webAlias = webAlias.ToUpper();
            var cacheKey = "{0}-OwnerIdentity-{1}".FormatWith(GlobalSettings.Exigo.Api.CompanyKey, webAlias);

            var identity = HttpContext.Current.Cache[cacheKey] as ReplicatedSiteIdentity;

            if (identity == null)
            {
                try
                {
                    // Get the customer site data 
                    var customerSite = Exigo.WebService().GetCustomerSite(new GetCustomerSiteRequest()
                    {
                        WebAlias = webAlias
                    });

                    var customerID = customerSite.CustomerID;

                    // bind the data to the identity model
                    identity = new ReplicatedSiteIdentity();
                    identity.CustomerID = customerID;
                    identity.WebAlias = customerSite.WebAlias;
                    identity.FirstName = customerSite.FirstName;
                    identity.LastName = customerSite.LastName;
                    identity.Company = customerSite.Company;
                    identity.Email = customerSite.Email;
                    identity.Phone = customerSite.Phone;
                    identity.Phone2 = customerSite.Phone2;
                    identity.Fax = customerSite.Fax;

                    identity.Notes1 = customerSite.Notes1;
                    identity.Notes2 = customerSite.Notes2;
                    identity.Notes3 = customerSite.Notes3;
                    identity.Notes4 = customerSite.Notes4;

                    // Get the remaining customer data
                    var customer = Exigo.OData().Customers
                        .Where(c => c.CustomerID == customerID)
                        .Select(c => new
                        {
                            CreatedDate = c.CreatedDate,
                            HighestAchievedRankID = c.RankID,
                            CustomerTypeID = c.CustomerTypeID,
                            CustomerStatusID = c.CustomerStatusID,
                            DefaultWarehouseID = c.DefaultWarehouseID,
                            EnrollerID = c.EnrollerID,
                            Address = c.MainAddress1,
                            Address2 = c.MainAddress2,
                            City = c.MainCity,
                            State = c.MainState,
                            Zip = c.MainZip,
                            Country = c.MainCountry
                        }).FirstOrDefault();

                    // Address 
                    identity.Address1 = customer.Address;
                    identity.Address2 = customer.Address2;
                    identity.City = customer.City;
                    identity.State = customer.State;
                    identity.Zip = customer.Zip;
                    identity.Country = customer.Country;


                    // Bind the additional data
                    identity.CreatedDate = customer.CreatedDate;
                    identity.HighestAchievedRankID = customer.HighestAchievedRankID;
                    identity.CustomerTypeID = customer.CustomerTypeID;
                    identity.CustomerStatusID = customer.CustomerStatusID;
                    identity.WarehouseID = (customer.DefaultWarehouseID != 0) ? customer.DefaultWarehouseID : Warehouses.Default;
                    identity.EnrollerID = Convert.ToInt32(customer.EnrollerID);


                    // Save the identity
                    HttpContext.Current.Cache.Insert(cacheKey,
                        identity,
                        null,
                        DateTime.Now.AddMinutes(Settings.IdentityRefreshInterval),
                        System.Web.Caching.Cache.NoSlidingExpiration,
                        System.Web.Caching.CacheItemPriority.Normal,
                        null);
                }
                catch
                {
                    return null;
                }
            }

            return identity;
        }


        // Customer Identities
        public LoginResponse SignIn(string loginname, string password, string overrideReturnUrl = "")
        {
            var response = new LoginResponse();

            try
            {
                // Authenticate the customer
                var customerID = Exigo.WebService().AuthenticateCustomer(new AuthenticateCustomerRequest { LoginName = loginname, Password = password }).CustomerID;
                if (customerID == 0)
                {
                    response.Fail("Unable to authenticate");
                    return response;
                }

                // Get the customer
                var identity = GetIdentity(customerID);
                if (identity == null)
                {
                    response.Fail("Customer not found");
                    return response;
                }

                response.Country = identity.Country;

                // Alan C - 1 April 2015 - Added below code block from Mike's update on my local machine at the office

                // If we are authenticating during the shopping process due to auto ship rules, we want to make sure we send the user back where they came from               
                var redirectUrl = GetSilentLoginRedirect(identity);

                response.RedirectUrl = (redirectUrl.IsNullOrEmpty()) ? overrideReturnUrl : redirectUrl;

                if (redirectUrl.IsNullOrEmpty()) CreateFormsAuthenticationTicket(customerID);

                // Here we check to see if the customer is a smart shopper, if so we need to redirect to their site
                if (identity.CustomerTypeID == CustomerTypes.SmartShopper)
                {
                    response.RedirectUrl = GetSmartShopperRedirect(identity.CustomerID);
                }

                // Mark the response as successful
                response.Success();
            }
            catch (Exception ex)
            {
                response.Fail(ex.Message);
            }

            return response;
        }
        public LoginResponse SignIn(int customerid)
        {
            var response = new LoginResponse();

            try
            {
                // Authenticate the customer
                var customerID = authProvider.AuthenticateCustomer(customerid);
                if (customerID == 0)
                {
                    response.Fail("Unable to authenticate");
                    return response;
                }

                // Get the customer
                var identity = GetIdentity(customerID);
                if (identity == null)
                {
                    response.Fail("Customer not found");
                    return response;
                }

                response.CustomerTypeID = identity.CustomerTypeID;

                // Get the redirect URL (for silent logins) or create the forms ticket
                response.RedirectUrl = GetSilentLoginRedirect(identity);
                if (response.RedirectUrl.IsEmpty()) CreateFormsAuthenticationTicket(customerID);

                // Mark the response as successful
                response.Success();
            }
            catch (Exception ex)
            {
                response.Fail(ex.Message);
            }

            return response;
        }
        public LoginResponse SignIn(string silentLoginToken)
        {
            var response = new LoginResponse();

            try
            {
                // New Method - if it works
                
                //// Decrypt the token
                //var decryptedToken = Security.Decrypt(silentLoginToken);

                //// Split the value and get the values
                //var splitToken = decryptedToken.Split('|');
                //var customerID = Convert.ToInt32(decryptedToken.CustomerID);
                //var tokenExpirationDate = Convert.ToDateTime(decryptedToken.ExpirationDate);

                // Split the value and get the values
                //var splitToken = decryptedToken.Split('|');
                //var customerID = Convert.ToInt32(decryptedToken.CustomerID);
                //var tokenExpirationDate = Convert.ToDateTime(decryptedToken.ExpirationDate);
                
                // Old School if new way doesn't work

                // Decrypt the token
                var key = GlobalSettings.Encryptions.Key;
                var iv = GlobalSettings.Encryptions.IV;
                var decryptedToken = Security.AESDecrypt(silentLoginToken, key, iv);

                // Split the value and get the values
                var splitToken = decryptedToken.Split('|');
                var customerID = Convert.ToInt32(splitToken[0]);
                var tokenExpirationDate = Convert.ToDateTime(splitToken[2]).ToUniversalTime();

                // Return the expiration status of the token and the sign in response
                if (tokenExpirationDate < DateTime.Now)
                {
                    response.Fail("Token expired");
                    return response;
                }

                // Sign the customer in with their customer ID
                response = SignIn(customerID);

                // Mark the response as successful
                response.Success();

                response.Status = true;
            }
            catch (Exception ex)
            {
                response.Fail(ex.Message);
            }

            return response;
        }
        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }

        public void RefreshIdentity()
        {
            CreateFormsAuthenticationTicket(Identity.Customer.CustomerID);
        }

        public CustomerIdentity GetIdentity(int customerID)
        {
            return Exigo.OData().Customers
                .Where(c => c.CustomerID == customerID)
                .Select(c => new CustomerIdentity()
                {
                    CustomerID = c.CustomerID,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Company = c.Company,
                    LoginName = c.LoginName,
                    CustomerTypeID = c.CustomerTypeID,
                    CustomerStatusID = c.CustomerStatusID,
                    LanguageID = c.LanguageID,
                    DefaultWarehouseID = c.DefaultWarehouseID,
                    CurrencyCode = c.CurrencyCode,
                    Country = c.MainCountry,
                    EnrollerID = Convert.ToInt32(c.EnrollerID)
                })
                .FirstOrDefault();
        }
        public string GetSilentLoginRedirect(CustomerIdentity identity)
        {
            if (identity.CustomerTypeID == CustomerTypes.BrandPartner)
            {
                var token = Security.Encrypt(new
                {
                    CustomerID = identity.CustomerID,
                    ExpirationDate = DateTime.Now.AddHours(1)
                });

                return GlobalSettings.Backoffices.SilentLogins.DistributorBackofficeUrl.FormatWith(token);
            }

            return string.Empty;
        }
        public string GetSmartShopperRedirect(int customerID)
        {
            var query = Exigo.OData().CustomerSites.Where(c => c.CustomerID == customerID);
            var url = "";

            if (query.Count() > 0)
            {
                try
                {
                    var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                    var customerSite = query.FirstOrDefault();
                    if (customerSite != null)
                    {
                        var returnUrl = HttpContext.Current.Request.QueryString["ReturnUrl"];

                        url = urlHelper.Action("index", "account", new { webalias = customerSite.WebAlias });

                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            if (returnUrl.Contains("checkout"))
                            {
                                url = urlHelper.Action("checkout", "shopping", new { webalias = customerSite.WebAlias });
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            return url;
        }
        public bool CreateFormsAuthenticationTicket(int customerID)
        {
            // If we got here, we are authorized. Let's attempt to get the identity.
            var identity = GetIdentity(customerID);
            if (identity == null) return false;



            // Create the ticket
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,
                identity.CustomerID.ToString(),
                DateTime.Now,
                DateTime.Now.AddMinutes(Settings.SessionTimeout),
                false,
                identity.SerializeProperties());


            // Encrypt the ticket
            string encTicket = FormsAuthentication.Encrypt(ticket);


            // Create the cookie.
            HttpCookie cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName]; //saved user
            if (cookie == null)
            {
                cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                cookie.HttpOnly = true;

                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            else
            {
                cookie.Value = encTicket;
                HttpContext.Current.Response.Cookies.Set(cookie);
            }


            // Add the customer ID to the items in case we need this in the same request later on.
            // We need this because we don't have access to the Identity.Current in this same request later on.
            HttpContext.Current.Items.Add("CustomerID", customerID);


            return true;
        }
    }
}