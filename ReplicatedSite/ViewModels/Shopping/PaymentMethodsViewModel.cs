﻿using ReplicatedSite.Models;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public class PaymentMethodsViewModel : IShoppingViewModel
    {
        public IEnumerable<IPaymentMethod> PaymentMethods { get; set; }
        public IEnumerable<Address> Addresses { get; set; }

        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public string[] Errors { get; set; }
        public bool UsePaymentForAutoOrders { get; set; }
        public bool IsAutoOrder { get; set; }

        public bool HasAutoOrderItems { get; set; }
        public string ExistingCardCVV { get; set; }

        public int PaymentTypeID { get; set; }
    }
}