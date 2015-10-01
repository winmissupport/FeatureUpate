using Common;
using Common.Api.ExigoWebService;
using Common.Filters;
using Common.Providers;
using Common.Services;
using ExigoService;
using Payments;
using ReplicatedSite.Factories;
using ReplicatedSite.Models;
using ReplicatedSite.Providers;
using ReplicatedSite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace ReplicatedSite.Controllers
{
    
    [RoutePrefix("{webalias}")]
    public class ShoppingController : Controller
    {
        #region Properties
        public string ShoppingCartName = "ReplicatedSiteShopping";
        public string PaymentRedirectURL = "p-l?type=RS"; // 18Sept2015 JWJ shortened payment-landing to p-l and RepShop to RS due to Ingenico URL length restrictions

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

        #region Shop Landing
        [Route("products")]
        public ActionResult Products()
        {
            var model = ShoppingViewModelFactory.Create<CategoryLandingViewModel>(PropertyBag);


            return View(model);
        }

        public ActionResult Index()
        {

            return RedirectToAction("ItemList");
        }

        [Route("category")]
        public ActionResult Category()
        {
            return View();
        }
        #endregion

        #region Items
        [Route("items")]
        public ActionResult ItemList()
        {
            var model = ShoppingViewModelFactory.Create<CategoryLandingViewModel>(PropertyBag);

            // Get the available products
            model.Items = Exigo.GetItems(new ExigoService.GetItemsRequest
            {
                Configuration = OrderConfiguration,
                IncludeChildCategories = true
            }).ToList();
            model.Items.ToList().ForEach(c => c.Quantity = 1);

            model.MainCategoryDescription = "Category Description";

            return View(model);
        }

        [Route("product/{itemcode}")]
        public ActionResult ItemDetail(string itemcode)
        {

            var availableWarehouses = Exigo.OData().ItemWarehouses.Where(c => c.Item.ItemCode == itemcode).ToList().Select(c => c.WarehouseID);
            var orderConfig = OrderConfiguration;
            if (availableWarehouses != null)
            {
                if (!availableWarehouses.Contains(OrderConfiguration.WarehouseID))
                {
                    var country = GlobalSettings.Markets.AvailableMarkets.Where(c => availableWarehouses.Contains(c.Configuration.Orders.WarehouseID)).First().Countries.First();

                    GlobalUtilities.SetSelectedCountryCode(country);


                    return RedirectToAction("ItemDetail", new { itemcode = itemcode });
                    //orderConfig = GlobalUtilities.GetMarketConfigurationByCountry(country).Orders;
                }
            }
            var model = ShoppingViewModelFactory.Create<ItemDetailViewModel>(PropertyBag);

            try
            {
                model.Item = Exigo.GetItemDetail(new GetItemDetailRequest
                {
                    Configuration = orderConfig, // used to refresh configuration if necessary
                    ItemCode = itemcode
                });

                if (model.Item != null)
                {
                    model.Item.Quantity = 1;

                    if (model.Item.Quantity == 0)
                    {
                        model.Item.Quantity = 1;
                    }
                }
            }
            catch
            {
                if (GlobalUtilities.GetSelectedCountryCode() == "US")
                {

                }
                model.Item = Exigo.GetItemDetail(new GetItemDetailRequest
                {
                    Configuration = orderConfig, // used to refresh configuration if necessary
                    ItemCode = itemcode
                });

                if (model.Item != null)
                {
                    model.Item.Quantity = 1;

                    if (model.Item.Quantity == 0)
                    {
                        model.Item.Quantity = 1;
                    }
                }
            }
            if (Identity.Customer != null)
            {
                var customerAutoOrders = Exigo.OData().AutoOrders.Where(a => a.CustomerID == Identity.Customer.CustomerID && a.AutoOrderStatusID == (int)AutoOrderStatusType.Active);
                model.HasAutoOrder = customerAutoOrders.Count() > 0;
            }
            return View(model);
        }
        [Route("cart")]
        public ActionResult Cart()
        {
            var model = ShoppingViewModelFactory.Create<CartViewModel>(PropertyBag);

            model = GetCartPreviewModel();  // This is a model Mike generated on line 1542 that is used for four different cart and add to cart actions

            return View(model);
        }

        #endregion

        #region Shipping and Billing Address
        [RequireHttpsWhenLive]
        [Route("checkout/shipping")]
        public ActionResult Shipping()
        {
            if (Identity.Customer == null)
            {
                return RedirectToAction("checkout");
            }

            var model = ShoppingViewModelFactory.Create<CheckoutViewModel>(PropertyBag);

            ViewBag.CheckoutStep = "Shipping Address";

            if (Identity.Customer != null)
            {
                model.Addresses = Exigo.GetCustomerAddresses(Identity.Customer.CustomerID)
                    .Where(c => c.IsComplete)
                    .Select(c => c as ShippingAddress);

                //model.PropertyBag.BillingAddress = new Address();
            }

            if (PropertyBag.ShippingAddress == null || PropertyBag.ShippingAddress.Country != GlobalUtilities.GetSelectedCountryCode())
            {
                PropertyBag.ShippingAddress = new ShippingAddress { Country = GlobalUtilities.GetSelectedCountryCode() };
            }
            model.ShippingAddress = PropertyBag.ShippingAddress;

            return View(model);
        }

        [RequireHttpsWhenLive]
        [HttpPost]
        [Route("checkout/shipping")]
        public ActionResult Shipping(ShippingAddress address, bool overrideValidation = false)
        {
            if (address.Country == "US")
            {
                // Validate the address
                var response = new ExigoService.VerifyAddressResponse()
                {
                    IsValid = false
                };
                if (!overrideValidation)
                {
                    response = Exigo.VerifyAddress(address as Address);

                    address.Address1 = address.Address1;
                    address.Address2 = address.Address2;
                    address.City = address.City;
                    address.State = address.State;
                    address.Zip = address.Zip;
                    address.Country = address.Country;

                    PropertyBag.ShippingAddress = address;
                    Exigo.PropertyBags.Update(PropertyBag);
                }

                if (response.IsValid || overrideValidation)
                {
                    if (!overrideValidation)
                    {
                        address.Address1 = response.VerifiedAddress.Address1;
                        address.Address2 = response.VerifiedAddress.Address2;
                        address.City = response.VerifiedAddress.City;
                        address.State = response.VerifiedAddress.State;
                        address.Zip = response.VerifiedAddress.Zip;
                        address.Country = response.VerifiedAddress.Country;

                        PropertyBag.ShippingAddress = address;
                    }
                    // Save the address to the customer's account if applicable
                    if (Request.IsAuthenticated && address.AddressType == AddressType.New)
                    {
                        Exigo.SetCustomerAddressOnFile(Identity.Customer.CustomerID, address as Address);
                    }

                    // If Auto Order Items are in Cart Ensure Defaults are set

                    if (ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder).Count() > 0)
                    {
                        // Ensure we have a valid frequency type
                        if (!GlobalSettings.AutoOrders.AvailableFrequencyTypes.Contains(PropertyBag.AutoOrderFrequencyType))
                        {
                            PropertyBag.AutoOrderFrequencyType = Identity.Customer.Market.Configuration.AutoOrders.DefaultFrequencyType;
                        }

                        // Ensure we have a valid start date based on the frequency
                        if (PropertyBag.AutoOrderStartDate == DateTime.MinValue)
                        {
                            PropertyBag.AutoOrderStartDate = GlobalUtilities.GetNextAvailableAutoOrderStartDate(DateTime.Today.AddMonths(1)).ToLocalTime();
                        }

                    }

                    PropertyBag.ShippingAddress = address;
                    Exigo.PropertyBags.Update(PropertyBag);

                    return LogicProvider.GetNextAction();
                }
                else
                {
                    
                    return RedirectToAction("Shipping", new { validate = "Unable to verify address" });
                }
            }
            else
            {
                // If Auto Order Items are in Cart Ensure Defaults are set

                if (ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder).Count() > 0)
                {
                    // Ensure we have a valid frequency type
                    if (!GlobalSettings.AutoOrders.AvailableFrequencyTypes.Contains(PropertyBag.AutoOrderFrequencyType))
                    {
                        PropertyBag.AutoOrderFrequencyType = Identity.Customer.Market.Configuration.AutoOrders.DefaultFrequencyType;
                    }

                    // Ensure we have a valid start date based on the frequency
                    if (PropertyBag.AutoOrderStartDate == DateTime.MinValue)
                    {
                        PropertyBag.AutoOrderStartDate = GlobalUtilities.GetNextAvailableAutoOrderStartDate(DateTime.Today.AddMonths(1)).ToLocalTime();
                    }

                }

                PropertyBag.ShippingAddress = address;
                Exigo.PropertyBags.Update(PropertyBag);

                return LogicProvider.GetNextAction();
            }
        }
        [RequireHttpsWhenLive]
        [Route("checkout/billing")]
        public ActionResult Billing()
        {
            var model = ShoppingViewModelFactory.Create<CheckoutViewModel>(PropertyBag);

            ViewBag.CheckoutStep = "Billing Address";

            if (Identity.Customer != null)
            {
                model.Addresses = Exigo.GetCustomerAddresses(Identity.Customer.CustomerID)
                    .Where(c => c.IsComplete)
                    .Select(c => c as ShippingAddress);
            }

            //if (model.BillingAddress == null) {
            //    model.BillingAddress = PropertyBag.BillingAddress;
            //}

            // Check for Auto Order Items
            var cartItems = ShoppingCart.Items.ToList();
            model.Items = Exigo.GetItems(cartItems, OrderConfiguration).ToList();

            return View(model);
        }
        [RequireHttpsWhenLive]
        [HttpPost]
        [Route("checkout/billing")]
        public ActionResult Billing(CheckoutViewModel model, Address address)
        {
            if (model.BillingSameAsShipping)
            {
                PropertyBag.BillingName = PropertyBag.ShippingAddress.FullName;
                PropertyBag.BillingAddress = PropertyBag.ShippingAddress;

                Exigo.PropertyBags.Update(PropertyBag);

                return LogicProvider.GetNextAction();
            }
            else if (model.BillingAddress != null)
            {
                PropertyBag.BillingName = model.BillingName;
                PropertyBag.BillingAddress = model.BillingAddress;
                Exigo.PropertyBags.Update(PropertyBag);

                return LogicProvider.GetNextAction();

            }
            else
            {
                PropertyBag.BillingName = (string.IsNullOrEmpty(model.BillingName)) ? PropertyBag.ShippingAddress.FullName : model.BillingName;
                PropertyBag.BillingAddress = address;

                Exigo.PropertyBags.Update(PropertyBag);


                return LogicProvider.GetNextAction();

            }

        }
        #endregion
        
        #region Delivery Options
        [RequireHttpsWhenLive]
        [Route("checkout/delivery")]
        public ActionResult Delivery()
        {
            ViewBag.CheckoutStep = "Delivery";

            var model = ShoppingViewModelFactory.Create<CheckoutViewModel>(PropertyBag);

            // Get the cart items
            var cartItems = ShoppingCart.Items.ToList();
            model.Items = Exigo.GetItems(cartItems, OrderConfiguration).ToList();
            var hasAutoOrder = (ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder).Count() > 0) ? true : false;
            // Calculate the order if applicable
            var orderItems = cartItems.Where(c => c.Type == ShoppingCartItemType.Order).ToList();
            if (orderItems.Count > 0)
            {
                model.OrderTotals = Exigo.CalculateOrder(new OrderCalculationRequest
                {
                    Configuration = OrderConfiguration,
                    Items = orderItems,
                    Address = PropertyBag.ShippingAddress,
                    ShipMethodID = PropertyBag.ShipMethodID,
                    ReturnShipMethods = true
                }, hasAutoOrder);

                model.ShipMethods = model.OrderTotals.ShipMethods;


            }


            // Calculate the autoorder if applicable
            var autoOrderItems = cartItems.Where(c => c.Type == ShoppingCartItemType.AutoOrder).ToList();
            if (autoOrderItems.Count > 0)
            {
                model.AutoOrderTotals = Exigo.CalculateOrder(new OrderCalculationRequest
                {
                    Configuration = AutoOrderConfiguration,
                    Items = autoOrderItems,
                    Address = PropertyBag.ShippingAddress,
                    ShipMethodID = AutoOrderConfiguration.DefaultShipMethodID,
                    ReturnShipMethods = true
                }, hasAutoOrder);

                // When an order has no items, we need the auto order to only show the singular ship method that it is defaulted to this would be either 14 or 15 currently which is the auto order flat rate shipping
                if (orderItems.Count == 0)
                {
                    model.ShipMethods = model.AutoOrderTotals.ShipMethods.Where(c => c.ShipMethodID == AutoOrderConfiguration.DefaultShipMethodID);
                }

            }

            // Set the default ship method
            if (PropertyBag.ShipMethodID == 0)
            {
                PropertyBag.ShipMethodID = OrderConfiguration.DefaultShipMethodID;
                Exigo.PropertyBags.Update(PropertyBag);
            }
            if (model.ShipMethods.Any(c => c.ShipMethodID == PropertyBag.ShipMethodID))
            {
                model.ShipMethods.First(c => c.ShipMethodID == PropertyBag.ShipMethodID).Selected = true;
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
                    Items = orderItems,
                    Address = PropertyBag.ShippingAddress,
                    ShipMethodID = PropertyBag.ShipMethodID,
                    ReturnShipMethods = false
                });

                model.OrderTotals = newCalculationResult;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult SetShipMethodID(int shipMethodID)
        {
            PropertyBag.ShipMethodID = shipMethodID;
            Exigo.PropertyBags.Update(PropertyBag);

            return RedirectToAction("Review");
        }

        #endregion

        // Not being used as of 8 April 2015 per Holly - Alan C
        #region AutoOrders
        [Route("checkout/autoorder")]
        public ActionResult AutoOrder()
        {
            var model = ShoppingViewModelFactory.Create<AutoOrderSettingsViewModel>(PropertyBag);

            // Ensure we have a valid frequency type
            if (PropertyBag.AutoOrderFrequencyType != Identity.Customer.Market.Configuration.AutoOrders.DefaultFrequencyType)
            {
                PropertyBag.AutoOrderFrequencyType = Identity.Customer.Market.Configuration.AutoOrders.DefaultFrequencyType;
            }

            // Ensure we have a valid start date based on the frequency
            if (PropertyBag.AutoOrderStartDate.ToLocalTime() == DateTime.MinValue)
            {
                PropertyBag.AutoOrderStartDate = GlobalUtilities.GetNextAvailableAutoOrderStartDate(DateTime.Now.AddDays(1)).ToLocalTime();
            }

            ViewBag.CheckoutStep = "Auto Order Settings";
            // Set our model
            model.AutoOrderStartDate = PropertyBag.AutoOrderStartDate.ToLocalTime();
            model.AutoOrderFrequencyType = PropertyBag.AutoOrderFrequencyType;

            return View(model);
        }

        [HttpPost]
        [Route("checkout/autoorder")]
        public ActionResult AutoOrder(DateTime autoOrderStartDate, FrequencyType autoOrderFrequencyType)
        {
            PropertyBag.AutoOrderStartDate = autoOrderStartDate.ToLocalTime();
            PropertyBag.AutoOrderFrequencyType = autoOrderFrequencyType;
            Exigo.PropertyBags.Update(PropertyBag);

            return LogicProvider.GetNextAction();
        }
        #endregion

        // DEVELOPER NOTE: NEED TO REVIEW IF THESE ARE BEING USED OR IF THEY HAVE SINCE BEEN MOVED TO THE AUTO ORDER CONTROLLER
        #region Auto Order Modal
        public JsonNetResult GetAutoOrderModal(string orderid)
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
                Configuration = Identity.Customer.Market.Configuration.Orders,
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

        [HttpPost]
        [Route("GetAutoOrderDetails")]
        public JsonNetResult GetAutoOrderDetails(int frequency)
        {
            var isExisting = false;
            try
            {
                var model = new AutoOrderCartReviewViewModel();

                var autoorders = Exigo.GetCustomerAutoOrders(new GetCustomerAutoOrdersRequest
                {
                    CustomerID = Identity.Customer.CustomerID,
                    IncludeDetails = true,
                    IncludePaymentMethods = true
                });

                var frequencyType = (FrequencyType)frequency;
                model.AutoOrderFrequencyType = frequencyType.ToString();
                var odataFrequencyTypeID = GlobalUtilities.GetODataFrequencyTypeID(frequency);

                // If we have an existing auto order that matches the current frequency, we then load it into our model
                if (autoorders.Any(a => a.FrequencyTypeID == odataFrequencyTypeID))
                //if (autoorders.Any(a => a.AutoOrderID == autoOrderID))
                {
                    isExisting = true;
                    model.ActiveAutoOrder = autoorders.Where(a => a.FrequencyTypeID == odataFrequencyTypeID).FirstOrDefault();
                    //model.ActiveAutoOrder = autoorders.Where(a => a.AutoOrderID == autoOrderID).FirstOrDefault();
                    model.AvailableRunDays.ForEach(a =>
                    {
                        if (Convert.ToDateTime(model.ActiveAutoOrder.NextRunDate).Day.ToString() == a.Value)
                        {
                            a.Selected = true;
                        }
                    });

                    model.NextRunDate = Convert.ToDateTime(model.ActiveAutoOrder.NextRunDate);
                }
                else
                {
                    if (PropertyBag.AutoOrderStartDate == DateTime.MinValue)
                    {
                        PropertyBag.AutoOrderStartDate = Exigo.GetNewAutoOrderStartDate(Exigo.GetFrequencyType(frequency));
                        Exigo.PropertyBags.Update(PropertyBag);
                    }

                    model.AvailableRunDays.ForEach(a =>
                    {
                        if (PropertyBag.AutoOrderStartDate.Day.ToString() == a.Value)
                        {
                            model.SelectedDay = Convert.ToInt32(a.Value);
                        }
                    });

                    model.NextRunDate = PropertyBag.AutoOrderStartDate;
                }

                if (ShoppingCart.Items.Any(i => i.Type == ShoppingCartItemType.AutoOrder))
                {
                    var autoorderCartItems = ShoppingCart.Items.Where(i => i.Type == ShoppingCartItemType.AutoOrder).ToList();

                    if (isExisting)
                    {
                        var activeAutoOrderDetails = model.ActiveAutoOrder.Details.Select(d => new ShoppingCartItem
                        {
                            ItemCode = d.ItemCode,
                            Type = ShoppingCartItemType.AutoOrder,
                            Quantity = d.Quantity,

                        });

                        foreach (var item in activeAutoOrderDetails)
                        {
                            // Check to see if we are adding new items from the active auto ship or if we are just updating quantities to display to the user
                            var cartItem = autoorderCartItems.Where(i => i.ItemCode == item.ItemCode);
                            if (cartItem.Count() > 0)
                            {
                                var newQuantity = cartItem.FirstOrDefault().Quantity + item.Quantity;
                                autoorderCartItems.Where(c => c.ItemCode == item.ItemCode).First().Quantity = newQuantity;
                            }
                            else
                            {
                                autoorderCartItems.Add(item);
                            }
                        }

                    }

                    var itemcodes = autoorderCartItems.Select(i => i.ItemCode).ToArray();
                    var autoorderItems = Exigo.GetItems(new ExigoService.GetItemsRequest { Configuration = AutoOrderConfiguration, ItemCodes = itemcodes });
                    if (autoorderItems != null && autoorderItems.Count() > 0)
                    {
                        var rawItems = autoorderItems.ToList();
                        rawItems.ForEach(i =>
                        {
                            var cartItem = autoorderCartItems.Where(a => a.ItemCode == i.ItemCode).FirstOrDefault();
                            var activeQuantity = 0m;

                            // Add the existing auto order detail quantities to the appropriate items 
                            //if (isExisting)
                            //{
                            //    var activeItem = model.ActiveAutoOrder.Details.Where(a => a.ItemCode == i.ItemCode);
                            //    if (activeItem.Count() > 0)
                            //    {
                            //        activeQuantity = activeItem.FirstOrDefault().Quantity;
                            //    }
                            //}

                            i.Quantity = cartItem.Quantity + activeQuantity;
                            i.ID = cartItem.ID;
                        });
                        model.AutoOrderCartItems = rawItems;

                        // Calculate Auto Order
                        var address = (PropertyBag.ShippingAddress != null && PropertyBag.ShippingAddress.IsComplete) ? PropertyBag.ShippingAddress : GlobalSettings.Company.Address;
                        var shipMethodID = (PropertyBag.ShipMethodID > 0) ? PropertyBag.ShipMethodID : AutoOrderConfiguration.DefaultShipMethodID;

                        var orderCalcResponse = Exigo.CalculateOrder(new OrderCalculationRequest()
                        {
                            Address = address,
                            Configuration = OrderConfiguration,
                            Items = model.AutoOrderCartItems,
                            ShipMethodID = shipMethodID

                        });

                        model.CalculatedAutoOrder = orderCalcResponse;


                    }
                }
                else
                {
                    // If we are just loading an auto order, we need to get the order totals as well
                    if (isExisting)
                    {
                        var activeAutoOrderDetails = model.ActiveAutoOrder.Details.Select(d => new ShoppingCartItem
                        {
                            ItemCode = d.ItemCode,
                            Type = ShoppingCartItemType.AutoOrder,
                            Quantity = d.Quantity
                        });

                        var itemcodes = activeAutoOrderDetails.Select(i => i.ItemCode).ToArray();

                        var rawItems = Exigo.GetItems(new ExigoService.GetItemsRequest { Configuration = OrderConfiguration, ItemCodes = itemcodes }).ToList();
                        rawItems.ForEach(i =>
                        {
                            var cartItem = activeAutoOrderDetails.Where(a => a.ItemCode == i.ItemCode).FirstOrDefault();
                            i.Quantity = cartItem.Quantity;
                            i.ID = cartItem.ID;
                        });
                        model.AutoOrderCartItems = rawItems;


                        // Calculate Auto Order
                        var address = model.ActiveAutoOrder.Recipient;
                        var shipMethodID = model.ActiveAutoOrder.ShipMethodID;
                        var orderCalcResponse = Exigo.CalculateOrder(new OrderCalculationRequest
                        {
                            Address = address,
                            ShipMethodID = shipMethodID,
                            Configuration = OrderConfiguration,
                            Items = model.AutoOrderCartItems
                        });

                        model.CalculatedAutoOrder = orderCalcResponse;
                    }
                }


                // Get available point balance for display purposes
                var customerPointAccount = Exigo.GetCustomerPointAccount(Identity.Customer.CustomerID, 1);
                model.AvailablePoints = (customerPointAccount != null) ? customerPointAccount.Balance : 0;


                var html = this.RenderPartialViewToString("displaytemplates/autoordermodule_cartreview", model);

                return new JsonNetResult(new
                {
                    success = true,
                    html = html,
                    isExistingAutoOrder = isExisting
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
        [Route("~/UpdateAutoOrder")]
        public JsonNetResult UpdateAutoOrder(int frequency, List<ShoppingCartItem> items, int runDay, int autoorderID = 0)
        {
            var currentDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);
            try
            {
                // First, we determine if we are dealing with an active auto order or if we are just updating the new auto order in the current shopping cart
                if (autoorderID == 0)
                {
                    var autoorderItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder).ToList();

                    foreach (var item in autoorderItems)
                    {
                        var currentItem = items.Where(i => i.ID == item.ID).FirstOrDefault();

                        if (item.Quantity != currentItem.Quantity)
                        {
                            ShoppingCart.Items.Update(item.ID, currentItem.Quantity);
                        }
                    }

                    if (ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder).Count() == 0)
                    {
                        ShoppingCart.AutoOrderFrequencyTypeID = 0;
                    }

                    Exigo.PropertyBags.Update(ShoppingCart);

                    var runDate = PropertyBag.AutoOrderStartDate;

                    var newStartDate = new DateTime(runDate.Year, runDate.Month, runDay);
                    if (newStartDate < currentDate)
                    {
                        newStartDate = newStartDate.AddMonths(1);
                    }
                    PropertyBag.AutoOrderStartDate = newStartDate;
                    Exigo.PropertyBags.Update(PropertyBag);
                }
                else
                {
                    var existingAutoOrder = Exigo.OData().AutoOrders.Expand("Details")
                    .Where(c => c.CustomerID == Identity.Customer.CustomerID)
                    .Where(c => c.AutoOrderID == autoorderID)
                    .FirstOrDefault();

                    // Re-create the autoorder
                    var request = new CreateAutoOrderRequest(existingAutoOrder);

                    var details = new List<OrderDetailRequest>();

                    foreach (var item in items.Where(i => i.Quantity > 0))
                    {
                        details.Add(new OrderDetailRequest
                        {
                            ItemCode = item.ItemCode,
                            Quantity = item.Quantity,
                            Type = ShoppingCartItemType.AutoOrder
                        });
                    }

                    request.Details = details.ToArray();

                    var startDate = request.StartDate;
                    var newStartDate = new DateTime(startDate.Year, startDate.Month, runDay);
                    if (newStartDate < currentDate)
                    {
                        newStartDate = newStartDate.AddMonths(1);
                    }
                    request.StartDate = newStartDate;

                    var response = Exigo.WebService().CreateAutoOrder(request);

                    // Clear out the auto order cart items since we have updated our auto order at this point
                    if (response.Result.Status == ResultStatus.Success)
                    {
                        ShoppingCart.Items.Remove(ShoppingCartItemType.AutoOrder);
                        Exigo.PropertyBags.Update(ShoppingCart);
                    }

                    // May need to return different type of view for updating existing auto orders
                    // return GetAutoOrderDetails(frequency);
                }

                return GetAutoOrderDetails(frequency);
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

        [Route("CancelAutoOrderUpdate")]
        public JsonNetResult CancelAutoOrderUpdate()
        {
            ShoppingCart.Items.Remove(ShoppingCartItemType.AutoOrder);
            Exigo.PropertyBags.Update(ShoppingCart);

            return new JsonNetResult(new
            {
                success = true
            });
        }

        #endregion

        #region Payments
        [RequireHttpsWhenLive]
        [Route("checkout/payment")]
        public ActionResult Payment()
        {
            var model = ShoppingViewModelFactory.Create<PaymentMethodsViewModel>(PropertyBag);

            if (Identity.Customer != null)
            {
                model.PaymentMethods = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
                {
                    CustomerID = Identity.Customer.CustomerID,
                    ExcludeIncompleteMethods = true,
                    ExcludeInvalidMethods = true
                });
                model.Addresses = Exigo.GetCustomerAddresses(Identity.Customer.CustomerID)
                    .Where(c => c.IsComplete)
                    .Select(c => c as ShippingAddress);
            }

            return View("Payment", model);
        }
        [RequireHttpsWhenLive]
        [HttpPost]
        public ActionResult UseCreditCardOnFile(CreditCardType type)
        {
            var paymentMethod = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                ExcludeIncompleteMethods = true,
                ExcludeInvalidMethods = true
            }).Where(c => c is CreditCard && ((CreditCard)c).Type == type).FirstOrDefault();

            return UsePaymentMethod(paymentMethod);
        }
        [RequireHttpsWhenLive]
        [HttpPost]
        public ActionResult UseBankAccountOnFile(ExigoService.BankAccountType type)
        {
            var paymentMethod = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
            {
                CustomerID = Identity.Customer.CustomerID,
                ExcludeIncompleteMethods = true,
                ExcludeInvalidMethods = true
            }).Where(c => c is BankAccount && ((BankAccount)c).Type == type).FirstOrDefault();

            return UsePaymentMethod(paymentMethod);
        }
        [RequireHttpsWhenLive]
        [HttpPost]
        public ActionResult UseCreditCard(CreditCard newCard)
        {
            try
            {
                var billingAddress = PropertyBag.BillingAddress;

                newCard.BillingAddress = new Address
                {
                    Address1 = billingAddress.Address1,
                    Address2 = billingAddress.Address2,
                    City = billingAddress.City,
                    State = billingAddress.State,
                    Zip = billingAddress.Zip,
                    Country = billingAddress.Country
                };



                // Verify that the card is valid
                if (!newCard.IsValid)
                {
                    return new JsonNetResult(new
                    {
                        success = false
                    });
                }
                else
                {
                    // Save the credit card to the customer's account if applicableB
                    if (LogicProvider.IsAuthenticated())
                    {
                        var paymentMethodsOnFile = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
                        {
                            CustomerID = Identity.Customer.CustomerID,
                            ExcludeIncompleteMethods = true,
                            ExcludeInvalidMethods = true
                        }).Where(c => c is CreditCard).Select(c => c as CreditCard);

                        var firstPayment = paymentMethodsOnFile.Where(c => c.Type == CreditCardType.Primary).FirstOrDefault();
                        var secondPayment = paymentMethodsOnFile.Where(c => c.Type == CreditCardType.Secondary).FirstOrDefault();

                        if (firstPayment == null)
                        {
                            Exigo.SetCustomerCreditCard(Identity.Customer.CustomerID, newCard, CreditCardType.Primary);
                        }

                        else if (secondPayment == null && firstPayment.CardNumber != newCard.CardNumber)
                        {
                            Exigo.SetCustomerCreditCard(Identity.Customer.CustomerID, newCard, CreditCardType.Secondary);
                        }
                    }

                    PropertyBag.IsRedirectPayment = false;

                    return UsePaymentMethod(newCard);
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

        [HttpPost]
        public ActionResult UseBankAccount(BankAccount newBankAccount, bool billingSameAsShipping = false)
        {
            if (billingSameAsShipping)
            {
                var address = PropertyBag.ShippingAddress;

                newBankAccount.BillingAddress = new Address
                {
                    Address1 = address.Address1,
                    Address2 = address.Address2,
                    City = address.City,
                    State = address.State,
                    Zip = address.Zip,
                    Country = address.Country
                };
            }


            // Verify that the card is valid
            if (!newBankAccount.IsValid)
            {
                return new JsonNetResult(new
                {
                    success = false
                });
            }
            else
            {
                // Save the bank account to the customer's account if applicable
                if (LogicProvider.IsAuthenticated())
                {
                    var paymentMethodsOnFile = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
                    {
                        CustomerID = Identity.Customer.CustomerID,
                        ExcludeIncompleteMethods = true,
                        ExcludeInvalidMethods = true,
                        ExcludeNonAutoOrderPaymentMethods = true
                    }).Where(c => c is BankAccount).Select(c => c as BankAccount);

                    if (paymentMethodsOnFile.FirstOrDefault() == null)
                    {
                        Exigo.SetCustomerBankAccount(Identity.Customer.CustomerID, newBankAccount);
                    }
                }


                return UsePaymentMethod(newBankAccount);
            }
        }

        [HttpPost]
        public ActionResult UsePaymentMethod(IPaymentMethod paymentMethod, int PaymentTypeID = 0)
        {
            try
            {
                // Check for redirect payment type : 999 = Ideal
                if (PaymentTypeID == 999)
                {
                    PropertyBag.PaymentMethod = new Ideal();
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

        #region Review/Checkout
        [RequireHttpsWhenLive]
        [Route("checkout/review")]
        public ActionResult Review()
        {
            var logicResult = LogicProvider.CheckLogic();
            if (!logicResult.IsValid) return logicResult.NextAction;
            var hasAutoOrder = (ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder).Count() > 0) ? true : false;

            ViewBag.CheckoutStep = "Payment and Review";

            var model = ShoppingViewModelFactory.Create<CheckoutViewModel>(PropertyBag);
            var customerID = Identity.Customer.CustomerID;

            // Get the cart items
            var cartItems = ShoppingCart.Items.ToList();
            model.Items = Exigo.GetItems(cartItems, OrderConfiguration).ToList();


            // Calculate the order if applicable
            var orderItems = cartItems.Where(c => c.Type == ShoppingCartItemType.Order).ToList();
            if (orderItems.Count > 0)
            {
                model.OrderTotals = Exigo.CalculateOrder(new OrderCalculationRequest
                {
                    Configuration = OrderConfiguration,
                    Items = orderItems,
                    Address = PropertyBag.ShippingAddress,
                    ShipMethodID = PropertyBag.ShipMethodID,
                    ReturnShipMethods = true
                }, hasAutoOrder);

                model.ShipMethods = model.OrderTotals.ShipMethods;

            }


            // Calculate the autoorder if applicable
            var autoOrderItems = cartItems.Where(c => c.Type == ShoppingCartItemType.AutoOrder).ToList();
            if (autoOrderItems.Count > 0)
            {
                model.AutoOrderTotals = Exigo.CalculateOrder(new OrderCalculationRequest
                {
                    Configuration = AutoOrderConfiguration,
                    Items = autoOrderItems,
                    Address = PropertyBag.ShippingAddress,
                    ShipMethodID = AutoOrderConfiguration.DefaultShipMethodID,
                    ReturnShipMethods = true
                }, hasAutoOrder);

                model.ShipMethods = model.AutoOrderTotals.ShipMethods;

            }


            // Set the default ship method
            if (PropertyBag.ShipMethodID == 0)
            {
                PropertyBag.ShipMethodID = OrderConfiguration.DefaultShipMethodID;
                Exigo.PropertyBags.Update(PropertyBag);
            }
            if (model.ShipMethods.Any(c => c.ShipMethodID == PropertyBag.ShipMethodID))
            {
                model.ShipMethods.First(c => c.ShipMethodID == PropertyBag.ShipMethodID).Selected = true;
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
                    Items = orderItems,
                    Address = PropertyBag.ShippingAddress,
                    ShipMethodID = PropertyBag.ShipMethodID,
                    ReturnShipMethods = false
                }, hasAutoOrder);

                model.OrderTotals = newCalculationResult;
            }

            var paymentMethods = Exigo.GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
            {
                CustomerID = customerID,
                ExcludeIncompleteMethods = true,
                ExcludeInvalidMethods = true
            });

            var addresses = Exigo.GetCustomerAddresses(customerID)
               .Where(c => c.IsComplete)
               .Select(c => c as ShippingAddress);

            var paymentMethodViewModel = new PaymentMethodsViewModel
            {
                PaymentMethods = paymentMethods,
                PropertyBag = PropertyBag,
                Addresses = addresses
            };

            model.PaymentMethodModel = paymentMethodViewModel;

            // Smart Shopper Logic, if not previously selected
            if (autoOrderItems.Count > 0 && Identity.Customer.CustomerTypeID == CustomerTypes.RetailCustomer)
            {
                if (!PropertyBag.GetSmartShopperPrice)
                {
                    model.PropertyBag.GetSmartShopperPrice = true;
                    PropertyBag.GetSmartShopperPrice = true;
                    Exigo.PropertyBags.Update(PropertyBag);
                }
            }

            return View(model);
        }

        [RequireHttpsWhenLive]
        [HttpPost]
        public ActionResult SubmitCheckout()
        {
            // Do one final check to ensure that our logic points are met before attempting to submit
            var logicResult = LogicProvider.CheckLogic();
            if (!logicResult.IsValid)
            {
                return logicResult.NextAction;
            }

            // Set up our guest customer & testing variables
            var isGuestCheckout = Identity.Customer == null;
            var isLocal = Request.IsLocal;

            var isRedirectPayment = PropertyBag.IsRedirectPayment;

            try
            {
                // Start creating the API requests
                var details = new List<ApiRequest>();


                // Update our Retail Customer to Smart Shopper, if applicable
                if (PropertyBag.GetSmartShopperPrice)
                {
                    var address = PropertyBag.ShippingAddress;

                    var updateCustomerRequest = new UpdateCustomerRequest
                    {
                        CustomerID = Identity.Customer.CustomerID,
                        MainAddress1 = address.Address1,
                        MainAddress2 = address.Address2,
                        MainCity = address.City,
                        MainState = address.State,
                        MainZip = address.Zip,
                        MainCountry = address.Country,
                        CustomerType = CustomerTypes.SmartShopper
                    };

                    details.Add(updateCustomerRequest);

                    // Create the Replicated Site for our Smart Shopper
                    var newSite = new SetCustomerSiteRequest
                    {
                        CustomerID = Identity.Customer.CustomerID,
                        WebAlias = Identity.Customer.CustomerID.ToString(),
                        FirstName = Identity.Customer.FirstName,
                        LastName = Identity.Customer.LastName,
                        Address1 = address.Address1,
                        Address2 = address.Address2,
                        City = address.City,
                        State = address.State,
                        Zip = address.Zip,
                        Country = address.Country,
                        Email = address.Email,
                        Company = Identity.Customer.PublicName,
                        Phone = address.Phone
                    };

                    details.Add(newSite);
                }


                var orderItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order);
                var hasOrder = orderItems.Count() > 0;

                var autoOrderItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder);
                var hasAutoOrder = autoOrderItems.Count() > 0;

                // Create the order request, if applicable
                if (hasOrder)
                {
                    var orderRequest = new CreateOrderRequest(OrderConfiguration, PropertyBag.ShipMethodID, orderItems, PropertyBag.ShippingAddress);


                    if (!isGuestCheckout)
                    {
                        orderRequest.CustomerID = Identity.Customer.CustomerID;
                    }

                    if (isRedirectPayment)
                    {
                        orderRequest.OrderStatus = OrderStatusType.Pending;
                    }

                    if (Identity.Customer.CustomerTypeID == CustomerTypes.SmartShopper)
                    {
                        orderRequest.PriceType = PriceTypes.Wholesale;
                    }

                    if (hasAutoOrder)
                    {
                        orderRequest.PriceType = PriceTypes.Autoship;
                    }

                    details.Add(orderRequest);
                }


                // Create the autoorder request, if applicable
                if (hasAutoOrder)
                {
                    var autoOrderRequest = new CreateAutoOrderRequest(AutoOrderConfiguration, Exigo.GetAutoOrderPaymentType(PropertyBag.PaymentMethod), PropertyBag.AutoOrderStartDate, AutoOrderConfiguration.DefaultShipMethodID, autoOrderItems, PropertyBag.ShippingAddress);

                    autoOrderRequest.Frequency = FrequencyType.Monthly;

                    if (!isGuestCheckout)
                    {
                        autoOrderRequest.CustomerID = Identity.Customer.CustomerID;
                    }

                    details.Add(autoOrderRequest);
                }


                // Create the payment request
                if (PropertyBag.PaymentMethod is CreditCard)
                {
                    var card = PropertyBag.PaymentMethod as CreditCard;
                    if (card.Type == CreditCardType.New)
                    {
                        card = Exigo.SaveNewCustomerCreditCard(Identity.Customer.CustomerID, card);

                        if (hasAutoOrder)
                        {
                            if (!isGuestCheckout)
                            {
                                card = Exigo.SaveNewCustomerCreditCard(Identity.Customer.CustomerID, card);
                                ((CreateAutoOrderRequest)details.Where(c => c is CreateAutoOrderRequest).FirstOrDefault()).PaymentType = Exigo.GetAutoOrderPaymentType(card);
                            }
                            else
                            {
                                // Add logic if guest checkout is allowed : constructor for SetAccountCreditCardTokenRequest

                            }
                        }
                        if (hasOrder)
                        {
                            if (card.IsTestCreditCard)
                            {
                                // no need to charge card
                                ((CreateOrderRequest)details.Where(c => c is CreateOrderRequest).FirstOrDefault()).OrderStatus = OrderStatusType.Shipped;
                            }
                            else
                            {
                                if (!isLocal)
                                {
                                    details.Add(new ChargeCreditCardTokenRequest(card));
                                }
                            }
                        }
                    }
                    else
                    {
                        if (hasOrder)
                        {
                            if (card.IsTestCreditCard)
                            {
                                // no need to charge card
                            }
                            else
                            {
                                if (!isLocal)
                                {
                                    var cctype = (card.Type == CreditCardType.Primary) ? AccountCreditCardType.Primary : AccountCreditCardType.Secondary;
                                    details.Add(new ChargeCreditCardOnFileRequest { CreditCardAccountType = cctype });
                                }
                            }
                        }
                    }
                }



                // Process the transaction
                var transactionRequest = new TransactionalRequest();
                transactionRequest.TransactionRequests = details.ToArray();
                var transactionResponse = Exigo.WebService().ProcessTransaction(transactionRequest);


                var newOrderID = 1;
                var customerID = 0;
                if (transactionResponse.Result.Status == ResultStatus.Success)
                {
                    foreach (var response in transactionResponse.TransactionResponses)
                    {
                        if (response is CreateOrderResponse) newOrderID = ((CreateOrderResponse)response).OrderID;
                        if (response is CreateCustomerResponse) customerID = ((CreateCustomerResponse)response).CustomerID;
                    }
                }

                if (PropertyBag.GetSmartShopperPrice)
                {

                    Identity.Customer.Refresh();
                }

                customerID = Identity.Customer.CustomerID;
                var token = Security.Encrypt(new { OrderID = newOrderID, CustomerID = Identity.Customer.CustomerID });

                // handle redirect payments
                var selectedCountry = PropertyBag.ShippingAddress.Country;

                if (isRedirectPayment && hasOrder)
                {
                    var paymentProvider = PaymentService.GetPaymentProvider(selectedCountry);
                    var order = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest()
                    {
                        CustomerID = customerID,
                        OrderID = newOrderID,
                        IncludeOrderDetails = true
                    }).FirstOrDefault();

                    if (paymentProvider.HandlerType == PaymentHandlerType.Remote)
                    {
                        //Exigo.PropertyBags.Delete(PropertyBag);
                        Exigo.PropertyBags.Delete(ShoppingCart);

                        paymentProvider.OrderConfiguration = OrderConfiguration;
                        paymentProvider.Order = order;
                        paymentProvider.Order.ShipMethodID = PropertyBag.ShipMethodID;

                       
                    }

                    // Get the request data
                    var paymentRequest = paymentProvider.GetPaymentRequest(new PaymentRequestArgs() { ReturnUrl = PaymentRedirectURL, BillingName = order.Recipient.FullName, BillingAddress = PropertyBag.BillingAddress, WebAlias = Identity.Owner.WebAlias });

                    // Handle the request
                    var postPaymentRequest = paymentRequest as POSTPaymentRequest;
                    if (postPaymentRequest != null)
                    {
                        
                        Exigo.PropertyBags.Delete(PropertyBag);

                        return new JsonNetResult(new
                        {
                            success = true,
                            redirectForm = postPaymentRequest.RequestForm
                        });
                    }
                    else
                    {
                        //var urlHelper = new UrlHelper(Request.RequestContext);
                        //var completeUrl = urlHelper.Action("OrderComplete", new { token = token });

                        return new JsonNetResult(new
                        {
                            success = false,
                        });
                    }
                }



                return new JsonNetResult(new
                {
                    success = true,
                    token = token
                });

            }
            catch (Exception exception)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = exception.Message
                });
            }
        }
        //public ActionResult IdealAutoOrderPayment(int OrderID)
        //{
        //    var selectedCountry = GlobalUtilities.GetSelectedCountryCode();
        //    var paymentProvider = PaymentService.GetPaymentProvider(selectedCountry);
        //    var order = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest()
        //    {
        //        CustomerID = Identity.Customer.CustomerID,
        //        OrderID = OrderID,
        //        IncludeOrderDetails = true
        //    }).FirstOrDefault();


        //    if (paymentProvider.HandlerType == PaymentHandlerType.Remote)
        //    {
        //        paymentProvider.OrderConfiguration = OrderConfiguration;
        //        paymentProvider.Order = order;

        //        var billingAddress = new Address()
        //        {
        //            AddressType = AddressType.Other,
        //            Address1 = order.Recipient.Address1,
        //            Address2 = order.Recipient.Address2,
        //            City = order.Recipient.City,
        //            State = order.Recipient.State,
        //            Zip = order.Recipient.Zip,
        //            Country = order.Recipient.Country
        //        }; 


        //        // Get the request data
        //        var paymentRequest = paymentProvider.GetPaymentRequest(new PaymentRequestArgs() { ReturnUrl = PaymentRedirectURL, BillingName = order.Recipient.FullName, BillingAddress = billingAddress, WebAlias = Identity.Owner.WebAlias });
              

        //        // Handle the request
        //        var postPaymentRequest = paymentRequest as POSTPaymentRequest;
        //        if (postPaymentRequest != null)
        //        {
        //            return new JsonNetResult(new
        //            {
        //                success = true,
        //                redirectForm = postPaymentRequest.RequestForm
        //            });
        //        }
        //        else
        //        {
        //            return new JsonNetResult(new
        //            {
        //                success = false
        //            });
        //        }
        //    }
        //    else
        //    {
        //        return new JsonNetResult(new
        //        {
        //            success = false
        //        });
        //    }
        //}
        [RequireHttpsWhenLive]
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


        }
        [RequireHttpsWhenLive]
        [Route("thank-you/{token}")]
        public ActionResult OrderComplete(string token)
        {
            Exigo.PropertyBags.Delete(PropertyBag);
            Exigo.PropertyBags.Delete(ShoppingCart);

            var rawToken = Security.Decrypt(token);
            var newOrderID = rawToken.OrderID;
            var customerID = rawToken.CustomerID;

            var model = new OrderCompleteViewModel();

            model.OrderID = newOrderID;

            return View();
        }

        [RequireHttpsWhenLive]
        [Route("error/{token}")]
        public ActionResult OrderCompleteError(string token)
        {
            Exigo.PropertyBags.Delete(PropertyBag);
            Exigo.PropertyBags.Delete(ShoppingCart);

            var rawToken = Security.Decrypt(token);
            var newOrderID = rawToken.OrderID;
            var customerID = rawToken.CustomerID;

            var model = new OrderCompleteViewModel();

            return View();
        }

        [RequireHttpsWhenLive]
        [Route("checkout")]
        public ActionResult Checkout()
        {
            return LogicProvider.GetNextAction();
        }
        #endregion

        #region Ajax
        // Cart preview found in header of site. Called via pubsub : window.trigger("update.cartpreview");
        public ActionResult FetchCartPreview()
        {
            try
            {
                var ordertotals = 0m;
                var autoordertotals = 0m;

                var model = GetCartPreviewModel(); // This is a model Mike generated on line 1541 that is used for four different cart and add to cart actions
                var cartHtml = this.RenderPartialViewToString("_cartpopup", model);

                if (model.Items != null && model.Items.Count() > 0)
                {
                    ordertotals = model.Items.Sum(i => i.Price * i.Quantity);
                }

                if (model.AutoOrderItems != null && model.AutoOrderItems.Count() > 0)
                {
                    autoordertotals = model.AutoOrderItems.Sum(i => i.Price * i.Quantity);
                }

                return new JsonNetResult(new
                {
                    success = true,
                    //total = (ordertotals + autoordertotals).ToString("C"),
                    total = ordertotals.ToString("C"),
                    cart = cartHtml
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

        public CartViewModel GetCartPreviewModel()
        {
            var model = ShoppingViewModelFactory.Create<CartViewModel>(PropertyBag);

            var autoordertotals = 0m;
            var orderTotals = 0m;

            // Cart items
            var orderItems = ShoppingCart.Items.Where(i => i.Type == ShoppingCartItemType.Order);
            var autoOrderItems = ShoppingCart.Items.Where(i => i.Type == ShoppingCartItemType.AutoOrder);


            if (orderItems.Count() > 0)
            {
                var configuration = OrderConfiguration;

                var priceType = PriceTypes.Retail;

                if (Identity.Customer != null && Identity.Customer.CustomerTypeID == CustomerTypes.SmartShopper)
                {
                    priceType = PriceTypes.Wholesale;
                }
                else if (Identity.Customer == null || Identity.Customer.CustomerTypeID == CustomerTypes.RetailCustomer)
                {
                    priceType = PriceTypes.Retail;
                }

                // If we have an auto order item 
                if (autoOrderItems.Count() > 0)
                {
                    priceType = PriceTypes.Autoship;
                }

                OrderConfiguration.PriceTypeID = priceType;

                var items = Exigo.GetItems(orderItems, OrderConfiguration).ToList();

                model.Items = items;

                orderTotals = items.Sum(i => i.Quantity * i.Price);
            }


            if (autoOrderItems.Count() > 0)
            {
                var configuration = AutoOrderConfiguration;

                var items = Exigo.GetItems(autoOrderItems, AutoOrderConfiguration).ToList();

                model.AutoOrderItems = items;

                autoordertotals = items.Sum(i => i.Quantity * i.Price);
            }

            return model;
        }

        public ActionResult QuickShopModal(string itemcode)
        {
            try
            {
                var model = ShoppingViewModelFactory.Create<ItemDetailViewModel>(PropertyBag);


                model.Item = Exigo.GetItemDetail(new GetItemDetailRequest
                {
                    Configuration = OrderConfiguration, // used to refresh configuration if necessary
                    ItemCode = itemcode
                });

                if (model.Item != null)
                {
                    model.Item.Quantity = 1;
                }

                if (Identity.Customer != null)
                {
                    var customerAutoOrders = Exigo.OData().AutoOrders.Where(a => a.CustomerID == Identity.Customer.CustomerID && a.AutoOrderStatusID == (int)AutoOrderStatusType.Active);
                    model.HasAutoOrder = customerAutoOrders.Count() > 0;
                }
                var html = this.RenderPartialViewToString("partials/items/details/singleitem", model);

                return new JsonNetResult(new
                {
                    success = true,
                    html = html,
                    description = model.Item.ItemDescription
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

        // Used in checkout pages, cart display on each step of checkout
        public JsonNetResult GetOrderSummary()
        {
            var model = ShoppingViewModelFactory.Create<CartViewModel>(PropertyBag);

            try
            {
                var cartItems = ShoppingCart.Items.ToList();
                var orderItems = cartItems.Where(i => i.Type == ShoppingCartItemType.Order);
                var autoOrderItems = cartItems.Where(i => i.Type == ShoppingCartItemType.AutoOrder);

                if (orderItems.Count() > 0)
                {
                    var address = (PropertyBag.ShippingAddress != null && PropertyBag.ShippingAddress.IsComplete) ? PropertyBag.ShippingAddress : null;

                    var priceType = PriceTypes.Retail;

                    if (Identity.Customer != null && Identity.Customer.CustomerTypeID == CustomerTypes.SmartShopper)
                    {
                        priceType = PriceTypes.Wholesale;
                    }
                    else if (Identity.Customer == null || Identity.Customer.CustomerTypeID == CustomerTypes.RetailCustomer)
                    {
                        priceType = PriceTypes.Retail;
                    }

                    // If we have an auto order item 
                    if (autoOrderItems.Count() > 0)
                    {
                        priceType = PriceTypes.Autoship;
                    }

                    OrderConfiguration.PriceTypeID = priceType;


                    // Calculate the order if applicable
                    if (address != null)
                    {
                        model.OrderTotals = Exigo.CalculateOrder(new OrderCalculationRequest
                        {
                            Configuration = OrderConfiguration,
                            Items = orderItems,
                            Address = PropertyBag.ShippingAddress,
                            ShipMethodID = PropertyBag.ShipMethodID,
                            ReturnShipMethods = false
                        });
                    }

                    model.Items = Exigo.GetItems(orderItems, OrderConfiguration);
                }


                if (autoOrderItems.Count() > 0)
                {
                    var address = (PropertyBag.AutoOrderShippingAddress != null && PropertyBag.AutoOrderShippingAddress.IsComplete) ? PropertyBag.AutoOrderShippingAddress : null;

                    // Calculate the order if applicable
                    if (address != null)
                    {
                        model.AutoOrderTotals = Exigo.CalculateOrder(new OrderCalculationRequest
                        {
                            Configuration = AutoOrderConfiguration,
                            Items = autoOrderItems,
                            Address = PropertyBag.AutoOrderShippingAddress,
                            ShipMethodID = AutoOrderConfiguration.DefaultShipMethodID,
                            ReturnShipMethods = false
                        });
                    }

                    model.AutoOrderItems = Exigo.GetItems(autoOrderItems, AutoOrderConfiguration);
                }


                var html = this.RenderPartialViewToString("Partials/Cart/_OrderSummary", model);

                return new JsonNetResult(new
                {
                    success = true,
                    html = html
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
        public ActionResult AddItemToCart(Item item)
        {
            ShoppingCart.Items.Add(item);
            Exigo.PropertyBags.Update(ShoppingCart);


            if (item.Type == ShoppingCartItemType.AutoOrder)
            {
                // Logic to set the property bag to account for the Smart Shopper integration (preferred customer type)
                if (Identity.Customer == null || Identity.Customer.CustomerTypeID == CustomerTypes.RetailCustomer)
                {
                    if (PropertyBag.GetSmartShopperPrice == false)
                    {
                        PropertyBag.GetSmartShopperPrice = true;
                        Exigo.PropertyBags.Update(PropertyBag);
                    }
                }

                // If Enrolling in a new auto-ship
                var webRecurringItem = new Item();

                webRecurringItem.ItemCode = item.ItemCode;
                webRecurringItem.Quantity = item.Quantity;
                webRecurringItem.PriceTypeID = PriceTypes.Retail;
                webRecurringItem.Type = ShoppingCartItemType.Order;

                // Check if Customer is logged in and is not a Smart Shopper already
                if (Identity.Customer != null)
                {

                    // Check for Existing Auto-Ships
                    var existingAutoOrders = Exigo.WebService().GetAutoOrders(new GetAutoOrdersRequest() { CustomerID = Identity.Customer.CustomerID }).AutoOrders.ToList();

                    // if no auto-ships on file, add to today's order along with enrolling in autoship
                    if (existingAutoOrders.Count() == 0)
                    {

                        ShoppingCart.Items.Add(webRecurringItem);
                        Exigo.PropertyBags.Update(ShoppingCart);
                    }
                }
                else
                {
                    // Is a new customer and can add to today's order and enroll in autoship in order to Join as a Smart Shopper
                    ShoppingCart.Items.Add(webRecurringItem);
                    Exigo.PropertyBags.Update(ShoppingCart);
                }

            }

            var cartPreviewModel = GetCartPreviewModel();  // This is a model Mike generated on line 1542 that is used for four different cart and add to cart actions
            var orderTotal = (cartPreviewModel.Items != null && cartPreviewModel.Items.Count() > 0) ? cartPreviewModel.Items.Sum(i => i.Quantity * i.Price) : 0m;
            var autoOrderTotal = (cartPreviewModel.AutoOrderItems != null && cartPreviewModel.AutoOrderItems.Count() > 0) ? cartPreviewModel.AutoOrderItems.Sum(i => i.Quantity * i.Price) : 0m;

            var cartHtml = this.RenderPartialViewToString("_cartpopup", cartPreviewModel);

            // Return the result
            if (Request.IsAjaxRequest())
            {
                return new JsonNetResult(new
                {
                    success = true,
                    //total = (orderTotal + autoOrderTotal).ToString("C"), Changed per ticket 65931 JWJ 28May2015
                    total = orderTotal.ToString("C"),
                    cart = cartHtml
                });
            }
            else
            {
                return RedirectToAction("Cart");
            }
        }

        public ActionResult RemoveItemFromCart(Guid id)
        {
            var item = ShoppingCart.Items.Where(c => c.ID == id).FirstOrDefault();

            ShoppingCart.Items.Remove(id);
            Exigo.PropertyBags.Update(ShoppingCart);

            if (!ShoppingCart.Items.Any(i => i.Type == ShoppingCartItemType.AutoOrder))
            {
                PropertyBag.GetSmartShopperPrice = false;
                PropertyBag.ShipMethodID = OrderConfiguration.DefaultShipMethodID;
                Exigo.PropertyBags.Update(PropertyBag);
            }


            // Conditionally calculate the totals - we don't need to calculate them every time
            OrderCalculationResponse totals = null;
            switch (item.Type)
            {
                case ShoppingCartItemType.Order:
                    totals = Exigo.CalculateOrder(new OrderCalculationRequest
                    {
                        Configuration = OrderConfiguration, // used to refresh configuration if necessary
                        Items = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order),
                        Address = PropertyBag.ShippingAddress,
                        ShipMethodID = PropertyBag.ShipMethodID
                    });
                    break;

                case ShoppingCartItemType.AutoOrder:
                    totals = Exigo.CalculateOrder(new OrderCalculationRequest
                    {
                        Configuration = AutoOrderConfiguration,
                        Items = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder),
                        Address = PropertyBag.ShippingAddress,
                        ShipMethodID = AutoOrderConfiguration.DefaultShipMethodID,
                    });
                    break;
            }


            if (Request.IsAjaxRequest())
            {
                return new JsonNetResult(new
                {
                    success = true,
                    item = item,
                    items = ShoppingCart.Items,
                    totals = totals
                });
            }
            else
            {
                return RedirectToAction("Cart");
            }
        }

        [HttpPost]
        [Route("UpdateCartItems")]
        public JsonNetResult UpdateCartItems(List<ShoppingCartItem> items)
        {
            try
            {
                foreach (var item in items)
                {
                    if (item.Quantity > 0)
                    {
                        ShoppingCart.Items.Update(item);
                    }
                    else
                    {
                        ShoppingCart.Items.Remove(item.ID);
                    }
                }

                Exigo.PropertyBags.Update(ShoppingCart);

                // Here we account for the Smart Shopper logic, if there aren't any auto order items, we need to set the property bag appropriately
                if (!ShoppingCart.Items.Any(i => i.Type == ShoppingCartItemType.AutoOrder))
                {
                    PropertyBag.GetSmartShopperPrice = false;
                    Exigo.PropertyBags.Update(PropertyBag);
                }

                var model = GetCartPreviewModel();  // This is a model Mike generated on line 1542 that is used for four different cart and add to cart actions

                var subtotal = 0m;
                var autoOrdersubtotal = 0m;

                if (model.Items != null && model.Items.Count() > 0)
                {
                    subtotal = model.Items.Sum(i => i.Quantity * i.Price);
                }

                if (model.AutoOrderItems != null && model.AutoOrderItems.Count() > 0)
                {
                    autoOrdersubtotal = model.AutoOrderItems.Sum(i => i.Quantity * i.Price);
                }

                return new JsonNetResult(new
                {
                    success = true,
                    subtotal = subtotal,
                    autoOrderSubtotal = autoOrdersubtotal
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

        public JsonNetResult ResetMarket(string country)
        {
            if (PropertyBag.ShippingAddress != null)
            {
                if (PropertyBag.ShippingAddress.Country != country)
                {
                    Exigo.PropertyBags.Delete(PropertyBag);
                    Exigo.PropertyBags.Delete(ShoppingCart);
                }
            }
            else
            {
                Exigo.PropertyBags.Delete(PropertyBag);
                Exigo.PropertyBags.Delete(ShoppingCart);
            }

            return new JsonNetResult(new
            {
                success = true
            });
        }
        #endregion
    }
}
