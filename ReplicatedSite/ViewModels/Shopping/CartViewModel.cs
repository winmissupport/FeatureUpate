using ReplicatedSite.Models;
using ExigoService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public class CartViewModel : IShoppingViewModel
    {
        public IEnumerable<IItem> Items { get; set; }
        public IEnumerable<IItem> AutoOrderItems { get; set; }

        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public string[] Errors { get; set; }

        public OrderCalculationResponse OrderTotals { get; set; }
        public OrderCalculationResponse AutoOrderTotals { get; set; }
        public IEnumerable<IShipMethod> ShipMethods { get; set; }
        public int ShipMethodID { get; set; }

        public decimal PointAccountBalance { get; set; }

        public Common.Api.ExigoWebService.CalculateOrderResponse OrderCalcResponse { get; set; }
    }
}