using Common;
using Common.Models;
using Common.Providers;
using Common.Services;
using ExigoService;
using ReplicatedSite;
using ReplicatedSite.Models;
using ReplicatedSite.Providers;
using ReplicatedSite.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Net.Mail;
using Payments;

namespace ReplicatedSite.Controllers
{
    [RoutePrefix("{webalias}")]
    public class HomeController : Controller
    {
        #region Properties
        public string ShoppingCartName = "ReplicatedSiteShopping";

        public IOrderConfiguration OrderConfiguration = (Identity.Customer == null) ? GlobalUtilities.GetMarketConfigurationByCountry().Orders : ((Identity.Customer.CustomerTypeID == CustomerTypes.SmartShopper) ? GlobalUtilities.GetMarketConfigurationByCountry().AutoOrders : GlobalUtilities.GetMarketConfigurationByCountry().Orders);
        public IAutoOrderConfiguration AutoOrderConfiguration = GlobalUtilities.GetMarketConfigurationByCountry().AutoOrders;

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

        public ILogicProvider LogicProvider
        {
            get
            {
                if (_logicProvider == null)
                {
                    _logicProvider = new ShoppingCartLogicProvider(this, ShoppingCart, PropertyBag);
                }
                return _logicProvider;
            }
        }
        private ILogicProvider _logicProvider;
        #endregion


        public ActionResult Index()
        {
            var model = new ItemListViewModel();
            var categoryID = OrderConfiguration.FeaturedProductsCategoryID;
            var configuration = (Identity.Customer == null) ? Identity.Owner.Market.Configuration.Orders : Identity.Customer.Market.Configuration.Orders;

            model.Items = Exigo.GetItems(new ExigoService.GetItemsRequest
            {
                Configuration = OrderConfiguration,
                IncludeChildCategories = true,
                CategoryID = categoryID
            }).ToList();
            model.Items.ToList().ForEach(c => c.Quantity = 1);


            return View(model);
        }

        [Route("about")]
        public ActionResult AboutWIN()
        {
            return View();
        }

        [Route("opportunity")]
        public ActionResult Opportunity()
        {
            return RedirectToAction("Index", "Enrollment");
        }


        public ActionResult ProductInfo()
        {
            return RedirectToAction("Index", "Shopping");
        }

        public ActionResult FAQ()
        {
            return View();
        }

        public ActionResult Terms()
        {
            return View();
        }

        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        [Route("contact")]
        public ActionResult ContactUs(string Message)
        {
            var model = new Email();
            ViewBag.Error = Message;

            return View(model);
        }

