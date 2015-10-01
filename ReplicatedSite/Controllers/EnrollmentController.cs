using Common;
using Common.Api.ExigoWebService;
using Common.Providers;
using Common.HtmlHelpers;
using Common.Services;
using ExigoService;
using ReplicatedSite.Factories;
using ReplicatedSite.Models;
using ReplicatedSite.Providers;
using ReplicatedSite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Payments;
using System.Threading.Tasks;
using Common.Filters;

namespace ReplicatedSite.Controllers
{
    [RequireHttpsWhenLive]
    [RoutePrefix("{webalias}/enrollment")]
    public class EnrollmentController : Controller
    {
        #region Properties
        public string ApplicationName = "Win Enrollment";
        public string PaymentRedirectURL = "p-l?type=RE"; // 18Sept2015 JWJ shortened payment-landing to p-l and repenroll to RE due to Ingenico URL length restrictions

        public IOrderConfiguration OrderConfiguration = Common.GlobalUtilities.GetMarketConfigurationByCountry().EnrollmentOrders;

        public ShoppingCartItemsPropertyBag ShoppingCart
        {
            get
            {
                if (_shoppingCart == null)
                {
                    _shoppingCart = Exigo.PropertyBags.Get<ShoppingCartItemsPropertyBag>(ApplicationName + "Cart");
                }
                return _shoppingCart;
            }
        }
        private ShoppingCartItemsPropertyBag _shoppingCart;

        public EnrollmentPropertyBag PropertyBag
        {
            get
            {
                if (_propertyBag == null)
                {
                    _propertyBag = Exigo.PropertyBags.Get<EnrollmentPropertyBag>(ApplicationName + "PropertyBag");
                }
                return _propertyBag;
            }
        }
        private EnrollmentPropertyBag _propertyBag;

        public ILogicProvider LogicProvider
        {
            get
            {
                if (_logicProvider == null)
                {
                    _logicProvider = new EnrollmentLogicProvider(this, ShoppingCart, PropertyBag);
                }
                return _logicProvider;
            }
        }
        private ILogicProvider _logicProvider;
        #endregion

        #region Views
        public ActionResult Index()
        {
            ShoppingCart.Items.Clear();
            Exigo.PropertyBags.Update(ShoppingCart);

            return View();
        }

        [Route("opportunity")]
        public ActionResult Opportunity()
        {
            return View();
        }

        public ActionResult EnrollmentConfiguration(EnrollmentConfigurationViewModel enroller = null)
        {
            //if (Request.HttpMethod == "GET")
            //{
            //    var model = new EnrollmentConfigurationViewModel();

            //    model.EnrollerID = Identity.Owner.CustomerID;

            //    return View(model);
            //}
            //else
            //{
            PropertyBag.EnrollerID = Identity.Owner.CustomerID;
            //PropertyBag.SelectedMarket = enroller.MarketName;
            PropertyBag.EnrollmentType = EnrollmentType.Distributor;  //enroller.SelectedEnrollmentType;
            Exigo.PropertyBags.Update(PropertyBag);
            return RedirectToAction("Checkout");
            //}
        }

        public ActionResult ProductList()
        {
            var model = new ProductListViewModel();

            model.OrderItems = Exigo.GetItems(new ExigoService.GetItemsRequest
            {
                Configuration = OrderConfiguration
            }).ToList();


            return View(model);
        }

        public ActionResult EnrolleeInfo(FormCollection form = null)
        {
            if (Request.HttpMethod == "GET")
            {
                var cartItems = ShoppingCart.Items.ToList();
                if (cartItems.Count() > 0)
                {
                    var order = Exigo.CalculateOrder(new OrderCalculationRequest
                    {
                        Configuration = OrderConfiguration,
                        Items = cartItems,
                        Address = PropertyBag.ShippingAddress,
                        ShipMethodID = PropertyBag.ShipMethodID,
                        ReturnShipMethods = true
                    });

                    PropertyBag.ShipMethods = order.ShipMethods;
                }

                return View(PropertyBag);
            }
            else
            {
                var type = typeof(EnrollmentPropertyBag);
                var binder = Binders.GetBinder(type);
                var bindingContext = new ModelBindingContext()
                {
                    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => PropertyBag, type),
                    ModelState = ModelState,
                    ValueProvider = form
                };
                binder.BindModel(ControllerContext, bindingContext);



                // Set shipping address
                if (PropertyBag.UseSameShippingAddress)
                {
                    PropertyBag.ShippingAddress = new ShippingAddress(PropertyBag.Customer.MainAddress);
                    PropertyBag.ShippingAddress.FirstName = PropertyBag.Customer.FirstName;
                    PropertyBag.ShippingAddress.LastName = PropertyBag.Customer.LastName;
                    PropertyBag.ShippingAddress.Phone = PropertyBag.Customer.PrimaryPhone;
                    PropertyBag.ShippingAddress.Email = PropertyBag.Customer.Email;
                }

                Exigo.PropertyBags.Update(PropertyBag);

                // Create the customer
                //var request = new CreateCustomerRequest(PropertyBag.Customer);

                return new JsonNetResult(new
                {
                    success = true
                });
            }

        }

