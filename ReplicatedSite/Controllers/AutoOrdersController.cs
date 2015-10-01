using Common;
using Common.Api.ExigoWebService;
using ExigoService;
using ReplicatedSite;
using ReplicatedSite.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;


namespace ReplicatedSite.Controllers
{
    [RoutePrefix("{webalias}")]
    [Authorize]
    public class AutoOrdersController : Controller
    {

        #region Unused
        //[Route("~/autoorders")]
        //public ActionResult AutoOrderList()
        //{
        //    var model = Exigo.GetCustomerAutoOrders(new GetCustomerAutoOrdersRequest
        //    {
        //        CustomerID            = Identity.Current.CustomerID,
        //        IncludeDetails        = true,
        //        IncludePaymentMethods = true
        //    });


        //    return View(model);
        //}

        //public ActionResult ShippingAddress(int id)
        //{
        //    var model = new AutoOrderShippingAddressViewModel();

        //    model.Addresses = Exigo.GetCustomerAddresses(Identity.Current.CustomerID)
        //        .Where(c => c.IsComplete)
        //        .Select(c => c as ShippingAddress);

        //    return View(model);
        //}
        //[HttpPost]
        //public ActionResult ShippingAddress(ShippingAddress address, int id)
        //{
        //    Exigo.UpdateCustomerAutoOrderShippingAddress(Identity.Current.CustomerID, id, address);

        //    return RedirectToAction("AutoOrderList");
        //}

        //public ActionResult PaymentMethod(int id)
        //{
        //    var model = new AutoOrderPaymentMethodViewModel();

        //    model.PaymentMethods = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
        //    {
        //        CustomerID                       = Identity.Current.CustomerID,
        //        ExcludeInvalidMethods            = true,
        //        ExcludeNonAutoOrderPaymentMethods = true
        //    });

        //    return View(model);
        //}
        //[HttpPost]
        //public ActionResult UseCreditCardOnFile(CreditCard card, int id)
        //{
        //    Exigo.UpdateCustomerAutoOrderPaymentMethod(Identity.Current.CustomerID, id, 
        //        (card.Type == CreditCardType.Primary) 
        //            ? AutoOrderPaymentType.PrimaryCreditCard 
        //            : AutoOrderPaymentType.SecondaryCreditCard);

        //    return RedirectToAction("AutoOrderList");
        //}
        //[HttpPost]
        //public ActionResult UseBankAccountOnFile(BankAccount account, int id)
        //{
        //    Exigo.UpdateCustomerAutoOrderPaymentMethod(Identity.Current.CustomerID, id, AutoOrderPaymentType.CheckingAccount);

        //    return RedirectToAction("AutoOrderList");
        //}

        //public ActionResult DeleteAutoOrder(int id)
        //{
        //    Exigo.DeleteCustomerAutoOrder(Identity.Current.CustomerID, id);

        //    return RedirectToAction("AutoOrderList");
        //}
        #endregion

        #region Properties

        public IAutoOrderConfiguration AutoOrderConfiguration = GlobalUtilities.GetMarketConfigurationByCountry().AutoOrders;

        #endregion

        #region AutoOrder Preferences
        [Route("autoships")] // Client requested  phrase auto order be replaced with auto-ship - Alan C 4/1/15
        public ActionResult AutoOrderPreferences()
        {
            var context = Exigo.OData();

            var model = Exigo.GetCustomerAutoOrders(new GetCustomerAutoOrdersRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                IncludeDetails = true,
                IncludePaymentMethods = true
            });

            return View(model);
        }

