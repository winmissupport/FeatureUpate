using Common;
using Common.Api.ExigoWebService;
using Common.Services;
using ExigoService;
using Newtonsoft.Json;
using Payments;
using ReplicatedSite.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Linq;
using System.Web.Mvc;

namespace ReplicatedSite.Controllers
{
    public class AppController : Controller
    {
        #region Global & Local Resources
        [AllowAnonymous]
        public JavaScriptResult Resource(string name = "resources", string path = "Resources")
        {
            // Clean up any references to .resx - our code enters that automatically.
            if (path.Contains(".resx")) path = path.Replace(".resx", "");

            // Create our factory
            var service = new ClientResourceService();
            service.JavaScriptObjectName = name;
            service.GlobalResXFileName = path;

            // Write our javascript to the page.            
            return JavaScript(service.GetJavaScript());
        }
        #endregion

        #region Cultures
        [AllowAnonymous]
        public JavaScriptResult Culture()
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            var adsfads = JsonConvert.SerializeObject(currentCulture.NumberFormat);

            var result = new StringBuilder();
            result.AppendFormat(@"
                CultureInfo = function (c, b, a) {{
                    this.name = c;
                    this.numberFormat = b;
                    this.dateTimeFormat = a
                }};

                CultureInfo.prototype = {{
                    _getDateTimeFormats: function () {{
                        if (!this._dateTimeFormats) {{
                            var a = this.dateTimeFormat;
                            this._dateTimeFormats = [a.MonthDayPattern, a.YearMonthPattern, a.ShortDatePattern, a.ShortTimePattern, a.LongDatePattern, a.LongTimePattern, a.FullDateTimePattern, a.RFC1123Pattern, a.SortableDateTimePattern, a.UniversalSortableDateTimePattern]
                        }}
                        return this._dateTimeFormats
                    }},
                    _getIndex: function (c, d, e) {{
                        var b = this._toUpper(c),
                            a = Array.indexOf(d, b);
                        if (a === -1) a = Array.indexOf(e, b);
                        return a
                    }},
                    _getMonthIndex: function (a) {{
                        if (!this._upperMonths) {{
                            this._upperMonths = this._toUpperArray(this.dateTimeFormat.MonthNames);
                            this._upperMonthsGenitive = this._toUpperArray(this.dateTimeFormat.MonthGenitiveNames)
                        }}
                        return this._getIndex(a, this._upperMonths, this._upperMonthsGenitive)
                    }},
                    _getAbbrMonthIndex: function (a) {{
                        if (!this._upperAbbrMonths) {{
                            this._upperAbbrMonths = this._toUpperArray(this.dateTimeFormat.AbbreviatedMonthNames);
                            this._upperAbbrMonthsGenitive = this._toUpperArray(this.dateTimeFormat.AbbreviatedMonthGenitiveNames)
                        }}
                        return this._getIndex(a, this._upperAbbrMonths, this._upperAbbrMonthsGenitive)
                    }},
                    _getDayIndex: function (a) {{
                        if (!this._upperDays) this._upperDays = this._toUpperArray(this.dateTimeFormat.DayNames);
                        return Array.indexOf(this._upperDays, this._toUpper(a))
                    }},
                    _getAbbrDayIndex: function (a) {{
                        if (!this._upperAbbrDays) this._upperAbbrDays = this._toUpperArray(this.dateTimeFormat.AbbreviatedDayNames);
                        return Array.indexOf(this._upperAbbrDays, this._toUpper(a))
                    }},
                    _toUpperArray: function (c) {{
                        var b = [];
                        for (var a = 0, d = c.length; a < d; a++) b[a] = this._toUpper(c[a]);
                        return b
                    }},
                    _toUpper: function (a) {{
                        return a.split(""\u00a0"").join("" "").toUpperCase()
                    }}
                }};
                CultureInfo._parse = function (a) {{
                    var b = a.dateTimeFormat;
                    if (b && !b.eras) b.eras = a.eras;
                    return new CultureInfo(a.name, a.numberFormat, b)
                }};


                CultureInfo.InvariantCulture = CultureInfo._parse({{
                    {0}
                }});

                CultureInfo.CurrentCulture = CultureInfo._parse({{
                    {1}
                }});

            ", GetCultureInfoJson(currentCulture), GetCultureInfoJson(currentUICulture));


            return JavaScript(result.ToString());
        }
        private string GetCultureInfoJson(CultureInfo cultureInfo)
        {
            var result = new StringBuilder();

            result.AppendFormat(@"
                ""name"": ""{0}"",
                ""numberFormat"": {1},
                ""dateTimeFormat"": {2},
                ""eras"": [1, ""A.D."", null, 0]
            ",
                cultureInfo.Name,
                JsonConvert.SerializeObject(cultureInfo.NumberFormat),
                JsonConvert.SerializeObject(cultureInfo.DateTimeFormat));

            return result.ToString();
        }
        #endregion