        public ActionResult TermsOfAgreement()
        {
            return View();
        }

        //public ActionResult Review()
        //{
        //    var model = EnrollmentViewModelFactory.Create<EnrollmentReviewViewModel>(PropertyBag);

        //    // Get the cart items
        //    var cartItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order || c.Type == ShoppingCartItemType.EnrollmentPack).ToList();
        //    model.Items = Exigo.GetItems(ShoppingCart.Items, OrderConfiguration).ToList();

        //    var calculationResult = Exigo.CalculateOrder(new OrderCalculationRequest
        //    {
        //        Configuration = OrderConfiguration,
        //        Items = cartItems,
        //        Address = PropertyBag.ShippingAddress,
        //        ShipMethodID = PropertyBag.ShipMethodID,
        //        ReturnShipMethods = true
        //    });

        //    model.Totals = calculationResult;
        //    model.ShipMethods = calculationResult.ShipMethods;


        //    // Set the default ship method
        //    var shipMethodID = 0;

        //    if (PropertyBag.ShipMethodID != 0)
        //    {
        //        shipMethodID = PropertyBag.ShipMethodID;
        //    }
        //    else
        //    {
        //        shipMethodID = OrderConfiguration.DefaultShipMethodID;
        //        PropertyBag.ShipMethodID = OrderConfiguration.DefaultShipMethodID;
        //        Exigo.PropertyBags.Update(PropertyBag);
        //    }

        //    if (model.ShipMethods != null)
        //    {
        //        if (model.ShipMethods.Any(c => c.ShipMethodID == shipMethodID))
        //        {
        //            model.ShipMethods.First(c => c.ShipMethodID == shipMethodID).Selected = true;
        //        }
        //        else
        //        {
        //            // If we don't have the ship method we're supposed to select, 
        //            // check the first one, save the selection and recalculate
        //            model.ShipMethods.First().Selected = true;

        //            PropertyBag.ShipMethodID = model.ShipMethods.First().ShipMethodID;
        //            Exigo.PropertyBags.Update(PropertyBag);

        //            var newCalculationResult = Exigo.CalculateOrder(new OrderCalculationRequest
        //            {
        //                Configuration = OrderConfiguration,
        //                Items = cartItems,
        //                Address = PropertyBag.ShippingAddress,
        //                ShipMethodID = PropertyBag.ShipMethodID,
        //                ReturnShipMethods = false
        //            });

        //            model.Totals = newCalculationResult;
        //        }
        //    }
        //    else
        //    {

        //    }


        //    return View(model);
        //}


        public ActionResult Checkout()
        {
            return LogicProvider.GetNextAction();
        }

        #endregion

        #region Step One - Country Selection
        [Route("selectcountry")]
        public ActionResult CountrySelection()
        {
            var model = new SelectCountryViewModel();

            if (PropertyBag.ShippingAddress != null && !PropertyBag.ShippingAddress.Country.IsEmpty())
            {
                model.SelectedCountry = PropertyBag.ShippingAddress.Country;
                model.SelectedLanguageID = Utilities.GetCurrentSelectedLanguage().LanguageID;
            }

            return View(model);
        }

        [HttpPost, Route("selectcountry")]
        public ActionResult CountrySelection(SelectCountryViewModel model)
        {
            bool countryChanged = false;

            // If we are changing the country from what was set before, we want to clear out the cart items before proceeding
            if (PropertyBag.ShippingAddress != null && !PropertyBag.ShippingAddress.Country.IsEmpty())
            {
                if (PropertyBag.ShippingAddress.Country != model.SelectedCountry)
                {
                    Exigo.PropertyBags.Delete(ShoppingCart);
                    Exigo.PropertyBags.Delete(PropertyBag);
                    countryChanged = true;
                }
            }

            // Set the selected country
            PropertyBag.ShippingAddress = new ShippingAddress { Country = model.SelectedCountry };
            Exigo.PropertyBags.Update(PropertyBag);

            Utilities.SetCurrentCountry(model.SelectedCountry);

            // Set the selected language for the entire site
            var cultureCode = GlobalSettings.Globalization.AvailableLanguages.FirstOrDefault(l => l.LanguageID == model.SelectedLanguageID).CultureCode;
            Utilities.SetCurrentLanguage(cultureCode);

            if (countryChanged)
            {
                return RedirectToAction("personalinfo");
            }
            else
            {
                return LogicProvider.GetNextAction();
            }
        }
        #endregion

