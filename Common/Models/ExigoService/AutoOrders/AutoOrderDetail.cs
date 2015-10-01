﻿namespace ExigoService
{
    public class AutoOrderDetail : IAutoOrderDetail
    {
        public int AutoOrderDetailID { get; set; }
        public int AutoOrderID { get; set; }

        public int ItemID { get; set; }
        public int? ParentItemID { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceEach { get; set; }
        public decimal PriceTotal { get; set; }
        public bool IsVirtual { get; set; }

        public decimal BVEach { get; set; }
        public decimal BV { get; set; }
        public decimal CVEach { get; set; }
        public decimal CV { get; set; }

        public decimal? PriceEachOverride { get; set; }
        public decimal? TaxableEachOverride { get; set; }
        public decimal? ShippingPriceEachOverride { get; set; }
        public decimal? BVEachOverride { get; set; }
        public decimal? CVEachOverride { get; set; }

        public string ImageUrl { get; set; }
    }
}