        #region Countries & Regions
        [OutputCache(Duration = 86400)]
        public JsonNetResult GetCountries()
        {
            var countries = Exigo.GetCountries();

            return new JsonNetResult(new
            {
                success = true,
                countries = countries
            });
        }

        [OutputCache(VaryByParam = "id", Duration = 86400)]
        public JsonNetResult GetRegions(string id)
        {
            var regions = Exigo.GetRegions(id);

            return new JsonNetResult(new
            {
                success = true,
                regions = regions
            });
        }
        #endregion

        #region Avatars
        [Route("~/profiles/avatar/{id:int}/{type?}/{cache?}")]
        public FileResult Avatar(int id, AvatarType type = AvatarType.Default, bool cache = true)
        {
            var bytes = Exigo.Images().GetCustomerAvatar(id, type, cache);

            // Return the image
            return File(bytes, "application/png", "{0}.png".FormatWith(id));
        }
        #endregion

        #region Globalization
        public ActionResult SetLanguagePreference(int id)
        {
            Exigo.SetCustomerPreferredLanguage(Identity.Customer.CustomerID, id);
            new IdentityService().RefreshIdentity();

            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }
        #endregion

        #region Validation
        public JsonResult VerifyAddress(Address address)
        {
            return Json(Exigo.VerifyAddress(address), JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsEmailAvailable([Bind(Prefix = "Customer.Email")]string Email)
        {
            return Json(Exigo.IsEmailAvailable(Email), JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsLoginNameAvailable([Bind(Prefix = "Customer.LoginName")]string UserName)
        {
            return Json(Exigo.IsLoginNameAvailable(UserName), JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsTaxIDAvailable([Bind(Prefix = "Customer.TaxID")]string TaxID)
        {
            return Json(Exigo.IsTaxIDAvailable(TaxID), JsonRequestBehavior.AllowGet);
        }

        // Think this needs to be removed
        public JsonResult IsLoginNameAvailable_Retail([Bind(Prefix = "UserName")]string UserName)
        {
            return Json(Exigo.IsLoginNameAvailable(UserName, Identity.Owner.CustomerID), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Redirects
        //[Route("ingenicopayment")]
        //public void PaymentRedirectLanding()
        //{
        //try
        //{


        // Temp
        //var provider = GlobalSettings.Payments.AvailablePaymentProviders.FirstOrDefault(c => c.ID == "Ingenico");
        ////var provider = PaymentService.GetPaymentProvider(Request.Url);
        //var result = provider.GetResponse(Request.Url, Request.Form);
        //if (result.Status == PaymentStatus.Success)
        //{

        //    var paymentType = Common.Api.ExigoWebService.PaymentType.Other1;
        //    if (result.PaymentType == "CreditCard")
        //    {
        //        paymentType = Common.Api.ExigoWebService.PaymentType.CreditCard;
        //    }

        //    //Set the order to accepted status                    
        //    var order = Exigo.OData().Orders.Where(c => c.OrderID == result.OrderID).FirstOrDefault();

        //    Exigo.WebService().CreatePayment(new CreatePaymentRequest()
        //    {
        //        OrderID = result.OrderID,
        //        Amount = order.Total,
        //        BillingName = order.FirstName + " " + order.LastName,
        //        PaymentDate = DateTime.Now,
        //        PaymentType = paymentType,
        //        Memo = "Deferred payment made for order ID : " + result.OrderID
        //    });

        //    Exigo.WebService().ChangeOrderStatus(new ChangeOrderStatusRequest()
        //    {
        //        OrderID = result.OrderID,
        //        OrderStatus = OrderStatusType.Accepted
        //    });

        //var context = Exigo.ODataLogging();
        //var log = context.Logs.Where(c => c.OrderID == result.OrderID).FirstOr(null);
        //if (log == null)
        //{
        //    context.AddToLogs(new Common.Api.ExigoOData.LoggingContext.Log { OrderID = result.OrderID, Response = Request.Url.Query.ToString(), ResponseDate = DateTime.Now });
        //    context.SaveChanges();

        //}
        //else
        //{
        //    log.Response = Request.Url.ToString();
        //    log.ResponseDate = DateTime.Now;
        //    context.UpdateObject(log);
        //    context.SaveChanges();
        //}
        //        }
        //    }

        //    catch (Exception e)
        //    {
        //        Console.Write(e);
        //    }
        //}

        //[Route("{webalias}/ingenicopayment")]
        //public void PaymentRedirectLandingRS()
        //{
        //    try
        //    {


        //        // Temp
        //        //var provider = GlobalSettings.Payments.AvailablePaymentProviders.FirstOrDefault(c => c.ID == "Ingenico");
        //        var provider = PaymentService.GetPaymentProvider(Request.Url);
        //        var result = provider.GetResponse(Request.Url, Request.Form);
        //        if (result.Status == PaymentStatus.Success)
        //        {

        //            var paymentType = Common.Api.ExigoWebService.PaymentType.Other1;
        //            if (result.PaymentType == "CreditCard")
        //            {
        //                paymentType = Common.Api.ExigoWebService.PaymentType.CreditCard;
        //            }

        //            //Set the order to accepted status                    
        //            var order = Exigo.OData().Orders.Where(c => c.OrderID == result.OrderID).FirstOrDefault();

        //            Exigo.WebService().CreatePayment(new CreatePaymentRequest()
        //            {
        //                OrderID = result.OrderID,
        //                Amount = order.Total,
        //                BillingName = order.FirstName + " " + order.LastName,
        //                PaymentDate = DateTime.Now,
        //                PaymentType = paymentType,
        //                Memo = "Deferred payment made for order ID : " + result.OrderID
        //            });

        //            Exigo.WebService().ChangeOrderStatus(new ChangeOrderStatusRequest()
        //            {
        //                OrderID = result.OrderID,
        //                OrderStatus = OrderStatusType.Accepted
        //            });

        //            //var context = Exigo.ODataLogging();
        //            //var log = context.Logs.Where(c => c.OrderID == result.OrderID).FirstOr(null);
        //            //if (log == null)
        //            //{
        //            //    context.AddToLogs(new Common.Api.ExigoOData.LoggingContext.Log { OrderID = result.OrderID, Response = Request.Url.Query.ToString(), ResponseDate = DateTime.Now });
        //            //    context.SaveChanges();

        //            //}
        //            //else
        //            //{
        //            //    log.Response = Request.Url.ToString();
        //            //    log.ResponseDate = DateTime.Now;
        //            //    context.UpdateObject(log);
        //            //    context.SaveChanges();
        //            //}
        //        }
        //    }

        //    catch (Exception e)
        //    {
        //        Console.Write(e);
        //    }
        //}
        //[Route("payment-landing")]
        //public ActionResult PaymentRedirectLandingBO()
        //{
        //    var OrderConfiguration = Common.GlobalUtilities.GetMarketConfigurationByCountry().EnrollmentOrders;
        //    try
        //    {


        //        var provider = PaymentService.GetPaymentProvider(Request.Url);
        //        var result = provider.GetResponse(Request.Url, Request.Form);
        //        var type = Request.QueryString["type"];
        //        var controller = "";
        //        var page = "";

        //        if (result.ProviderType == "Ingenico")
        //        {
        //            var context = Exigo.ODataLogging();
        //            var log = context.Logs.Where(c => c.OrderID == result.OrderID).FirstOr(null);
        //            if (log == null)
        //            {
        //                context.AddToLogs(new Common.Api.ExigoOData.LoggingContext.Log { OrderID = result.OrderID, Response = Request.Url.ToString(), ResponseDate = DateTime.Now });
        //                context.SaveChanges();

        //            }
        //            else
        //            {
        //                log.Response = Request.Url.ToString();
        //                log.ResponseDate = DateTime.Now;
        //                context.UpdateObject(log);
        //                context.SaveChanges();
        //            }

        //            if (result.Status == PaymentStatus.Success)
        //            {

        //                var paymentType = Common.Api.ExigoWebService.PaymentType.Other1;
        //                if (result.PaymentType == "CreditCard")
        //                {
        //                    paymentType = Common.Api.ExigoWebService.PaymentType.CreditCard;
        //                }

        //                //Set the order to accepted status                    
        //                var order = Exigo.OData().Orders.Where(c => c.OrderID == result.OrderID).FirstOrDefault();

        //                Exigo.WebService().CreatePayment(new CreatePaymentRequest()
        //                {
        //                    OrderID = result.OrderID,
        //                    Amount = order.Total,
        //                    BillingName = order.FirstName + " " + order.LastName,
        //                    PaymentDate = DateTime.Now,
        //                    PaymentType = paymentType,
        //                    Memo = "Deferred payment made for order ID : " + result.OrderID
        //                });

        //                Exigo.WebService().ChangeOrderStatus(new ChangeOrderStatusRequest()
        //                {
        //                    OrderID = result.OrderID,
        //                    OrderStatus = OrderStatusType.Accepted
        //                });

        //                switch (type)
        //                {
        //                    case "repenroll":
        //                        controller = "enrollment";
        //                        page = "enrollmentcomplete";
        //                        break;
        //                    case "repshop":
        //                        controller = "shopping";
        //                        page = "ordercomplete";
        //                        break;
        //                    case "boshop":
        //                        Response.Redirect("http://backoffice.mywinlife.com/store/ordercomplete");
        //                        break;
        //                    case "backofficeenrollment":
        //                        Response.Redirect("http://backoffice.mywinlife.com/store/enrollmentcomplete");
        //                        break;
        //                    default:
        //                        break;
        //                }

        //                return RedirectToAction(page, controller, new { token = result.Token });
        //            }
        //            else
        //            {
        //                switch (type)
        //                {
        //                    case "repenroll":
        //                        controller = "home";
        //                        page = "index";
        //                        break;
        //                    case "repshop":
        //                        controller = "shopping";
        //                        page = "ordercompleteError";
        //                        break;
        //                    case "boshop":
        //                        Response.Redirect("http://backoffice.mywinlife.com/store/ordercompleteError");
        //                        break;
        //                    default:
        //                        break;
        //                }

        //                return RedirectToAction(page, controller, new { token = result.Token });
        //            }
        //        }
        //        else
        //        {
        //            switch (type)
        //            {
        //                case "repenroll":
        //                    controller = "home";
        //                    page = "index";
        //                    break;
        //                case "repshop":
        //                    controller = "shopping";
        //                    page = "ordercompleteError";
        //                    break;
        //                case "boshop":
        //                    Response.Redirect("http://backoffice.mywinlife.com/store/ordercompleteError");
        //                    break;
        //                default:
        //                    break;
        //            }

        //            return RedirectToAction(page, controller, new { token = result.Token });
        //        }

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        [Route("{webalias}/p-l")]
        public ActionResult PaymentRedirectLanding()
        {
            var OrderConfiguration = Common.GlobalUtilities.GetMarketConfigurationByCountry().EnrollmentOrders;
            try
            {


                var provider = PaymentService.GetPaymentProvider(Request.Url);
                var result = provider.GetResponse(Request.Url, Request.Form);
                var type = Request.QueryString["type"];
                var controller = "";
                var page = "";

                if (result.ProviderType == "Ingenico")
                {
                    var context = Exigo.ODataLogging();
                    var log = context.Logs.Where(c => c.OrderID == result.OrderID).FirstOr(null);
                    if (log == null)
                    {
                        context.AddToLogs(new Common.Api.ExigoOData.LoggingContext.Log { OrderID = result.OrderID, Response = Request.Url.ToString(), ResponseDate = DateTime.Now });
                        context.SaveChanges();

                    }
                    else
                    {
                        log.Response = Request.Url.ToString();
                        log.ResponseDate = DateTime.Now;
                        context.UpdateObject(log);
                        context.SaveChanges();
                    }

                    if (result.Status == PaymentStatus.Success)
                    {

                        var paymentType = Common.Api.ExigoWebService.PaymentType.Other1;
                        if (result.PaymentType == "CreditCard")
                        {
                            paymentType = Common.Api.ExigoWebService.PaymentType.CreditCard;
                        }

                        //Set the order to accepted status                    
                        var order = Exigo.OData().Orders.Where(c => c.OrderID == result.OrderID).FirstOrDefault();

                        Exigo.WebService().CreatePayment(new CreatePaymentRequest()
                        {
                            OrderID = result.OrderID,
                            Amount = order.Total,
                            BillingName = order.FirstName + " " + order.LastName,
                            PaymentDate = DateTime.Now,
                            PaymentType = paymentType,
                            Memo = "Deferred payment made for order ID : " + result.OrderID
                        });

                        Exigo.WebService().ChangeOrderStatus(new ChangeOrderStatusRequest()
                        {
                            OrderID = result.OrderID,
                            OrderStatus = OrderStatusType.Accepted
                        });

                        switch (type)
                        {
                            case "repenroll":
                                controller = "enrollment";
                                page = "enrollmentcomplete";
                                break;
                            case "repshop":
                                controller = "shopping";
                                page = "ordercomplete";
                                break;
                            case "boshop":
                                Response.Redirect("http://backoffice.mywinlife.com/store/ordercomplete");
                                break;
                            case "backofficeenrollment":
                                Response.Redirect("http://backoffice.mywinlife.com/store/enrollmentcomplete");
                                break;
                            default:
                                break;
                        }

                        return RedirectToAction(page, controller, new { token = result.Token });
                    }
                    else
                    {
                        switch (type)
                        {
                            case "RE":
                                controller = "home";
                                page = "index";
                                break;
                            case "RS":
                                controller = "shopping";
                                page = "ordercompleteError";
                                break;
                            case "BOS":
                                Response.Redirect("http://backoffice.mywinlife.com/store/ordercompleteError");
                                break;
                            default:
                                break;
                        }

                        return RedirectToAction(page, controller, new { token = result.Token });
                    }
                }
                else
                {
                    switch (type)
                    {
                        case "RE":
                            controller = "home";
                            page = "index";
                            break;
                        case "RS":
                            controller = "shopping";
                            page = "ordercompleteError";
                            break;
                        case "BOS":
                            Response.Redirect("http://backoffice.mywinlife.com/store/ordercompleteError");
                            break;
                        default:
                            break;
                    }

                    return RedirectToAction(page, controller, new { token = result.Token });
                }

            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