        #region Step Two - Personal Info
        [Route("personalinfo")]
        public ActionResult PersonalInfo()
        {
            return View(PropertyBag);
        }

        [HttpPost, Route("personalinfo")]
        public ActionResult PersonalInfo(EnrollmentPropertyBag propertyBag)
        {
            var requestForm = Request.Form;

            var birthDay = Convert.ToInt32(requestForm["Customer.BirthDay"]);
            var birthMonth = Convert.ToInt32(requestForm["Customer.BirthMonth"]);
            var birthYear = Convert.ToInt32(requestForm["Customer.BirthYear"]);
            DateTime birthDate = new DateTime(birthYear, birthMonth, birthDay);

            propertyBag.Customer.BirthDate = birthDate;
            PropertyBag.Customer = propertyBag.Customer;
            PropertyBag.Customer.MainAddress = propertyBag.ShippingAddress;
            PropertyBag.Customer.MailingAddress = propertyBag.ShippingAddress;
            PropertyBag.Customer.LoginName = propertyBag.Customer.Email;
            PropertyBag.Customer.PublicName = propertyBag.Customer.PublicName;

            // Set up our shipping address
            var shippingAddress = propertyBag.ShippingAddress;
            shippingAddress.FirstName = propertyBag.Customer.FirstName;
            shippingAddress.LastName = propertyBag.Customer.LastName;
            shippingAddress.Email = propertyBag.Customer.Email;
            shippingAddress.Phone = propertyBag.Customer.PrimaryPhone;

            PropertyBag.Customer.IsOptedIn = propertyBag.Customer.IsOptedIn;
            PropertyBag.ShippingAddress = propertyBag.ShippingAddress;

            Exigo.PropertyBags.Update(PropertyBag);
            return LogicProvider.GetNextAction();
        }
        #endregion

        #region Step Three - Enrollment Kits
        [Route("pack")]
        public ActionResult Packs()
        {
            var model = new PacksViewModel();

            var items = Exigo.GetItems(new ExigoService.GetItemsRequest
            {
                Configuration = OrderConfiguration
            });

            model.OrderItems = items;

            if (ShoppingCart.Items.Count > 0)
            {
                model.SelectedOrderItem = ShoppingCart.Items.FirstOrDefault();
            }
            else
            {
                var selectedItemCode = (items.Count() > 1) ? items.Skip(1).FirstOrDefault().ItemCode : items.First().ItemCode;
                model.SelectedOrderItem = new ShoppingCartItem { ItemCode = selectedItemCode };
            }

            return View(model);
        }
        [HttpPost, Route("pack")]
        public ActionResult Packs(string SelectedPack)
        {
            // Add pack
            ShoppingCart.Items.Remove(ShoppingCartItemType.EnrollmentPack);
            ShoppingCart.Items.Add(new ShoppingCartItem
            {
                ItemCode = SelectedPack,
                Quantity = 1,
                Type = ShoppingCartItemType.EnrollmentPack
            });


            Exigo.PropertyBags.Update(ShoppingCart);

            return LogicProvider.GetNextAction();
        }
        #endregion

