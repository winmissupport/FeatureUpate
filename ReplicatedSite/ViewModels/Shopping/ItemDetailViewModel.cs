using ReplicatedSite.Models;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public class ItemDetailViewModel : IShoppingViewModel
    {
        public Item Item { get; set; }

        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public string[] Errors { get; set; }

        public IEnumerable<Item> RelatedItems { get; set; }
        public List<AutoOrder> AutoOrders { get; set; }
        public bool HasAutoOrder { get; set; }
        public ItemDetailViewModel()
        {
            this.AutoOrders = new List<AutoOrder>();
        }
    }
}