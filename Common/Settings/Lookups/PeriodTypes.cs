namespace Common
{
    public static class PeriodTypes
    {
        /// <summary>
        ///	Period Type 1
        /// </summary>
        public const int Monthly = 1;
        /// <summary>
        ///	Period Type 2
        /// </summary>
        public const int Weekly = 2;
        /// <summary>
        ///	Period Type 3
        /// </summary>
        public const int Quarterly = 3;
        /// <summary>
        ///	Period Type 100
        /// </summary>
        public const int Prelaunch = 100;

        /// <summary>
        /// The most commonly used period type. Set to Monthly (1).
        /// </summary>
        public const int Default = Monthly;
    }
}