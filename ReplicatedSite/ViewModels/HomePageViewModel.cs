using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common;
using ExigoService;
using ReplicatedSite.Models;

namespace ReplicatedSite
{
    public class HomePageViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public IEnumerable<IItem> FeaturedProducts { get; set; }

        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public ShoppingCartItemsPropertyBag ShoppingCart { get; set; }
        public OrderCalculationResponse OrderTotals { get; set; }
        public string[] Errors { get; set; }
    }
}