        [HttpPost]
        [Route("contact")]
        public ActionResult ContactUs(Email form)
        {
            var model = new Email();
            try
            {
                // Add Form data to our model
                model.FirstName = form.FirstName;
                model.LastName = form.LastName;
                model.Comments = form.Comments;
                model.ReferringBrandPartner = form.ReferringBrandPartner;
                model.PreferredContactType = form.PreferredContactType;
                model.AlternateContactType = form.AlternateContactType;
                model.PreferredContact = form.PreferredContact;
                model.AlternateContact = form.AlternateContact;
                model.MessageType = form.MessageType;

                // Contact info form values for us to parse into Preferred and Alternate Contact Method fields
                model.EmailAddress = form.EmailAddress;
                model.PhoneNumber = form.PhoneNumber;

                if (model.PreferredContact != null)
                {
                    if (model.PreferredContactType == Email.ContactType.Email && model.EmailAddress != null)
                    {
                        model.EmailAddress = model.PreferredContact;
                    }
                    //else if (model.PreferredContactType == Email.ContactType.Phone && model.PhoneNumber != null)
                    else
                    {
                        model.PhoneNumber = model.PreferredContact;
                    }
                }

                if (model.AlternateContact != null)
                {
                    if (model.AlternateContactType == Email.ContactType.Email && model.EmailAddress != null)
                    {
                        model.EmailAddress = model.AlternateContact;
                    }
                    //else if (model.PreferredContactType == Email.ContactType.Phone && model.PhoneNumber != null)
                    else
                    {
                        model.PhoneNumber = model.AlternateContact;
                    }
                }

                if (ModelState.IsValid)
                {
                    var mailConfig = Common.GlobalSettings.Emails.SMTPConfigurations.Default;

                    MailMessage mail = new MailMessage();
                    mail.To.Add(new MailAddress(GlobalSettings.Company.Email)); // The correct email address to use - but we have to identify why the contact from won't deliver to it.
                    //mail.To.Add(new MailAddress("br@winltd.com")); //Alternate provided by the WIN Team for the time being
                    //mail.To.Add(new MailAddress("alanc@exigo.com")); //For testing purposes
                    mail.From = new MailAddress(GlobalSettings.Emails.NoReplyEmail);
                    //mail.From = new MailAddress("alanc@exigo.com"); //For testing only
                    mail.Subject = "Online Contact Form: [" + model.FirstName + " " + model.LastName + " ]";

                    mail.IsBodyHtml = true;
                    mail.Body = "<html><body>"
                        + "<p>"
                        + "An inquiry was submitted from " + model.FirstName + " " + model.LastName
                        + " on " + DateTime.Now.Date.ToShortDateString()
                        + "</p>"
                        + "<h4>Customer Information:</h4>"
                        + "<p><strong>Name</strong> "
                        + model.FirstName
                        + " "
                        + model.LastName
                        + "<br /> <strong>Inquiry Type:</strong> "
                        + "<br />" + model.MessageType
                        + "<br /> <strong>Preferred Contact Method:</strong> " + model.PreferredContactType + " - " + model.PreferredContact
                        + "<br /> <strong>Alternate Contact Method:</strong> " + model.AlternateContactType + " - " + model.AlternateContact
                        + "<br /> <strong>Referring Brand Partner:</strong> "
                        + "<br />" + model.ReferringBrandPartner
                        + "</p>"
                        + "<p>" + "<h4>Inquiry Message:</h4>" + "</p>"
                        + "<p>" + model.Comments + "</p>"
                        + "</html></body>";
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = mailConfig.Server;
                    smtp.Port = mailConfig.Port;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential
                    (mailConfig.Username, mailConfig.Password);
                    smtp.EnableSsl = mailConfig.EnableSSL;
                    smtp.Send(mail);
                    return RedirectToAction("ContactSuccess", new { Message = "Your form has been successfully submitted" });
                }
                else
                {
                    return RedirectToAction("ContactUs", new { Message = "Error - Please check the form for completion" });
                }
            }
            catch (Exception exception)
            {
                ViewBag.Error = exception.Message;

                return RedirectToAction("ContactUs", new { Message = "An unexpected error has occurred. We sincerely apologize. Please try again later." });
            }
        }

        [Route("success")]
        public ActionResult ContactSuccess()
        {

            return View();
        }

        //This Blog page was for testing only - currently not being used as customer provided link to blog and not the embed - if they ask for embedded blog this is ready to go

        [Route("blog")]
        public ActionResult Blog()
        {
            return View();
        }

        public ActionResult TestRedirect()
        {
            var newcustomerid = 3;
            var neworderid = 286;

            var paymentProvider = PaymentService.GetPaymentProvider("NL");
            var order = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest()
            {
                CustomerID = newcustomerid,
                OrderID = neworderid,
                IncludeOrderDetails = true
            }).FirstOrDefault();

            if (paymentProvider.HandlerType == PaymentHandlerType.Remote)
            {
                //Exigo.PropertyBags.Delete(PropertyBag);
                //Exigo.PropertyBags.Delete(ShoppingCart);

                paymentProvider.OrderConfiguration = OrderConfiguration;
                paymentProvider.Order = order;

                // Get the request data
                var paymentRequest = paymentProvider.GetPaymentRequest(new PaymentRequestArgs() { ReturnUrl = "enrollment/paymentredirectlanding", WebAlias = Identity.Owner.WebAlias });

                // Handle the request
                if (paymentRequest.Method == PaymentRequestMethod.Get)
                {
                    return Redirect(paymentRequest.RequestUrl);
                }
                else
                {
                    var postPaymentRequest = paymentRequest as POSTPaymentRequest;
                    if (postPaymentRequest != null)
                    {
                        return new JsonNetResult(new { content = postPaymentRequest.RequestForm });
                    }
                }
            }

            return new JsonNetResult(new
            {
                success = false,
                message = "redirect failed"
            });
        }

        public ActionResult ReturnPolicy()
        {
            return View();
        }
    }
}
