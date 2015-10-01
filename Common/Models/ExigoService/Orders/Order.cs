using System;
using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public class Order : IOrder
    {
        public Order()
        {
            Details = new List<OrderDetail>();
            TrackingNumbers = new List<string>();
        }

        public int OrderID { get; set; }
        public int CustomerID { get; set; }

        public string CurrencyCode { get; set; }
        public int WarehouseID { get; set; }
        public int ShipMethodID { get; set; }
        public int OrderStatusID { get; set; }
        public int OrderTypeID { get; set; }
        public int PriceTypeID { get; set; }
        public string Notes { get; set; }

        public int? AutoOrderID { get; set; }
        public int? ReturnOrderID { get; set; }
        public int? ParentOrderID { get; set; }
        public int? TransferToCustomerID { get; set; }
        public int DeclineCount { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }

        public IEnumerable<OrderDetail> Details { get; set; }
        public IEnumerable<IPayment> Payments { get; set; }

        public ShippingAddress Recipient { get; set; }

        public decimal Total { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal ShippingTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal WeightTotal { get; set; }
        public decimal BVTotal { get; set; }
        public decimal CVTotal { get; set; }

        public IEnumerable<string> TrackingNumbers { get; set; }

        public decimal Other1Total { get; set; }
        public decimal Other2Total { get; set; }
        public decimal Other3Total { get; set; }
        public decimal Other4Total { get; set; }
        public decimal Other5Total { get; set; }
        public decimal Other6Total { get; set; }
        public decimal Other7Total { get; set; }
        public decimal Other8Total { get; set; }
        public decimal Other9Total { get; set; }
        public decimal Other10Total { get; set; }

        public string Other11 { get; set; }
        public string Other12 { get; set; }
        public string Other13 { get; set; }
        public string Other14 { get; set; }
        public string Other15 { get; set; }
        public string Other16 { get; set; }
        public string Other17 { get; set; }
        public string Other18 { get; set; }
        public string Other19 { get; set; }
        public string Other20 { get; set; }

        public bool IsOpen
        {
            get { return OrderStatusID < 7; }
        }
        public bool WasCreatedFromAutoOrder
        {
            get { return this.AutoOrderID != null; }
        }
        public bool IsVirtualOrder
        {
            get { return this.Details != null && this.Details.All(d => d.IsVirtual); }
        }
        public bool IsReturnOrder
        {
            get { return this.ReturnOrderID != null; }
        }
        public bool HasDeclinedAtLeastOnce
        {
            get { return this.DeclineCount > 0; }
        }
        public bool HasShipped
        {
            get { return OrderStatusID == 9; }
        }
        public bool HasTrackingNumbers
        {
            get { return TrackingNumbers != null && TrackingNumbers.Count() > 0; }
        }
        public string OrderStatusDescription
        {
            get
            {
                switch (OrderStatusID)
                {
                    default: return "Unknown (Call customer service for more info)";

                    case 1: return "Pending (Call customer service for more info)";

                    case 0:
                    case 5: return "Pending payment information (Credit Card)";
                    case 6: return "Pending payment information";

                    case 2: return "Pending payment information (Payment was declined)";
                    case 3: return "Pending payment information (Payment was declined)";

                    case 4: return "Cancelled";

                    case 7: return "Preparing your order";

                    case 8: return "About to ship";

                    case 9: return "Shipped " + ((this.ShippedDate != null) ? ((DateTime)this.ShippedDate).ToString("dddd, MMMM d, yyyy") : "");
                }
            }
        }
        public bool CanBeCancelled
        {
            get { return new int[] { 0, 1, 2, 3, 5, 6, 10 }.Contains(OrderStatusID); }
        }
    }
}