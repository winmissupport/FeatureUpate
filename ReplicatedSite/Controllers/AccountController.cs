using Common;
using Common.Api.ExigoWebService;
using Common.HtmlHelpers;
using Common.Services;
using ExigoService;
using ReplicatedSite;
using ReplicatedSite.Controllers;
using ReplicatedSite.Models;
using ReplicatedSite.Services;
using ReplicatedSite.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Common.Filters;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ReplicatedSite.Controllers
{
    [RequireHttpsWhenLive]
    [Authorize]
    [RoutePrefix("{webalias}")]
    
    public class AccountController : Controller
    {
        #region Properties
        public string ShoppingCartName = "ReplicatedSiteShopping";

        public ShoppingCartItemsPropertyBag ShoppingCart
        {
            get
            {
                if (_shoppingCart == null)
                {
                    _shoppingCart = Exigo.PropertyBags.Get<ShoppingCartItemsPropertyBag>(ShoppingCartName + "Cart");
                }
                return _shoppingCart;
            }
        }
        private ShoppingCartItemsPropertyBag _shoppingCart;

        public ShoppingCartCheckoutPropertyBag PropertyBag
        {
            get
            {
                if (_propertyBag == null)
                {
                    _propertyBag = Exigo.PropertyBags.Get<ShoppingCartCheckoutPropertyBag>(ShoppingCartName + "PropertyBag");
                }
                return _propertyBag;
            }
        }
        private ShoppingCartCheckoutPropertyBag _propertyBag;

        #endregion

        #region Overview
        [Route("settings")]
        public ActionResult Index()
        {
            var model = new AccountSummaryViewModel();

            var customer = Exigo.GetCustomer(Identity.Customer.CustomerID);
            

            model.CustomerID = customer.CustomerID;
            model.FirstName = customer.FirstName;
            model.LastName = customer.LastName;
            model.Email = customer.Email;
            model.LoginName = customer.LoginName;
            model.LanguageID = customer.LanguageID;

            model.PrimaryPhone = customer.PrimaryPhone;
            model.SecondaryPhone = customer.SecondaryPhone;
            model.MobilePhone = customer.MobilePhone;
            model.Fax = customer.Fax;
            model.Addresses = customer.Addresses;

            model.IsOptedIn = customer.IsOptedIn;

            if (Identity.Customer.CustomerTypeID == CustomerTypes.SmartShopper) {
                model.CustomerSite = new CustomerSite();
                model.CustomerSite = Exigo.GetCustomerSite(Identity.Customer.CustomerID);

                if (model.CustomerSite.WebAlias == null) {
                    model.CustomerSite.WebAlias = customer.CustomerID.ToString(); // WIN Requested this on 4/9/2015 

                    // Update the customer web alias

                        var customerSiteRequest = new SetCustomerSiteRequest(customer);
                        customerSiteRequest.CustomerID = model.CustomerID;
                        customerSiteRequest.WebAlias = model.CustomerID.ToString();
                        customerSiteRequest.FirstName = model.FirstName;
                        customerSiteRequest.LastName = model.LastName;
                        SetCustomerSiteResponse res = Exigo.WebService().SetCustomerSite(customerSiteRequest);
                }
                else
                {
                    model.CustomerSite.WebAlias = model.CustomerSite.WebAlias;
                }
            }

            // Get the available languages
            model.Languages = Exigo.GetLanguages();


            return View(model);
        }

        [HttpParamAction]
        public JsonNetResult UpdateEmailAddress(string email)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Email = email
            });

            Exigo.SendEmailVerification(Identity.Customer.CustomerID, email);

            var html = string.Format("{0}", email);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateEmailAddress",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateNotifications(bool IsOptedIn, string Email)
        {
            var model = new AccountSummaryViewModel();
            var html = string.Empty;

            try
            {

                if (IsOptedIn)
                {
                    Exigo.SendEmailVerification(Identity.Customer.CustomerID, Email);
                    html = string.Format("{0}", Resources.Common.OptedInStatus);
                }
                else
                {
                    Exigo.OptOutCustomer(Identity.Customer.CustomerID);
                    html = string.Format("{0}", Resources.Common.OptedOutStatus);
                }

                return new JsonNetResult(new
                {
                    success = true,
                    action = "UpdateNotifications",
                    message = "We have sent a verification email to your email address on file. Click the link in the email and you will be opted in!",
                    html = html
                });

            }
            catch (Exception exception)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = exception.Message + ": We apologize. We cannot opt you in at this time. Please try back later",
                    action = "UpdateNotfications"
                });
            }
        }

        [HttpParamAction]
        public JsonNetResult UpdateName(string firstname, string lastname)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                FirstName = firstname,
                LastName = lastname
            });

            var html = string.Format("{0} {1}, {3}# {2}", firstname, lastname, Identity.Customer.CustomerID, Resources.Common.ID);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateName",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateWebAlias(string webalias)
        {
            Exigo.WebService().SetCustomerSite(new SetCustomerSiteRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                WebAlias = webalias
            });

            var html = string.Format("<a href='http://www.mywinlife.com/{0}'>www.mywinlife.com/<strong>{0}</strong></a>", webalias);
            var url = "../../" + webalias.ToString() + "/settings";

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateWebAlias",
                url = url,
                html = html
            });
        }
        public JsonResult IsValidWebAlias(string webalias)
        {
            var isValid = Exigo.IsWebAliasAvailable(Identity.Customer.CustomerID, webalias);

            if (isValid) return Json(true, JsonRequestBehavior.AllowGet);
            else return Json(string.Format(Resources.Common.PasswordNotAvailable, webalias), JsonRequestBehavior.AllowGet);
        }

        [HttpParamAction]
        public JsonNetResult UpdateLoginName(string loginname)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                LoginName = loginname
            });

            var html = string.Format("{0}", loginname);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateLoginName",
                html = html
            });
        }
        public JsonResult IsValidLoginName(string loginname)
        {
            var isValid = Exigo.IsLoginNameAvailable(loginname, Identity.Customer.CustomerID);

            if (isValid) return Json(true, JsonRequestBehavior.AllowGet);
            else return Json(string.Format(Resources.Common.LoginNameNotAvailable, loginname), JsonRequestBehavior.AllowGet);
        }

        [HttpParamAction]
        public JsonNetResult UpdatePassword(string password)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                LoginPassword = password
            });

            var html = "********";

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdatePassword",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateLanguagePreference(int languageid)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                LanguageID = languageid
            });


            var language = Exigo.GetLanguage(languageid);
            var html = language.LanguageDescription;

            // Refresh the identity in case the country changed
            new IdentityService().RefreshIdentity();

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateLanguagePreference",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdatePhoneNumbers(string primaryphone, string secondaryphone)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Phone = primaryphone,
                Phone2 = secondaryphone
            });

            var html = string.Format(@"
                " + Resources.Common.Primary + @": <strong>{0}</strong><br />
                " + Resources.Common.Secondary + @": <strong>{1}</strong>
                ", primaryphone, secondaryphone);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdatePhoneNumbers",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateMobilePhone(string mobilephone)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                MobilePhone = mobilephone
            });

            var html = string.Format("{1}: <strong>{0}</strong>", mobilephone, Resources.Common.SendTextsTo);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateMobilePhone",
                html = html
            });
        }

        [HttpParamAction]
        public JsonNetResult UpdateFaxNumber(string fax)
        {
            Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Fax = fax
            });

            var html = string.Format("{1}: <strong>{0}</strong>", fax, Resources.Common.SendFaxesTo);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateFaxNumber",
                html = html
            });
        }

        // Website Settings
        [HttpParamAction]
        public JsonNetResult UpdateWebsiteCompany(CustomerSite customersite)
        {
            Exigo.UpdateCustomerSite(new CustomerSite
            {
                CustomerID = Identity.Customer.CustomerID,
                Company = customersite.Company,
                FirstName = Identity.Customer.FirstName,
                LastName = Identity.Customer.LastName
                
            });

            var html = string.Format("{0}", customersite.Company);

            return new JsonNetResult(new
            {
                success = true,
                action = "UpdateWebsiteCompany",
                html = html
            });
        }

        #endregion

        #region Creating an account
        [AllowAnonymous]
        [Route("register")]
        public ActionResult Register(string ReturnUrl)
        {
            var model = new AccountRegistrationViewModel();
            
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public ActionResult Register(string ReturnUrl, AccountRegistrationViewModel model)
        {
            try
            {
                var orderConfiguration = Utilities.GetCurrentMarket().Configuration.Orders;

                // Save the customer
                var request = new CreateCustomerRequest(); //removed model.middlename because is not used in project JWJ 02July2015
                request.FirstName = model.FirstName;               
                request.LastName = model.LastName;
                request.Email = model.Username;
                request.Phone = model.PhoneNumber;

                request.CanLogin = true;
                request.LoginName = model.Username;
                request.LoginPassword = model.Password;
                request.CustomerType = CustomerTypes.RetailCustomer;

                request.CustomerStatus = CustomerStatuses.Active;

                request.InsertEnrollerTree = true;
                request.EnrollerID = (model.EnrollerID != 0) ? model.EnrollerID : Identity.Owner.CustomerID;
                request.SponsorID = Identity.Owner.CustomerID;
                request.InsertUnilevelTree = true;
                request.EntryDate = DateTime.Now;
                request.DefaultWarehouseID = orderConfiguration.WarehouseID;
                request.CurrencyCode = orderConfiguration.CurrencyCode;
                request.LanguageID = orderConfiguration.LanguageID;

                var response = Exigo.WebService().CreateCustomer(request);

                // Sign the customer into their backoffice
                var service = new IdentityService();
                service.SignIn(model.Username, model.Password);


                if (ReturnUrl.IsNotEmpty()) return Redirect(ReturnUrl);
                else return RedirectToAction("index", "account", new { webalias = Identity.Owner.WebAlias });
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                if (ReturnUrl.IsNotEmpty()) return RedirectToAction("register", "account", new { success = false, ReturnUrl = ReturnUrl, webalias = Identity.Owner.WebAlias });
                else return RedirectToAction("register", "account", new { success = false, webalias = Identity.Owner.WebAlias });

            }
        }
        #endregion

        #region Addresses
        [Route("addresses")]
        public ActionResult AddressList()
        {
            var model = Exigo.GetCustomerAddresses(Identity.Customer.CustomerID).Where(c => c.IsComplete).ToList();

            return View(model);
        }

        [Route("addresses/edit/{type:alpha}")]
        public ActionResult ManageAddress(AddressType type)
        {
            var model = Exigo.GetCustomerAddresses(Identity.Customer.CustomerID).Where(c => c.AddressType == type).FirstOrDefault();

            return View("ManageAddress", model);
        }

        [Route("addresses/new")]
        public ActionResult AddAddress()
        {
            var model = new Address();
            model.AddressType = AddressType.New;
            model.Country = GlobalUtilities.GetSelectedCountryCode();

            return View("ManageAddress", model);
        }

        public ActionResult DeleteAddress(AddressType type)
        {
            Exigo.DeleteCustomerAddress(Identity.Customer.CustomerID, type);

            return RedirectToAction("AddressList", "Account", new { webalias = Identity.Owner.WebAlias });
        }

        public ActionResult SetPrimaryAddress(AddressType type)
        {
            Exigo.SetCustomerPrimaryAddress(Identity.Customer.CustomerID, type);

            return RedirectToAction("AddressList", "Account", new { webalias = Identity.Owner.WebAlias });
        }

        [HttpPost]
        public ActionResult SaveAddress(Address address, bool? makePrimary)
        {
            // Verify the address
            var verifyAddressResponse = Exigo.VerifyAddress(address);

            // Verify the address //This is failing right now - Alan C 15 April 2015

            //if (Identity.Customer.Market.Name == MarketName.UnitedStates) {
            //    var verifyAddressResponse = Exigo.VerifyAddress(address);
            //    address = verifyAddressResponse.VerifiedAddress as Address;
            //}

            address = Exigo.SetCustomerAddressOnFile(Identity.Customer.CustomerID, address);

            if (makePrimary != null && ((bool)makePrimary) == true)
            {
                Exigo.SetCustomerPrimaryAddress(Identity.Customer.CustomerID, address.AddressType);
            }

            return RedirectToAction("AddressList", "Account", new { webalias = Identity.Owner.WebAlias });
        }
        #endregion

        #region Payment Methods
        [Route("paymentmethods")]
        
        public ActionResult PaymentMethodList()
        {
            var model = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                ExcludeIncompleteMethods = true
            });

            return View(model);
        }

        #region Credit Cards
        [Route("paymentmethods/creditcards/edit/{type:alpha}")]
        public ActionResult ManageCreditCard(CreditCardType type)
        {
            var model = Exigo.GetCustomerPaymentMethods(Identity.Customer.CustomerID)
                .Where(c => c is CreditCard && ((CreditCard)c).Type == type)
                .FirstOrDefault();

            // Clear out the card number
            ((CreditCard)model).CardNumber = "";

            return View("ManageCreditCard", model);
        }

        [Route("paymentmethods/creditcards/new")]
        public ActionResult AddCreditCard()
        {
            var model = new CreditCard();
            model.Type = CreditCardType.New;
            model.BillingAddress = new Address()
            {
                Country = GlobalSettings.Company.Address.Country
            };

            return View("ManageCreditCard", model);
        }

        public ActionResult DeleteCreditCard(CreditCardType type)
        {
            Exigo.DeleteCustomerCreditCard(Identity.Customer.CustomerID, type);

            return RedirectToAction("PaymentMethods");
        }

        [HttpPost]
        public ActionResult SaveCreditCard(CreditCard card)
        {
            try
            {
                card = Exigo.SetCustomerCreditCard(Identity.Customer.CustomerID, card);

                return RedirectToAction("PaymentMethods");
            }
            catch (Exception ex)
            {
                return RedirectToAction("PaymentMethods", new { error = ex.Message.ToString() });
            }
        }
        #endregion

        #region Bank Accounts
        [Route("paymentmethods/bankaccounts/edit/{type:alpha}")]
        public ActionResult ManageBankAccount(ExigoService.BankAccountType type)
        {
            var model = Exigo.GetCustomerPaymentMethods(Identity.Customer.CustomerID)
                .Where(c => c is BankAccount && ((BankAccount)c).Type == type)
                .FirstOrDefault();


            // Clear out the account number
            ((BankAccount)model).AccountNumber = "";


            return View("ManageBankAccount", model);
        }

        [Route("paymentmethods/bankaccounts/new")]
        public ActionResult AddBankAccount()
        {
            var model = new BankAccount();
            model.Type = ExigoService.BankAccountType.New;
            model.BillingAddress = new Address()
            {
                Country = GlobalSettings.Company.Address.Country
            };

            return View("ManageBankAccount", model);
        }

        public ActionResult DeleteBankAccount(ExigoService.BankAccountType type)
        {
            Exigo.DeleteCustomerBankAccount(Identity.Customer.CustomerID, type);

            return RedirectToAction("PaymentMethodList");
        }

        [HttpPost]
        public ActionResult SaveBankAccount(BankAccount account)
        {
            account = Exigo.SetCustomerBankAccount(Identity.Customer.CustomerID, account);

            return RedirectToAction("PaymentMethodList");
        }
        #endregion

        #endregion

        #region Order History
        [Route("orders/{page:int:min(1)=1}")]
        public ActionResult OrderList(int page)
        {
            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Page = page,
                RowCount = 10,
                IncludeOrderDetails = true
            }).Where(c => c.OrderTypeID != OrderTypes.RecurringOrder).ToList();

            return View("OrderList", model);
        }

        [Route("orders/cancelled/{page:int:min(1)=1}")]
        public ActionResult CancelledOrdersList(int page)
        {
            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Page = page,
                RowCount = 10,
                OrderStatuses = new int[] { 4 },
                IncludeOrderDetails = true
            }).Where(c => c.OrderTypeID != OrderTypes.RecurringOrder).ToList();

            return View("OrderList", model);
        }
        [Route("orders/pendingautoorderlist/{page:int:min(1)=1}")]
        public ActionResult PendingAutoOrderList(int page)
        {
            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Page = page,
                RowCount = 10,
                OrderStatuses = new int[] { OrderStatuses.Pending },
                IncludeOrderDetails = true
            }).Where(c => c.OrderTypeID == OrderTypes.RecurringOrder).ToList();

            return View("OrderList", model);
        }
        [Route("orders/open/{page:int:min(1)=1}")]
        public ActionResult OpenOrdersList(int page)
        {
            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Page = page,
                RowCount = 10,
                OrderStatuses = new int[] { 0, 1, 2, 3, 5, 6, 10 },
                IncludeOrderDetails = true
            }).Where(c => c.OrderTypeID != OrderTypes.RecurringOrder).ToList();

            return View("OrderList", model);
        }

        [Route("orders/shipped/{page:int:min(1)=1}")]
        public ActionResult ShippedOrdersList(int page)
        {
            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Page = page,
                RowCount = 10,
                OrderStatuses = new int[] { 9 },
                IncludeOrderDetails = true
            }).Where(c => c.OrderTypeID != OrderTypes.RecurringOrder).ToList();

            return View("OrderList", model);
        }

        [Route("orders/declined/{page:int:min(1)=1}")]
        public ActionResult DeclinedOrdersList(int page)
        {
            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                Page = page,
                RowCount = 10,
                OrderStatuses = new int[] { 0, 2, 3 },
                IncludeOrderDetails = true
            }).Where(c => c.OrderTypeID != OrderTypes.RecurringOrder).ToList();

            return View("OrderList", model);
        }

        [Route("orders/search/{id:int}")]
        public ActionResult SearchOrdersList(int id)
        {
            ViewBag.IsSearch = true;

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                OrderID = id,
                IncludeOrderDetails = true
            }).Where(c => c.OrderTypeID != OrderTypes.RecurringOrder).ToList();

            return View("OrderList", model);
        }

        [Route("order/cancel")]
        public ActionResult CancelOrder(string token)
        {
            var orderID = Convert.ToInt32(Security.Decrypt(token, Identity.Customer.CustomerID));

            Exigo.CancelOrder(orderID);

            return Redirect(Request.UrlReferrer.ToString());
        }
        #endregion

        #region Invoices
        [Route("invoice")]
        public ActionResult OrderInvoice(string token)
        {
            var orderID = Convert.ToInt32(Security.Decrypt(token, Identity.Customer.CustomerID));

            var model = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                OrderID = orderID
            }).FirstOrDefault();

            return View("OrderInvoice", model);
        }
        #endregion

        #region Signing in
        [AllowAnonymous]
        [Route("login")]
        public ActionResult Login()
        {
            var model = new LoginViewModel();

            return View(model);
        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public JsonNetResult Login(LoginViewModel model)
        {
            var service = new IdentityService();
            var response = service.SignIn(model.LoginName, model.Password);

            if (response.Status)
            {
                var selectedCountry = GlobalUtilities.GetSelectedCountryCode();

                if (response.Country != selectedCountry)
                {
                    Exigo.PropertyBags.Delete(PropertyBag);
                    Exigo.PropertyBags.Delete(ShoppingCart);

                    var country = (response.Country.IsEmpty()) ? "US" : response.Country;

                    GlobalUtilities.SetSelectedCountryCode(country);

                    GlobalUtilities.SetCurrentCulture();
                }
            }

            return new JsonNetResult(response);
        }

        [AllowAnonymous]
        public ActionResult SilentLogin(string token)
        {
            var service = new IdentityService();
            var response = service.SignIn(token);

            if (response.Status)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        #endregion
        
        #region Signing Out
        [Route("logout")]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        #endregion

        #region AJAX
        [AllowAnonymous]
        [HttpPost]
        public JsonNetResult GetDistributors(string query)
        {
            try
            {
                // assemble a list of customers who match the search criteria
                var enrollerCollection = new List<SearchResult>();

                var basequery = Exigo.OData().CustomerSites.Where(c => c.Customer.CustomerTypeID == CustomerTypes.BrandPartner);
                var isCustomerID = query.CanBeParsedAs<int>();

                if (isCustomerID)
                {
                    var customerQuery = basequery.Where(c => c.CustomerID == Convert.ToInt32(query));

                    if (customerQuery.Count() > 0)
                    {
                        enrollerCollection = customerQuery.Select(c => new SearchResult
                        {
                            CustomerID = c.CustomerID,
                            FirstName = c.FirstName,
                            LastName = c.LastName,
                            MainCity = c.Customer.MainCity,
                            MainState = c.Customer.MainState,
                            MainCountry = c.Customer.MainCountry,
                            WebAlias = c.WebAlias
                        }).ToList();
                    }
                }
                else
                {
                    var customerQuery = basequery.Where(c => c.FirstName.Contains(query) || c.LastName.Contains(query));

                    if (customerQuery.Count() > 0)
                    {
                        enrollerCollection = customerQuery.Select(c => new SearchResult
                        {
                            CustomerID = c.CustomerID,
                            FirstName = c.FirstName,
                            LastName = c.LastName,
                            MainCity = c.Customer.MainCity,
                            MainState = c.Customer.MainState,
                            MainCountry = c.Customer.MainCountry,
                            WebAlias = c.WebAlias
                        }).ToList();
                    }
                }



                var urlHelper = new UrlHelper(Request.RequestContext);
                foreach (var item in enrollerCollection)
                {
                    item.AvatarURL = urlHelper.Avatar(item.CustomerID);
                }

                return new JsonNetResult(new
                {
                    success = true,
                    enrollers = enrollerCollection
                });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        public ActionResult GetWebAlias()
        {
            var customerID = Identity.Customer.CustomerID;
            
            try
            {
                var customerSite = Exigo.GetCustomerSite(customerID);

                if (Identity.Customer.WebAlias == null) {
                    Identity.Customer.WebAlias = customerSite.WebAlias;
                }
                
                return new JsonNetResult(new 
                {
                    success = true,
                    webalias = customerSite.WebAlias
                });
            }
            catch(Exception exception)
            {
                return new JsonNetResult(new
                    {
                        success = false,
                        message = exception.Message
                    });
            }
        }
        #endregion

        #region Models and Enums

        public class SearchResult
        {
            public int CustomerID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName
            {
                get { return this.FirstName + " " + this.LastName; }
            }
            public string AvatarURL { get; set; }
            public string WebAlias { get; set; }
            public string ReplicatedSiteUrl
            {
                get
                {
                    if (string.IsNullOrEmpty(this.WebAlias)) return "";
                    else return "http://google.com/" + this.WebAlias;
                }
            }

            public string MainState { get; set; }
            public string MainCity { get; set; }
            public string MainCountry { get; set; }
        }

        #endregion

        #region ForgotPassword

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ForgotPasswordPartial()
        {
            return PartialView("_DistributorForgotPasswordPartial", new DistributorForgotPasswordViewModel());
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public string DistibutorForgotPassword(DistributorForgotPasswordViewModel model)
        {
            //Search if email
            GetCustomersRequest getRequest = new GetCustomersRequest();
            //getRequest.CustomerID = model.CustomerID;
            getRequest.LoginName = model.LoginName;
            
            var response = Exigo.WebService().GetCustomers(getRequest);

            if (response.Customers.Count() == 0)
                return "<strong>There is no user associated with the User Name you provided. Please contact customer Service.</strong>";

            //Generate Link to reset password
            string hashPassword = ConvertStringtoMD5(GenerateRandomString(20));

            string url = Url.Action("ResetPassword", "account", new { passwordReset = hashPassword }, HttpContext.Request.Url.Scheme);

            Customer customer = (Customer)response.Customers[0];

            var email = customer.Email;

            byte[] array = Encoding.ASCII.GetBytes(email);

            SendEmail(url, customer.CustomerID, customer.Email, hashPassword);

            return "<strong>We've sent you an email with instructions on how to reset your password</strong>";
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPassword(string passwordReset)
        {
            //Created Model
            var model = new ResetPasswordViewModel();

            //Find Customer By hash
            var query = Exigo.OData().Customers.Where(c => c.Field3 == passwordReset);

            if (query != null && query.Count() > 0)
            {
                var customer = query.FirstOrDefault();
                return View(new ResetPasswordViewModel() { CustomerID = customer.CustomerID, CustomerType = customer.CustomerTypeID });
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            UpdateCustomerRequest updRequest = new UpdateCustomerRequest();
            updRequest.CustomerID = model.CustomerID;
            updRequest.LoginPassword = model.Password;
            updRequest.Field3 = string.Empty;
            var response = Exigo.WebService().UpdateCustomer(updRequest);


            return new JsonNetResult(new
            {
                success = true
            });
        }

        public void SendEmail(string url, int customerID, string emailAddress, string hashPassword)
        {
            try
            {
                //Send Email with Reset instructions
                MailMessage email = new MailMessage();

                email.From = new MailAddress(GlobalSettings.Emails.NoReplyEmail);
                email.To.Add(emailAddress);
                email.Subject = "Win Password Reset";

                email.Body = url;

                SmtpClient SmtpServer = new SmtpClient();
                SmtpServer.Host = GlobalSettings.Emails.SMTPConfigurations.Default.Server;
                SmtpServer.Port = GlobalSettings.Emails.SMTPConfigurations.Default.Port;
                SmtpServer.Credentials = new System.Net.NetworkCredential(GlobalSettings.Emails.SMTPConfigurations.Default.Username, GlobalSettings.Emails.SMTPConfigurations.Default.Password);
                SmtpServer.EnableSsl = GlobalSettings.Emails.SMTPConfigurations.Default.EnableSSL;

                Task.Factory.StartNew(() =>
                {
                    SmtpServer.Send(email);
                });

                UpdateCustomerRequest request = new UpdateCustomerRequest();
                request.CustomerID = customerID;
                request.Field3 = hashPassword;
                Exigo.WebService().UpdateCustomer(request);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string ConvertStringtoMD5(string strword)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(strword);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private string GenerateRandomString(int length)
        {
            char[] charArr = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
            string randomString = string.Empty;
            Random objRandom = new Random();
            for (int i = 0; i < length; i++)
            {
                int x = objRandom.Next(1, charArr.Length);
                if (!randomString.Contains(charArr.GetValue(x).ToString()))
                    randomString += charArr.GetValue(x);
                else
                    i--;
            }
            return randomString;
        }

        #endregion
    }
}