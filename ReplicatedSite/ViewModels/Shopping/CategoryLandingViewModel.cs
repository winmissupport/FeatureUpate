﻿using ReplicatedSite.Models;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public class CategoryLandingViewModel : IShoppingViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public IEnumerable<Item> Items { get; set; }
        public ItemDetailViewModel QuickShop { get; set; }

        public CategoryViewModel CategoryOptions { get; set; }
        public string MainCategoryDescription { get; set; }

        public int Page { get; set; }
        public int RecordCount { get; set; }

        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public string[] Errors { get; set; }
    }
}