using Common;
using Common.Api.ExigoOData;
using Common.Api.ExigoWebService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static IEnumerable<Order> GetCustomerOrders(GetCustomerOrdersRequest request)
        {
            if (request.CustomerID == 0)
            {
                throw new ArgumentException("CustomerID is required.");
            }

            var context = Exigo.CreateODataContext<ExigoContext>(GlobalSettings.Exigo.Api.SandboxID);

            // Clear the OrderDetailModels
            if (OrderDetailModels != null) OrderDetailModels = new List<ExigoService.OrderDetail>();
            var orders = new List<Order>();

            // Setup the base orders query
            var ordersBaseQuery = context.Orders;
            if (request.IncludePayments) ordersBaseQuery = ordersBaseQuery.Expand("Payments");

            var ordersQuery = ordersBaseQuery.Where(c => c.CustomerID == request.CustomerID);


            // Apply the request variables
            if (request.OrderID != null)
            {
                ordersQuery = ordersQuery.Where(c => c.OrderID == ((int)request.OrderID));
            }
            if (request.OrderStatuses.Length > 0)
            {
                ordersQuery = ordersQuery.Where(request.OrderStatuses.ToList().ToOrExpression<Common.Api.ExigoOData.Order, int>("OrderStatusID"));
            }
            if (request.OrderTypes.Length > 0)
            {
                ordersQuery = ordersQuery.Where(request.OrderTypes.ToList().ToOrExpression<Common.Api.ExigoOData.Order, int>("OrderTypeID"));
            }
            if (request.StartDate != null)
            {
                ordersQuery = ordersQuery.Where(c => c.OrderDate >= (DateTime)request.StartDate);
            }


            // Get the orders
            var odataOrders = ordersQuery
                .OrderByDescending(c => c.OrderDate)
                .Skip(request.Skip)
                .Take(request.Take)
                .Select(c => c)
                .ToList();


            // If we don't have any orders, stop here.
            if (odataOrders.Count == 0) yield break;


            // Collect our orders together
            foreach (var order in odataOrders)
            {
                var model = (Order)order;
                orders.Add(model);
            }


            // Get the order details if applicable
            if (request.IncludeOrderDetails)
            {
                // Get the order IDs
                var orderIDs = orders.Select(c => c.OrderID).Distinct().ToList();


                // Get the order details (Results are saved via the ReadingEntity delegate to the private OrderDetailModels property.
                context.ReadingEntity += context_ReadingEntity;
                context.OrderDetails
                    .Where(orderIDs.ToOrExpression<Common.Api.ExigoOData.OrderDetail, int>("OrderID"))
                    .ToList();


                // Get a unique list of item IDs in the orders
                var itemIDs = OrderDetailModels.Select(c => c.ItemID).Distinct().ToList();


                // Get the extra data we need for each detail
                var apiItems = new List<Common.Api.ExigoOData.Item>();
                if (itemIDs.Count > 0)
                {
                    apiItems = context.Items
                        .Where(itemIDs.ToOrExpression<Common.Api.ExigoOData.Item, int>("ItemID"))
                        .Select(c => new Common.Api.ExigoOData.Item
                        {
                            ItemCode = c.ItemCode,
                            SmallImageUrl = c.SmallImageUrl,
                            IsVirtual = c.IsVirtual
                        })
                        .ToList();
                }


                // Format the data to our models
                foreach (var order in orders)
                {
                    // Get the order details
                    var details = OrderDetailModels.Where(c => c.OrderID == order.OrderID);
                    foreach (var detail in details)
                    {
                        var apiItem = apiItems.Where(c => c.ItemCode == detail.ItemCode).FirstOrDefault();
                        if (apiItem != null)
                        {
                            detail.ImageUrl = apiItem.SmallImageUrl;
                            detail.IsVirtual = apiItem.IsVirtual;
                        }
                    }
                    order.Details = details;
                }
            }


            // Format the data to our models
            foreach (var order in orders)
            {
                yield return order;
            }
        }
        private static List<ExigoService.OrderDetail> OrderDetailModels { get; set; }
        private static void context_ReadingEntity(object sender, System.Data.Services.Client.ReadingWritingEntityEventArgs e)
        {
            if (OrderDetailModels == null) OrderDetailModels = new List<ExigoService.OrderDetail>();

            var orderDetailModel = ((ExigoService.OrderDetail)((Common.Api.ExigoOData.OrderDetail)e.Entity));

            OrderDetailModels.Add(orderDetailModel);
        }

        public static void CancelOrder(int orderID)
        {
            Exigo.WebService().ChangeOrderStatus(new ChangeOrderStatusRequest
            {
                OrderID = orderID,
                OrderStatus = OrderStatusType.Canceled
            });
        }

        public static OrderCalculationResponse CalculateOrder(OrderCalculationRequest request, bool hasAutoOrder = false)
        {
            var result = new OrderCalculationResponse();
            if (request.Items.Count() == 0) return result;
            if (request.Address == null) request.Address = GlobalSettings.Company.Address;
            if (request.ShipMethodID == 0) request.ShipMethodID = request.Configuration.DefaultShipMethodID;


            var apirequest = new CalculateOrderRequest();

            apirequest.WarehouseID       = request.Configuration.WarehouseID;
            apirequest.CurrencyCode      = request.Configuration.CurrencyCode;
            apirequest.PriceType         = request.Configuration.PriceTypeID;
            apirequest.ShipMethodID      = request.ShipMethodID;
            apirequest.ReturnShipMethods = request.ReturnShipMethods;
            apirequest.City              = request.Address.City;
            apirequest.State             = request.Address.State;
            apirequest.Zip               = request.Address.Zip;
            apirequest.Country           = request.Address.Country;
            apirequest.Details           = request.Items.Select(c => new OrderDetailRequest(c)).ToArray();
            if(hasAutoOrder){
                
                apirequest.OrderType = Common.Api.ExigoWebService.OrderType.AutoOrder;
            
            }
            var apiresponse = Exigo.WebService().CalculateOrder(apirequest);

            result.Subtotal = apiresponse.SubTotal;
            result.Shipping = apiresponse.ShippingTotal;
            result.Tax      = apiresponse.TaxTotal;
            result.Discount = apiresponse.DiscountTotal;
            result.Total    = apiresponse.Total;


            // Assemble the ship methods
            var shipMethods = new List<ShipMethod>();
            if (apiresponse.ShipMethods != null && apiresponse.ShipMethods.Length > 0)
            {
                foreach (var shipMethod in apiresponse.ShipMethods)
                {
                    shipMethods.Add((ShipMethod)shipMethod);
                }

                // Ensure that at least one ship method is selected
                var shipMethodID = (request.ShipMethodID != 0) ? request.ShipMethodID : request.Configuration.DefaultShipMethodID;
                if (shipMethods.Any(c => c.ShipMethodID == (int)shipMethodID))
                {
                    shipMethods.First(c => c.ShipMethodID == shipMethodID).Selected = true;
                }
                else
                {
                    shipMethods.First().Selected = true;
                }
            }
            result.ShipMethods = shipMethods.AsEnumerable();

            return result;
        }
    }
}