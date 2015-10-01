using ReplicatedSite.Models;
using Common.Providers;
using ExigoService;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ReplicatedSite.Providers
{
    public class EnrollmentLogicProvider : BaseLogicProvider
    {
        #region Constructors
        public EnrollmentLogicProvider() : base() { }
        public EnrollmentLogicProvider(Controller controller, ShoppingCartItemsPropertyBag cart, EnrollmentPropertyBag propertyBag)
        {
            Controller = controller;
            Cart = cart;
            PropertyBag = propertyBag;
        }
        #endregion

        #region Properties
        public ShoppingCartItemsPropertyBag Cart { get; set; }
        public EnrollmentPropertyBag PropertyBag { get; set; }
        #endregion

        #region Logic
        public override CheckLogicResult CheckLogic()
        {
            if (!HasEnrollerID(PropertyBag.EnrollerID))
            {
                return CheckLogicResult.Failure(RedirectToAction("enrollmentconfiguration"));
            }
            if (!HasSelectedCountry(PropertyBag.ShippingAddress))
            {
                return CheckLogicResult.Failure(RedirectToAction("countryselection"));
            }

            if (!HasValidShippingAddress(PropertyBag.ShippingAddress))
            {
                return CheckLogicResult.Failure(RedirectToAction("personalinfo"));
            }

            if (!HasValidPackDetails(Cart.Items))
            {
                return CheckLogicResult.Failure(RedirectToAction("packs"));
            }

            //if (!HasValidOrderDetails(Cart.Items))
            //{
            //    return CheckLogicResult.Failure(RedirectToAction("ProductList"));
            //}

            //if (!HasValidShippingAddress(PropertyBag.ShippingAddress))
            //{
            //    return CheckLogicResult.Failure(RedirectToAction("EnrolleeInfo"));
            //}

            //if (!HasValidPaymentMethod(PropertyBag.PaymentMethod))
            //{
            //    return CheckLogicResult.Failure(RedirectToAction("EnrolleeInfo"));
            //}

            return CheckLogicResult.Success(RedirectToAction("Review"));
        }

        public bool HasEnrollerID(int EnrollerID)
        {
            return EnrollerID != null && EnrollerID > 0;
        }
        public bool HasSelectedCountry(ShippingAddress address)
        {
            return address != null && !address.Country.IsEmpty();
        }

        public bool HasValidPackDetails(IEnumerable<IShoppingCartItem> items)
        {
            return items.Where(c => c.Type == ShoppingCartItemType.EnrollmentPack || c.Type == ShoppingCartItemType.EnrollmentAutoOrderPack).Count() > 0;
        }
        public bool HasValidOrderDetails(IEnumerable<IShoppingCartItem> items)
        {
            return items.Where(c => c.Type == ShoppingCartItemType.Order || c.Type == ShoppingCartItemType.AutoOrder).Count() > 0;
        }
        public bool HasValidShippingAddress(ShippingAddress address)
        {
            return address != null && address.IsComplete;
        }
        public bool HasValidPaymentMethod(IPaymentMethod paymentMethod)
        {
            return paymentMethod != null &&
                (paymentMethod is CreditCard || paymentMethod is BankAccount);
        }
        #endregion
    }
}