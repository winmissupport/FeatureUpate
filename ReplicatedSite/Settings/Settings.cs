using Common;

namespace ReplicatedSite
{
    public static class Settings
    {
        public static string DefaultWebAlias                    = GlobalSettings.ReplicatedSites.DefaultWebAlias;
        public static int IdentityRefreshInterval               = GlobalSettings.ReplicatedSites.IdentityRefreshInterval;
        public static bool RememberLastWebAliasVisited          = true;
        public static bool AllowOrphans                         = true;

        public static string SmartShopperSubscriptionItemCode = "TestSmart01";

        public static int SessionTimeout                        = GlobalSettings.Backoffices.SessionTimeout;

        public static class SilentLogins
        {
            public static string BrandPartnerBackofficeUrl = "http://backoffice.mywinlife.com/silentlogin/?token={0}";
        }
    }

    public enum EnrollmentType
    {
        Distributor = 1,
        SmartShopper = 2
    }

    public enum SearchType
    {
        webaddress = 1,
        distributorID = 2,
        distributorInfo = 3,
        zipcode = 4,
        eventcode = 5,
        eventname = 6
    }

}