        [Route("getautoordermodal")]
        public JsonNetResult GetAutoOrderModal(string orderid)
        {
            try
            {
                var context = Exigo.OData();
                int OrderID = Convert.ToInt32(orderid);
                AutoOrderAddEditCartViewModel model = new AutoOrderAddEditCartViewModel();

                var autoorder = (ExigoService.AutoOrder)Exigo.OData().AutoOrders.Expand("Details")
                    .Where(a => a.CustomerID == Identity.Customer.CustomerID)
                    .Where(a => a.AutoOrderID == Convert.ToInt32(orderid))
                    .First();

                foreach (var detail in autoorder.Details)
                {
                    detail.ImageUrl = context.Items.Where(c => c.ItemCode == detail.ItemCode).FirstOrDefault().TinyImageUrl;
                }

                model.AutoOrder = autoorder;

                var products = Exigo.GetItems(new ExigoService.GetItemsRequest
                {
                    Configuration = Identity.Customer.Market.Configuration.BackOfficeAutoOrders,
                    IncludeChildCategories = true
                }).ToList();

                var orderItems = autoorder.Details.ToList();
                var itemCodeList = orderItems.Select(c => c.ItemCode).ToList();

                products.Where(p => itemCodeList.Contains(p.ItemCode)).ToList().ForEach(p => products.Remove(p));
                model.ProductsList = products;

                var html = this.RenderPartialViewToString("../AutoOrders/DisplayTemplates/AutoOrderEditOrder", model);

                return new JsonNetResult(new
                {
                    html = html
                });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    exception = ex
                });
            }
        }

        public AutoOrderResponse GetAutoOrder(int autoorderid)
        {
            var context = Exigo.WebService();

            var autoOrder = context.GetAutoOrders(new GetAutoOrdersRequest
            {
                AutoOrderID = autoorderid,
                CustomerID = Identity.Customer.CustomerID
            });

            return autoOrder.AutoOrders[0];
        }

        public ActionResult UpdateAutoOrderDate(AutoOrderDateViewModel dateVM)
        {
            if (dateVM.NextDate > dateVM.CreatedDate)
            {
                try
                {
                    var autoorderid = dateVM.AutoorderID;

                    Exigo.UpdateCustomerAutoOrderRunDate(Identity.Customer.CustomerID, autoorderid, dateVM.NextDate);

                    var model = Exigo.GetCustomerAutoOrder(Identity.Customer.CustomerID, autoorderid);
                    var partial = RenderPartialViewToString("displaytemplates/autoorderrow", model);

                    return new JsonNetResult(new
                    {
                        success = true,
                        html = partial,
                        autoorderid = autoorderid
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
            else
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = "Please Select AValid Date"
                    //message = Resources.Common.PleaseSelectAValidDate
                });
            }
        }

        public ActionResult UpdateAutoOrderShippingAddress(ShippingAddress recipient)
        {
            if (!recipient.IsComplete)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = "Please Enter A CompleteAddress"
                    //message = Resources.Common.PleaseEnterACompleteAddress
                });
            }

            try
            {
                var autoorderid = Convert.ToInt32(Request.Form["autoorderid"]);

                Exigo.UpdateCustomerAutoOrderShippingAddress(Identity.Customer.CustomerID, autoorderid, recipient);

                // Get Partial to update the AutoOrder
                var model = Exigo.GetCustomerAutoOrder(Identity.Customer.CustomerID, autoorderid);
                var partial = RenderPartialViewToString("displaytemplates/autoorderrow", model);

                return new JsonNetResult(new
                {
                    success = true,
                    html = partial,
                    autoorderid = autoorderid
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

        public ActionResult UpdateAutoOrderShipMethod(int shipMethodID)
        {
            try
            {
                var autoorderid = Convert.ToInt32(Request.Form["autoorderid"]);

                Exigo.UpdateCustomerAutoOrderShipMethod(Identity.Customer.CustomerID, autoorderid, shipMethodID);

                // Get Partial to update the AutoOrder 
                var model = Exigo.GetCustomerAutoOrder(Identity.Customer.CustomerID, autoorderid);
                var partial = RenderPartialViewToString("displaytemplates/autoorderrow", model);

                return new JsonNetResult(new
                {
                    success = true,
                    html = partial,
                    autoorderid = autoorderid
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
        public JsonNetResult SetAutoOrderPaymentMethodPreference(int autoorderid, AutoOrderPaymentType type)
        {
            try
            {
                Exigo.UpdateCustomerAutoOrderPaymentMethod(Identity.Customer.CustomerID, autoorderid, type);

                var model = Exigo.GetCustomerAutoOrder(Identity.Customer.CustomerID, autoorderid);
                var partial = RenderPartialViewToString("displaytemplates/autoorderrow", model);

                return new JsonNetResult(new
                {
                    success = true,
                    html = partial,
                    autoorderid = autoorderid
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
        [Route("UpdateAutoOrderItems")]
        public JsonNetResult UpdateAutoOrderItems(int autoorderid, List<Item> products)
        {
            try
            {
                Exigo.UpdateCustomerAutoOrderItems(Identity.Customer.CustomerID, autoorderid, products);

                var model = Exigo.GetCustomerAutoOrder(Identity.Customer.CustomerID, autoorderid);
                var partial = RenderPartialViewToString("displaytemplates/autoorderrow", model);

                return new JsonNetResult(new
                {
                    success = true,
                    html = partial,
                    autoorderid = autoorderid
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

        #region Modules
        [HttpPost]
        public ActionResult FetchAutoOrderModule(int autoorderid, string module)
        {
            try
            {
                switch (module)
                {
                    case ".auto-order-shipping":
                        return FetchAutoOrderShippingModule(autoorderid);
                    case ".auto-order-shipmethod":
                        return FetchAutoOrderShipMethodModule(autoorderid);
                    case ".auto-order-cart":
                        return FetchEditAutoOrderOrderModule(autoorderid);
                    case ".auto-order-payment":
                        return FetchAutoOrderEditPaymentMethodModule(autoorderid);
                    case ".auto-order-date":
                        return FetchAutoOrderEditDateModule(autoorderid);
                    default:
                        return FetchAutoOrderShippingModule(autoorderid);
                }
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

        // Edit Order Module
        public ActionResult FetchEditAutoOrderOrderModule(int autoorderid)
        {
            var autoorder = Exigo.GetCustomerAutoOrder(Identity.Customer.CustomerID, autoorderid);

            var configuration = Identity.Customer.Market.Configuration.BackOfficeAutoOrders;
            var products = Exigo.GetItems(new ExigoService.GetItemsRequest
            {
                Configuration = configuration,
                IncludeChildCategories = true
            }).ToList();

            var orderItems = autoorder.Details.ToList();
            var itemCodeList = orderItems.Select(c => c.ItemCode).ToList();

            products.Where(p => itemCodeList.Contains(p.ItemCode)).ToList().ForEach(p => products.Remove(p));


            // Populate our model with the products and the Auto Order
            var model = new AutoOrderAddEditCartViewModel();
            model.AutoOrder = autoorder;
            model.ProductsList = products;


            string html = RenderPartialViewToString("displaytemplates/autoordereditorder", model);

            return new JsonNetResult(new
            {
                success = true,
                module = html
            });
        }

        // Shipping AutoOrder Module
        public ActionResult FetchAutoOrderShippingModule(int autoorderid)
        {
            var autoorder = Exigo.GetCustomerAutoOrder(Identity.Customer.CustomerID, autoorderid);

            string html = RenderPartialViewToString("displaytemplates/autoordershippingaddress", autoorder);

            return new JsonNetResult(new
            {
                success = true,
                module = html
            });
        }

        // Ship Method autoOrder Module
        public ActionResult FetchAutoOrderShipMethodModule(int autoorderid)
        {
            var autoorder = Exigo.OData().AutoOrders.Expand("Details")
                .Where(a => a.CustomerID == Identity.Customer.CustomerID)
                .Where(a => a.AutoOrderID == autoorderid)
                .First();

            var model = new AutoOrderShipMethodViewModel();
            model.AutoorderID = autoorderid;

            var address = new Address()
            {
                Address1 = autoorder.Address1,
                Address2 = autoorder.Address2,
                City = autoorder.City,
                State = autoorder.State,
                Zip = autoorder.Zip,
                Country = autoorder.Country
            };

            // We only have one flat rate Autoship ship method so this call does not work currently

            //var shipmethods = Exigo.CalculateOrder(new OrderCalculationRequest
            //{
            //    Address = address,
            //    Configuration = Identity.Customer.Market.Configuration.AutoOrders,
            //    Items = autoorder.Details.Select(c => new Item { ItemCode = c.ItemCode, Quantity = c.Quantity, Price = c.PriceEach }).ToList(),
            //    ReturnShipMethods = true
            //}).ShipMethods.Where(c => c.ShipMethodID == Identity.Customer.Market.Configuration.AutoOrders.DefaultShipMethodID);

            //foreach (var shipmethod in shipmethods)
            //{
            //    if (autoorder.ShipMethodID == shipmethod.ShipMethodID)
            //    {
            //        shipmethod.Selected = true;
            //    }
            //}

            //model.ShipMethods = shipmethods;

            model.SelectedShipMethod = new ShipMethod();
            model.SelectedShipMethod = Exigo.WebService().GetShipMethods(new GetShipMethodsRequest()
            {
                WarehouseID = AutoOrderConfiguration.WarehouseID,
                CurrencyCode = AutoOrderConfiguration.CurrencyCode,
                OrderSubTotal = autoorder.SubTotal
            }).ShipMethods.Where(s => s.ShipMethodID == AutoOrderConfiguration.DefaultShipMethodID).Select(s => (ShipMethod)s).First();	

            string html = RenderPartialViewToString("displaytemplates/autoordershipmethod", model);

            return new JsonNetResult(new
            {
                success = true,
                module = html
            });
        }

        // Edit Payment Method
        public ActionResult FetchAutoOrderEditPaymentMethodModule(int autoorderid)
        {
            var model = new AutoOrderPaymentViewModel();

            model.AutoorderID = autoorderid;
            model.PaymentMethods = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                ExcludeIncompleteMethods = true,
                ExcludeInvalidMethods = true
            });

            string html = RenderPartialViewToString("displaytemplates/autoordereditpaymentmethod", model);

            return new JsonNetResult(new
            {
                success = true,
                module = html
            });
        }

        // Edit Processing Date
        public ActionResult FetchAutoOrderEditDateModule(int autoorderid)
        {
            var autoorder = Exigo.WebService().GetAutoOrders(new GetAutoOrdersRequest
            {
                AutoOrderID = autoorderid,
                CustomerID = Identity.Customer.CustomerID
            }).AutoOrders[0];

            // Removed CreatedDate. This did not appear to be used  ~ Marshall 02/25/15
            //var createdDate = Exigo.OData().Customers.Where(c => c.CustomerID == Identity.Customer.CustomerID).FirstOrDefault().CreatedDate;
            var model = new AutoOrderDateViewModel();

            model.AutoorderID = autoorderid;
            model.Frequency = autoorder.Frequency;
            model.NextDate = autoorder.NextRunDate;
            //model.CreatedDate = createdDate;


            string html = RenderPartialViewToString("displaytemplates/autoordereditdate", model);

            return new JsonNetResult(new
            {
                success = true,
                module = html
            });
        }


        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;


            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
        #endregion

        #endregion
}

