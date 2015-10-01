namespace Common
{
    public static class CustomerTypes
    {
        /// <summary>
        ///	Customer Type 0
        /// </summary>
        public const int ProspectorLead = 0;
        /// <summary>
        ///	Customer Type 1
        /// </summary>
        public const int RetailCustomer = 1;
        /// <summary>
        ///	Customer Type 2
        /// </summary>
        public const int SmartShopper = 2;
        /// <summary>
        ///	Customer Type 4
        /// </summary>
        public const int BrandPartner = 3;
        /// <summary>
        ///	Happy Hour
        /// </summary>
        public const int Employee = 4;


        /// <summary>
        ///	A collection of distributor types
        /// </summary>
        public static readonly int[] DistributorTypes = new[] { BrandPartner };
    }
}