        #region Step Four - Payment & Review
        public ActionResult Review()
        {
            var logicCheck = LogicProvider.CheckLogic();
            var model = EnrollmentViewModelFactory.Create<EnrollmentReviewViewModel>(PropertyBag);

            try
            {
                var availableShipMethods = OrderConfiguration.AvailableShipMethods;

                var shipMethodID = PropertyBag.ShipMethodID;
                if (PropertyBag.ShipMethodID == 0)
                {
                    shipMethodID = OrderConfiguration.DefaultShipMethodID;
                }

                // Get the cart items
                var cartItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.EnrollmentPack).ToList();
                model.Items = Exigo.GetItems(ShoppingCart.Items, OrderConfiguration).ToList();


                // Calculate the order totals
                var calculationResult = Exigo.CalculateOrder(new OrderCalculationRequest
                {
                    Configuration = OrderConfiguration,
                    Items = cartItems,
                    Address = PropertyBag.ShippingAddress,
                    ShipMethodID = shipMethodID,
                    ReturnShipMethods = true
                });

                model.Totals = calculationResult;
                model.ShipMethods = calculationResult.ShipMethods.Where(c => availableShipMethods.Contains(c.ShipMethodID));


                if (PropertyBag.PaymentMethod == null) {
                    PropertyBag.PaymentTypeID = 999;
                }

                if (PropertyBag.PaymentMethod != null && PropertyBag.PaymentMethod.CanBeParsedAs<CreditCard>())
                {
                    var creditcard = PropertyBag.PaymentMethod.As<CreditCard>();
                    creditcard.CardNumber = "";

                    // Capture the payment method
                    model.PaymentMethod = creditcard;
                }
                else
                {
                    model.PaymentMethod = PropertyBag.PaymentMethod;
                }


                // Set the default ship method
                if (PropertyBag.ShipMethodID != 0)
                {
                    shipMethodID = PropertyBag.ShipMethodID;
                }
                else
                {
                    shipMethodID = OrderConfiguration.DefaultShipMethodID;
                    PropertyBag.ShipMethodID = OrderConfiguration.DefaultShipMethodID;
                    Exigo.PropertyBags.Update(PropertyBag);
                }

                model.ShipMethodID = shipMethodID;

                if (model.ShipMethods != null)
                {

                    if (model.ShipMethods.Any(c => c.ShipMethodID == shipMethodID))
                    {
                        model.ShipMethods.First(c => c.ShipMethodID == shipMethodID).Selected = true;
                    }
                    else
                    {
                        // If we don't have the ship method we're supposed to select, 
                        // check the first one, save the selection and recalculate
                        model.ShipMethods.First().Selected = true;

                        PropertyBag.ShipMethodID = model.ShipMethods.First().ShipMethodID;
                        Exigo.PropertyBags.Update(PropertyBag);

                        var newCalculationResult = Exigo.CalculateOrder(new OrderCalculationRequest
                        {
                            Configuration = OrderConfiguration,
                            Items = cartItems,
                            Address = PropertyBag.ShippingAddress,
                            ShipMethodID = PropertyBag.ShipMethodID,
                            ReturnShipMethods = false
                        });

                        model.Totals = newCalculationResult;
                    }
                }


                return View(model);
            }
            catch (Exception exception)
            {
                ViewBag.Error = "We are currently expereriencing intermittent technical isues with our Enrollment Process. We apologize for any inconvience. Please contact our customer service at" + GlobalSettings.Company.Phone + " or by email at " + GlobalSettings.Company.Email + " for assisance.";


                return View(model);
            }
        }

        public ActionResult IdealAutoOrderPayment(int OrderID)
        {
            var selectedCountry = GlobalUtilities.GetSelectedCountryCode();
            var paymentProvider = PaymentService.GetPaymentProvider(selectedCountry);
            var order = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest()
            {
                CustomerID = Identity.Customer.CustomerID,
                OrderID = OrderID,
                IncludeOrderDetails = true
            }).FirstOrDefault();

            if (paymentProvider.HandlerType == PaymentHandlerType.Remote)
            {
                paymentProvider.OrderConfiguration = OrderConfiguration;
                paymentProvider.Order = order;

                // Get the request data
                var paymentRequest = paymentProvider.GetPaymentRequest(new PaymentRequestArgs() { ReturnUrl = PaymentRedirectURL, WebAlias = Identity.Owner.WebAlias });

                // Handle the request
                var postPaymentRequest = paymentRequest as POSTPaymentRequest;
                if (postPaymentRequest != null)
                {
                    return new JsonNetResult(new
                    {
                        success = true,
                        redirectForm = postPaymentRequest.RequestForm
                    });
                }
                else
                {
                    return new JsonNetResult(new
                    {
                        success = false
                    });
                }
            }
            else
            {
                return new JsonNetResult(new
                {
                    success = false
                });
            }
        }

        public ActionResult UsePaymentMethod(IPaymentMethod paymentMethod, int PaymentTypeID = 0)
        {
            try
            {
                // Check for redirect payment type : 999 = Ideal
                if (PaymentTypeID == 999)
                {
                    var ideal = paymentMethod.As<Ideal>();
                    PropertyBag.PaymentMethod = ideal;
                    PropertyBag.PaymentTypeID = 999;
                    PropertyBag.IsRedirectPayment = true;
                    Exigo.PropertyBags.Update(PropertyBag);

                }
                else
                {
                    var message = "";

                    if (paymentMethod.CanBeParsedAs<CreditCard>())
                    {
                        var creditCard = paymentMethod.As<CreditCard>();
                        PropertyBag.PaymentMethod = creditCard;
                        PropertyBag.IsRedirectPayment = false;
                        Exigo.PropertyBags.Update(PropertyBag);

                        if (creditCard.IsExpired)
                        {
                            message = "Please enter a card that is not expired before proceeding.";

                            return new JsonNetResult(new
                            {
                                success = false,
                                message = message
                            });
                        }

                        if (!creditCard.IsComplete)
                        {
                            message = "Please enter a card that is complete before proceeding.";

                            return new JsonNetResult(new
                            {
                                success = false,
                                message = message
                            });
                        }

                        //if (paymentMethod.IsValid)
                        //{
                        //    PropertyBag.PaymentMethod = paymentMethod;
                        //    Exigo.PropertyBags.Update(PropertyBag);

                        //}
                    }
                }

                return new JsonNetResult(new
                {
                    success = true
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
        #endregion

        #region Submission and Complete
        [HttpPost]
        public ActionResult SubmitEnrollment()
        {
            var isRedirectPayment = PropertyBag.IsRedirectPayment;

            try
            {
                // Start creating the API requests
                var apiRequests = new List<ApiRequest>();

                // Create the customer
                var customerRequest = new CreateCustomerRequest(PropertyBag.Customer);
                customerRequest.InsertEnrollerTree = true;
                customerRequest.InsertUnilevelTree = true;
                customerRequest.CustomerType = CustomerTypes.BrandPartner;
                customerRequest.EnrollerID = Identity.Owner.CustomerID;
                customerRequest.SponsorID = Identity.Owner.CustomerID;
                customerRequest.EntryDate = DateTime.Now;
                customerRequest.CustomerStatus = CustomerStatuses.Active;
                customerRequest.CanLogin = true;
                customerRequest.LoginName = PropertyBag.Customer.LoginName;
                customerRequest.Company = PropertyBag.Customer.PublicName;
                customerRequest.CurrencyCode = OrderConfiguration.CurrencyCode;
                customerRequest.PayableType = PayableType.Check;
                customerRequest.LoginPassword = PropertyBag.Customer.Password;
                customerRequest.Notes = "Distributor was entered by Distributor #{0}. Created by the API Enrollment at ".FormatWith(Identity.Owner.CustomerID) + HttpContext.Request.Url.Host + HttpContext.Request.Url.LocalPath + " on " + DateTime.Now.ToString("dddd, MMMM d, yyyy h:mmtt") + " CST at IP " + Common.GlobalUtilities.GetClientIP() + " using " + HttpContext.Request.Browser.Browser + " " + HttpContext.Request.Browser.Version + " (" + HttpContext.Request.Browser.Platform + ").";
                apiRequests.Add(customerRequest);

                // Set a few variables up for our shippping address, order/auto order items and the default auto order payment type
                var shippingAddress = PropertyBag.ShippingAddress;
                shippingAddress.FirstName = PropertyBag.Customer.FirstName;
                shippingAddress.LastName = PropertyBag.Customer.LastName;
                shippingAddress.Phone = PropertyBag.Customer.PrimaryPhone;
                shippingAddress.Email = PropertyBag.Customer.Email;

                var orderItems = ShoppingCart.Items.Where(i => i.Type == ShoppingCartItemType.EnrollmentPack).ToList();

                // Create initial order
                var orderRequest = new CreateOrderRequest(OrderConfiguration, PropertyBag.ShipMethodID, orderItems, shippingAddress);

                if (isRedirectPayment)
                {
                    orderRequest.OrderStatus = OrderStatusType.Pending;
                }

                // Add the new credit card to the customer's record and charge it for the current order
                if (PropertyBag.PaymentMethod.CanBeParsedAs<CreditCard>())
                {
                    var creditCard = PropertyBag.PaymentMethod.As<CreditCard>();

                    // If we are dealing with a test credit card, then we set the order as accepted to simulate an 'Accepted' order
                    if (!creditCard.IsTestCreditCard)
                    {
                        var chargeCCRequest = new ChargeCreditCardTokenRequest(creditCard);
                        apiRequests.Add(chargeCCRequest);

                        var saveCCRequest = new SetAccountCreditCardTokenRequest(creditCard);
                        apiRequests.Add(saveCCRequest);
                    }
                    else
                    {
                        orderRequest.OrderStatus = OrderStatusType.Shipped;
                    }
                }

                // Add order request now if we need to do any testing with the accepted functionality
                apiRequests.Add(orderRequest);


                // Process the transaction
                var transaction = new TransactionalRequest { TransactionRequests = apiRequests.ToArray() };
                var response = Exigo.WebService().ProcessTransaction(transaction);

                var newcustomerid = 0;
                var neworderid = 0;

                if (response.Result.Status == ResultStatus.Success)
                {
                    foreach (var apiresponse in response.TransactionResponses)
                    {
                        if (apiresponse.CanBeParsedAs<CreateCustomerResponse>()) newcustomerid = apiresponse.As<CreateCustomerResponse>().CustomerID;

                        if (apiresponse.CanBeParsedAs<CreateOrderResponse>()) neworderid = apiresponse.As<CreateOrderResponse>().OrderID;
                    }
                }


                // Update the customer web alias
                var propertyBagCustomer = PropertyBag.Customer;
                Task.Factory.StartNew(() =>
                {
                    var customerSiteRequest = new SetCustomerSiteRequest(propertyBagCustomer);
                    customerSiteRequest.CustomerID = newcustomerid;
                    customerSiteRequest.WebAlias = newcustomerid.ToString();
                    customerSiteRequest.FirstName = propertyBagCustomer.FirstName;
                    customerSiteRequest.LastName = propertyBagCustomer.LastName;
                    customerSiteRequest.Company = propertyBagCustomer.PublicName;
                    SetCustomerSiteResponse res = Exigo.WebService().SetCustomerSite(customerSiteRequest);
                });

                var token = Security.Encrypt(new { OrderID = neworderid, CustomerID = newcustomerid });

                if (PropertyBag.Customer.IsOptedIn) {
                    Exigo.SendEmailVerification(newcustomerid, propertyBagCustomer.Email);

                }

                var selectedCountry = PropertyBag.ShippingAddress.Country;


                // handle redirect payments
                if (isRedirectPayment)
                {
                    var paymentProvider = PaymentService.GetPaymentProvider(selectedCountry);
                    var order = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest()
                    {
                        CustomerID = newcustomerid,
                        OrderID = neworderid,
                        IncludeOrderDetails = true
                    }).FirstOrDefault();

                    if (paymentProvider.HandlerType == PaymentHandlerType.Remote)
                    {
                        paymentProvider.OrderConfiguration = OrderConfiguration;
                        paymentProvider.Order = order;
                        paymentProvider.Order.ShipMethodID = PropertyBag.ShipMethodID;


                    }

                    var billingAddress = new Address()
                    {
                        AddressType = AddressType.Other,
                        Address1 = order.Recipient.Address1,
                        Address2 = order.Recipient.Address2,
                        City = order.Recipient.City,
                        State = order.Recipient.State,
                        Zip = order.Recipient.Zip,
                        Country = order.Recipient.Country
                    };

                    // Get the request data
                    var paymentRequest = paymentProvider.GetPaymentRequest(new PaymentRequestArgs() { ReturnUrl = PaymentRedirectURL, BillingName = order.Recipient.FullName, BillingAddress = billingAddress, WebAlias = Identity.Owner.WebAlias });

                    // Handle the request
                    var postPaymentRequest = paymentRequest as POSTPaymentRequest;
                    if (postPaymentRequest != null)
                    {
                        return new JsonNetResult(new
                        {
                            success = true,
                            redirectForm = postPaymentRequest.RequestForm
                        });
                    }
                    else
                    {
                        return new JsonNetResult(new
                        {
                            success = false,
                        });
                    }

                    //if (paymentProvider.HandlerType == PaymentHandlerType.Remote)
                    //{
                    //    //Exigo.PropertyBags.Delete(PropertyBag);
                    //    Exigo.PropertyBags.Delete(ShoppingCart);

                    //    paymentProvider.OrderConfiguration = OrderConfiguration;
                    //    paymentProvider.Order = order;
                    //    paymentProvider.Order.ShipMethodID = PropertyBag.ShipMethodID;

                    //    // Get the request data
                    //    var paymentRequest = paymentProvider.GetPaymentRequest(new PaymentRequestArgs() { ReturnUrl = PaymentRedirectURL, WebAlias = Identity.Owner.WebAlias, BillingAddress = PropertyBag.Customer.MainAddress });

                    //    // Handle the request
                    //    var postPaymentRequest = paymentRequest as POSTPaymentRequest;
                    //    if (postPaymentRequest != null)
                    //    {
                    //        Exigo.PropertyBags.Delete(PropertyBag);
                            
                    //        return new JsonNetResult(new
                    //        {
                    //            success = true,
                    //            redirectForm = postPaymentRequest.RequestForm
                    //        });
                    //    }
                    //}

                    //    return new JsonNetResult(new
                    //    {
                    //    success = false,
                    //    message = "redirect failed"
                    //});
                }

            //    // Enrollment complete, now delete the Property Bag
                Exigo.PropertyBags.Delete(PropertyBag);
                Exigo.PropertyBags.Delete(ShoppingCart);

                return new JsonNetResult(new { token = token, success = true });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new { message = ex.Message, success = false });
            }
        }

        public ActionResult EnrollmentComplete(string token)
        {
            var model = new EnrollmentCompleteViewModel();
            var args = Security.Decrypt(token);

            model.CustomerID = Convert.ToInt32(args.CustomerID);
            model.OrderID = Convert.ToInt32(args.OrderID);
            model.AutoOrderID = Convert.ToInt32(args.AutoOrderID);

            try
            {
                model.Order = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
                {
                    CustomerID = model.CustomerID,
                    OrderID = model.OrderID,
                    IncludeOrderDetails = true,
                    IncludePayments = true
                }).FirstOrDefault();

            }
            catch
            {
                model.Order = null;
            }

            return View(model);
        }
        #endregion

        #region Ajax Helpers
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

        [HttpPost]
        public JsonNetResult UpdatePackItems(string itemcode, string action, int packType)
        {
            try
            {
                if (packType == (int)ShoppingCartItemType.EnrollmentPack)
                {
                    ShoppingCart.Items.Remove(ShoppingCartItemType.EnrollmentPack);

                    if (action == "add")
                    {
                        ShoppingCart.Items.Add(new ShoppingCartItem()
                        {
                            ItemCode = itemcode,
                            Quantity = 1,
                            Type = ShoppingCartItemType.EnrollmentPack
                        });
                    }

                    Exigo.PropertyBags.Update(ShoppingCart);

                    return new JsonNetResult(new
                    {
                        success = true
                    });
                }
                else if (packType == (int)ShoppingCartItemType.EnrollmentAutoOrderPack)
                {
                    ShoppingCart.Items.Remove(ShoppingCartItemType.EnrollmentAutoOrderPack);

                    if (action == "add")
                    {
                        ShoppingCart.Items.Add(new ShoppingCartItem()
                        {
                            ItemCode = itemcode,
                            Quantity = 1,
                            Type = ShoppingCartItemType.EnrollmentAutoOrderPack
                        });
                    }

                    Exigo.PropertyBags.Update(ShoppingCart);

                    return new JsonNetResult(new
                    {
                        success = true
                    });
                }
                else
                {
                    return new JsonNetResult(new
                    {
                        success = false
                    });
                }

            }
            catch (Exception)
            {
                return new JsonNetResult(new
                {
                    success = false
                });
            }
        }

        public JsonNetResult UpdateItemSummary()
        {
            try
            {
                var model = new EnrollmentSummaryViewModel();
                var orderItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order || c.Type == ShoppingCartItemType.EnrollmentPack).ToList();
                var order = Exigo.CalculateOrder(new OrderCalculationRequest
                {
                    Configuration = OrderConfiguration,
                    Items = orderItems,
                    Address = PropertyBag.ShippingAddress,
                    ShipMethodID = PropertyBag.ShipMethodID,
                    ReturnShipMethods = true
                });

                model.OrderEnrollmentPacks = Exigo.GetItems(ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.EnrollmentPack), OrderConfiguration);
                var autoOrderSubtotal = model.AutoOrderItems.Sum(c => c.Price * c.Quantity);
                var autoOrderPackSubtotal = model.AutoOrderEnrollmentPacks.Sum(c => c.Price * c.Quantity);
                model.AutoOrderSubtotal = autoOrderPackSubtotal + autoOrderSubtotal;
                model.OrderSubtotal = order.Subtotal;
                model.Total = order.Total;
                model.Shipping = order.Shipping;
                model.Tax = order.Tax;

                return new JsonNetResult(new
                {
                    success = true,
                    orderitems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order),
                    autoorderitems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder),
                    html = this.RenderPartialViewToString("_EnrollmentSummary", model)
                });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        public JsonNetResult AddItemToCart(Item item)
        {
            try
            {
                ShoppingCart.Items.Add(item);
                Exigo.PropertyBags.Update(ShoppingCart);

                return new JsonNetResult(new
                {
                    success = true
                });
            }
            catch (Exception)
            {

                return new JsonNetResult(new
                {
                    success = false
                });
            }

        }

        public ActionResult UpdateItemQuantity(string itemcode, string type, decimal quantity)
        {
            try
            {
                var itemType = ConvertItemType(type);
                var item = ShoppingCart.Items.Where(c => c.ItemCode == itemcode && c.Type == itemType).FirstOrDefault();

                ShoppingCart.Items.Update(item.ID, quantity);
                Exigo.PropertyBags.Update(ShoppingCart);

                return new JsonNetResult(new
                {
                    success = true
                });
            }
            catch (Exception)
            {

                return new JsonNetResult(new
                {
                    success = false
                });
            }
        }

        public JsonNetResult DeleteItemFromCart(string itemcode, string type)
        {
            try
            {
                var itemType = ConvertItemType(type);
                var itemID = ShoppingCart.Items.Where(c => c.ItemCode == itemcode && c.Type == itemType).FirstOrDefault().ID;

                ShoppingCart.Items.Remove(itemID);
                Exigo.PropertyBags.Update(ShoppingCart);

                return new JsonNetResult(new
                {
                    success = true
                });
            }
            catch (Exception)
            {

                return new JsonNetResult(new
                {
                    success = false
                });
            }
        }

        [HttpPost]
        public ActionResult SetShipMethodID(int shipMethodID)
        {
            var model = EnrollmentViewModelFactory.Create<EnrollmentReviewViewModel>(PropertyBag);

            try {
                model.ShipMethodID = shipMethodID;

                var orderItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order || c.Type == ShoppingCartItemType.EnrollmentPack).ToList();
                model.Totals = Exigo.CalculateOrder(new OrderCalculationRequest
                {
                    Configuration = OrderConfiguration,
                    Items = orderItems,
                    Address = PropertyBag.ShippingAddress,
                    ShipMethodID = shipMethodID,
                    ReturnShipMethods = true
                });

                model.Items = Exigo.GetItems(ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.EnrollmentPack), OrderConfiguration);

                PropertyBag.ShipMethodID = shipMethodID;
                Exigo.PropertyBags.Update(PropertyBag);

                var html = this.RenderPartialViewToString("_EnrollmentSummary", model);

                return new JsonNetResult(new
                {
                    success = true,
                    html = html
                });
            }
            catch(Exception exception) {
                return new JsonNetResult(new
                {
                    success = false,
                    error = exception.Message
                });
            }

        }

        [HttpPost]
        public ActionResult GetEnrollmentSummary()
        {
            var model = EnrollmentViewModelFactory.Create<EnrollmentReviewViewModel>(PropertyBag);

            try
            {
                model.ShipMethodID = PropertyBag.ShipMethodID;

                var orderItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order || c.Type == ShoppingCartItemType.EnrollmentPack).ToList();
                model.Totals = Exigo.CalculateOrder(new OrderCalculationRequest
                {
                    Configuration = OrderConfiguration,
                    Items = orderItems,
                    Address = PropertyBag.ShippingAddress,
                    ShipMethodID = PropertyBag.ShipMethodID,
                    ReturnShipMethods = true
                });

                model.Items = Exigo.GetItems(ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.EnrollmentPack), OrderConfiguration);

                var html = this.RenderPartialViewToString("_EnrollmentSummary", model);

                return new JsonNetResult(new
                {
                    success = true,
                    html = html
                });
            }
            catch (Exception exception)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    error = exception.Message
                });
            }

        }

        [OutputCache(Duration = 600, VaryByParam = "itemcategoryid")]
        public JsonNetResult GetItems(int itemcategoryid)
        {
            var items = Exigo.GetItems(new ExigoService.GetItemsRequest
            {
                Configuration = OrderConfiguration,
                CategoryID = itemcategoryid
            }).ToList();

            ViewBag.Category = Exigo.GetItemCategory(itemcategoryid);


            var html = this.RenderPartialViewToString("_ProductList", items);


            return new JsonNetResult(new
            {
                success = true,
                items = items,
                html = html
            });
        }
        #endregion

        #region Helpers
        public ShoppingCartItemType ConvertItemType(string type)
        {
            var itemType = new ShoppingCartItemType();
            switch (type)
            {
                case "Order":
                    itemType = ShoppingCartItemType.Order;
                    break;
                case "AutoOrder":
                    itemType = ShoppingCartItemType.AutoOrder;
                    break;
                case "EnrollmentPack":
                    itemType = ShoppingCartItemType.EnrollmentPack;
                    break;
                case "EnrollmentAutoOrderPack":
                    itemType = ShoppingCartItemType.EnrollmentAutoOrderPack;
                    break;
                default:
                    break;
            }
            return itemType;
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
                    else return "http://exigodemo.azurewebsites.net/" + this.WebAlias;
                }
            }
            public string MainState { get; set; }
            public string MainCity { get; set; }
            public string MainCountry { get; set; }
        }
        #endregion

        #region Redirect Payments
        //CURRENTLY USES APP CONTROLLER PAYMENT LANDING
        //[Route("payment-landing")]
        //public ActionResult PaymentRedirectLanding(string status)
        //{
        //    try
        //    {
        //        var provider = PaymentService.GetPaymentProvider(Request.Url);
        //        var result = provider.GetResponse(Request.Url, Request.Form);

        //        if (Request.HttpMethod == "GET")
        //        {
        //            if (result.Status == PaymentStatus.Success)
        //            {
        //                return RedirectToAction("EnrollmentComplete", new { token = result.Token });
        //            }
        //            else
        //            {
        //                return View(result);
        //            }
        //        }
        //        else
        //        {
        //            var paymentProvider = PaymentService.GetPaymentProviderByID(result.ProviderType);
        //            var order = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest()
        //            {
        //                CustomerID = Convert.ToInt32(result.CustomerID),
        //                OrderID = result.OrderID,
        //                IncludeOrderDetails = true
        //            }).FirstOrDefault();
        //            var attempt = result.Attempt + 1;

        //            paymentProvider.OrderConfiguration = OrderConfiguration;
        //            paymentProvider.Order = order;


        //            // Get the request data
        //            var paymentRequest = paymentProvider.GetPaymentRequest(new PaymentRequestArgs() { ReturnUrl = PaymentRedirectURL, Attempt = attempt });

        //            // Handle the request
        //            if (paymentRequest.Method == PaymentRequestMethod.Get)
        //            {
        //                return Redirect(paymentRequest.RequestUrl);
        //            }
        //            else
        //            {
        //                var postPaymentRequest = paymentRequest as POSTPaymentRequest;
        //                if (postPaymentRequest != null)
        //                {
        //                    return Content(postPaymentRequest.RequestForm, "text/html");
        //                }
        //            }

        //            return new JsonNetResult(new
        //            {
        //                success = false
        //            });
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        #endregion
    }
}

