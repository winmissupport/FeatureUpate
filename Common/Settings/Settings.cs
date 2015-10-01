using System.Collections.Generic;
using System.Linq;
using ExigoService;
using Common.Api.ExigoWebService;
using Payments;

namespace Common
{
    public static class GlobalSettings
    {
        /// <summary>
        /// Exigo-specific API credentials and configurations
        /// </summary> 
        public static class Exigo
        {
            /// <summary>
            /// Web service, OData and SQL API credentials and configurations
            /// </summary>
            public static class Api
            {
                public const string LoginName = "API_WinWorldwide";
                public const string Password = "Ap!W!nWorldw!de&&";
                public const string CompanyKey = "win";

                public static bool UseSandboxGlobally = true; //2015-07-28 When dealing with iDEAL change line 25 and line 144/145. Also go to Ingenico.cs and change line 157/160.  All other testing just change line 25.
                public static int SandboxID { get { return (UseSandboxGlobally) ? 2 : 0; } }

                /// <summary>
                /// Web Service configurations
                /// </summary>
                public static class WebService
                {
                    public static string LiveUrl = "http://api.exigo.com/3.0/ExigoApi.asmx";
                    public static string Sandbox1Url = "http://sandboxapi1.exigo.com/3.0/ExigoApi.asmx";
                    public static string Sandbox2Url = "http://sandboxapi2.exigo.com/3.0/ExigoApi.asmx";
                    public static string Sandbox3Url = "http://sandboxapi3.exigo.com/3.0/ExigoApi.asmx";
                    public static string Sandbox4Url = "http://sandboxapi4.exigo.com/3.0/ExigoApi.asmx";
                }

                /// <summary>
                /// Odata configurations
                /// </summary>
                public static class OData
                {
                    public static string LiveUrl = "http://api.exigo.com/4.0/" + GlobalSettings.Exigo.Api.CompanyKey;
                    public static string Sandbox1Url = "http://sandboxapi1.exigo.com/4.0/" + GlobalSettings.Exigo.Api.CompanyKey;
                    public static string Sandbox2Url = "http://sandboxapi2.exigo.com/4.0/" + GlobalSettings.Exigo.Api.CompanyKey;
                    public static string Sandbox3Url = "http://sandboxapi3.exigo.com/4.0/" + GlobalSettings.Exigo.Api.CompanyKey;
                    public static string Sandbox4Url = "http://sandboxapi4.exigo.com/4.0/" + GlobalSettings.Exigo.Api.CompanyKey;
                }

                /// <summary>
                /// Replicated SQL connection strings and configurations
                /// </summary>
                public static class Sql
                {
                    public static class ConnectionStrings
                    {
                        public static string SqlReporting = "Server=win.bi.exigo.com;database=WINReporting;uid=Exigo;pwd=@iti_qLr2C97;pooling=false;";
                    }
                }
            }

            /// <summary>
            /// Payment API credentials
            /// </summary>
            public static class PaymentApi
            {
                public const string LoginName = "WIN_7JZrXQB4E";
                public const string Password = "2g0NU1hkwmNjYGWBBKVb45mp";
            }
        }

        /// <summary>
        /// Default backoffice settings
        /// </summary>
        public static class Backoffices
        {
            public static int SessionTimeout = 30; // In minutes

            /// <summary>
            /// Silent login URL's and configurations
            /// </summary>
            public static class SilentLogins
            {
                public static string DistributorBackofficeUrl = "http://backoffice.mywinlife.com/silentlogin/?token={0}";
                public static string RetailCustomerBackofficeUrl = "http://www.mywinlife.com/account/silentlogin/?token={0}";
            }

            /// <summary>
            /// Waiting room configurations
            /// </summary>
            public static class WaitingRooms
            {
                /// <summary>
                /// The number of days a customer can be placed in a waiting room after their initial enrollment.
                /// </summary>
                public static int GracePeriod = 60; // In days // Updated per ticket 65531 on 12 June 2015 - Alan C
            }
        }

        /// <summary>
        /// Default replicated site settings
        /// </summary>
        public static class ReplicatedSites
        {
            public static string DefaultWebAlias = "www";
            public static int DefaultAccountID = 1;
            public static int IdentityRefreshInterval = 15; // In minutes
            public static string ReplicatedSiteLoginNameCookie = Company.Name + "ReplicatedSiteUser";
        }

        /// <summary>
        /// Market configurations used for orders, autoOrders, products and more
        /// </summary>
        public static class Markets
        {
            public static List<Market> AvailableMarkets = new List<Market>
                                                        {
                                                            new UnitedStatesMarket(),
                                                            new BelgiumMarket(),
                                                            new GermanyMarket(),
                                                            new NetherlandsMarket(),
                                                            new UnitedKingdomMarket()
                                                        };
        }      

        public static class Merchants
        {
            public static class Ingenico
            {
                //public static string Address = "https://secure.ogone.com/ncol/test/orderstandard_utf8.asp"; // Test URL
                public static string Address = "https://secure.ogone.com/ncol/prod/orderstandard_utf8.asp"; // Production URL
                //public static string MerchantID = "1031308";
                //public static string AccessCode = "423B3225";
                //public static string VPCCommand = "pay";
                //public static string VPCVersion = "1";
                //public static string ReturnURL = "";
                //public static string SecureSecret = "64D0B8B74709F12111E9BB593DDD7385";
            }
        }

        /// <summary>
        /// Language and culture code configurations
        /// </summary>
        public static class Globalization
        {
            public static string CountryCookieName = Company.Name + "SelectedCountry";
            public static string LanguageCookieName = Company.Name + "SelectedLanguage";
            public static string MarketSet =  "HasSetCustomerMarket";
            public static List<Language> AvailableLanguages = new List<Language>
                                                                {
                                                                    new Language(Languages.English, "en-US"),
                                                                    //new Language(Languages.Spanish, "es-MX"),
                                                                    //new Language(Languages.German, "de-DE"),
                                                                    //new Language(Languages.Dutch, "nl-NL")
                                                                };
        }

        /// <summary>
        /// Language and culture code configurations
        /// </summary>
        public static class AutoOrders
        {
            public static List<int> AvailableFrequencyTypeIDs = new List<int>
                                                                        {
                                                                            FrequencyTypes.Monthly,
                                                                            FrequencyTypes.Quarterly,
                                                                            FrequencyTypes.Yearly
                                                                        };
            public static List<FrequencyType> AvailableFrequencyTypes = AvailableFrequencyTypeIDs.Select(c => ExigoService.Exigo.GetFrequencyType(c)).ToList();
        }

        /// <summary>
        /// Customer avatar configurations
        /// </summary>
        public static class Avatars
        {
            public static string DefaultAvatarAsBase64 = "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8UHRofHh0aHBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5PTgyPC4zNDL/2wBDAQkJCQwLDBgNDRgyIRwhMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjL/wAARCAEsASwDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD0WiiigAooooAKKKKACiiigBO1LRRQAneloooAKKKKAEpaKKACiiigApO1LRQAUUUUAFFFFABRRRQAUUUUAJ3paKKACiiigAoopO1AC0UUUAFFFFABRSd6WgAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAopKWgBOKWiigAooooAKKTvS0AJS0UUAFJS0UAFFFFABRRRQAUnelooAKKKKACiiigApO9LRQAnaloooAKKKKACiiigBKKWigAopO9LQAUlLRQAUUUUAFJS0UAJS0UUAFFFFACUtFFABSUtFABSUtJQAtFFFABRRRQAUUUUAFFFFABRRRQAUUUnegBaKKKACinRxvK4SNSzHsK17bQJGwbiTYP7q8mgDGpyo7/AHULfQV1cOl2cH3YQx9W5q4FVRhQB9KAOMFpdHpbSn/gBprW06fehkH1Q121FAHCkYNFdrJbwyjEkSN9VrPn0O1lyY90Te3IoA5qir11pVzaguV3oP4lqjQAUUneloAKKKKACiiigAooooAKKKKACiiigAooooAKKTvS0AFFFFABRRRQAUnalooAKKKKACiiigAq9YaZJetuOUiHVvX6U/S9NN3J5knEKn/vr2rp1VUUKoAUcADtQBFbWkNomyFMep7mp6KKACiiigAooooAKKKKACsq/wBHiuQXhAjl/Rq1aKAOIlieCQxyKVYdQaZXXX9hHfRYPEgHyt6Vyk0TwStHIuGWgBlFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFACd6WiigAooooAKKKKACk5paKACrFlaNeXKxDherH0FV66jRrUW9mJCP3kvzH6dqAL8USQxLGgwqjAFPoooAKKKKACikpaACiiigAooooAKKKKACszV7D7TD5qD96g7fxD0rTooA4Wir+r2gtbzKDCSfMPaqFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQBPZQ/abyOLszc/SuyAwMDpXO+H4g11JIf4VwPxro6ACiiigAooooAKKO1HagAooooAO9HakpaACiiigAooooAzdat/OsGcfejO4f1rl67eRBJE6HoykGuJZdrFT1BwaAEooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAoopO1AHQ+Hl/cTN6uBW1WP4e/49JR/00/pWxQAUUUUAFFFJ3oAWiiigAooooAKKKKACiiigAooooAK4y+XbfTr/tmuzrjtROdRuP8AfNAFaiiigAooooAKKKKACiiigAooooAKKKKACkpaKACiiigAooooAKKKKAN3w6/+vj+jVu1y2izeVqKqTxICtdTQAUUUUAFFJS0AFFFFABRRRQAUUUUAFFFFABRRRQAVxVw/mXMr/wB5yf1rrL6b7PZTSdwpx9a46gAooooAKKKKACiiigAooooAKKKKAE70tFFABRRRQAUUUnegBaKKKACik7UtACo5jkV14ZTkV2dvMLi3SVejDNcXW1oV7tc2rn5W5T60AdBRRRQAUUdqKACikpaACiiigAooooAO1FFFABRRUU0yQQtK5wqjJoAyNfucLHbA9fmb+lYNS3E73M7zP95j+VRUAFJ2paTvQAtFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFKrFWDA4I5BpKKAOq0zUFvIcMcTL94evvWhXEQzPBKssbbWXvXT6fqUV6m0kLMOq+v0oA0KKKKACiiigAooooAKKKKACiimswRSzEBR1JoAUkAZNczq+ofapPJiP7lD1/vGpNU1bz8wW5Ij6M396sigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACk70tFABRRRQAUUUnegBaKKKAClVmRgykhh0Iq5Z6XcXnzAbI/7zD+Vb1ppVta4bb5kn95qAINMvLudQs0DFe0vStaiigAopKWgAooooAKKKKAGSu0cZZELkdFB61y+o311cSFJlaJR/BjFdXUUsEc6bZUDr7igDiqK3bvQOr2r/APAG/wAaxZI3hcpIhVh2IoAZRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUVJBBJcSrHEu5jQAxEeRwiKWY8ACugsNFSLElzh36hewq3YadHZJnhpT95sfyq9QAgAAwKWiigAooooAKKSloAKKKKACiiigAooo7UAFV7mzhu02ypn0PcVYooA5K+02WybP34j0cD+dUq7hlV1KsAVPUGuc1PSTbZmgBMXcf3aAMqiiigAooooAKKKKACiiigAooooAKKKTtQAtFFFABRSUtABRRRgmgB8UTzSrHGu5m4ArqtPsUsYcDmRvvNUOk6eLWLzZB++f8A8dHpWnQAUUUd6ACjtRRQAUlFLQAUUUUAFFFFABRRRQAUUUUAFFFFABSEAjB6UtFAHNarpn2ZjNEP3THkf3ayq7h0WRCrgMp4INcnqNi1lcYHMbcqf6UAU6KKKACiiigAooooAKKKKACiik7UALRRRQAlFLRQAVr6LY+dL9okHyIflHqazLeB7idIU+8xxXY28K28CRIAFUYoAlooooAKKKKACiiigAooooAKO1FFABRRRQAUUUUAFFFFABRRRQAUUUUAFVr21S8tmibg9VPoas0UAcPJG0UjRuMMpwRTa3des+l0g9n/AKGsKgAooooAKKKKACiiigAooooAKKKKACiiljRpJFRclmOBQBu6BagK10w5Pyp/WtyoreFbeBIl6KMVLQAUUUUAFFFHagApKWigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKAGSxrNE0bjKsMGuMuImt7h4m6qcV21c/4gttskdwo4b5W+tAGLRRRQAUUUUAFFFFACd6WiigAopKWgArT0O382+8wj5Yxu/HtWZXSaDDssmkPWRv0FAGtRR2ooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAqpqMH2mxlTGTjK/UVbooA4Wip72HyL2aPsG4+lQUAFFFFABRRRQAUUUUAFFFFABXZWUfk2UMfogz9a5GBPMuI0/vOBXa0ALRRRQAUlLRQAUUUUAFFFFABRRSUAHeloooAKKKKACiiigAo7UUUAFFFFABR2oooAKKKKACiiigDmtfjC3qPj76frWVXQeIY8wQyf3WxXP0AFFFFABRRRQAUUUUAFFJ2ooAuaWu/U4B/tZ/KuvrldF51SP2B/lXVUAFHaiigA70UlLQAUUd6O9ABRRRQAlLRR3oAKKKKACiiigAooooAKKKKACiiigAoo70UAFFFFABRRR3oAzdcXdpjn+6wNcvXW6tzpc/0H865KgAooooAKKTvS0Af//Z";
        }

        /// <summary>
        /// Error logging configuration
        /// </summary>
        public static class ErrorLogging
        {
            public static bool ErrorLoggingEnabled = true;
            public static string[] EmailRecipients = new string[] { "jamesw@exigo.com" };
        }

        /// <summary>
        /// Email configurations
        /// </summary>
        public static class Emails
        {
            public static string NoReplyEmail = "noreply@mywinlife.com";
            public static string VerifyEmailUrl = "http://backoffice.mywinlife.com/verifyemail"; //Live Site Url
            //public static string VerifyEmailUrl = "https://devwinoffice.azurewebsites.net/verifyemail"; // Testing Site Url

            public static class SMTPConfigurations
            {
                public static SMTPConfiguration Default = new SMTPConfiguration
                {
                    Server = "smtpcorp.com",
                    Port = 2525,
                    Username = "mis.support@winltd.com",
                    Password = "ExMail50",
                    EnableSSL = false
                };
            }
        }

        /// <summary>
        /// Company information
        /// </summary>
        public static class Company
        {
            public static int CorporateCalendarAccountID = 1;
            public static string Name = "WIN Worldwide";

            // Corporate Address 
            public static Address Address = new Address()
            {
                Address1 = "5700 Tennyson Parkway",
                Address2 = "Suite 350",
                City = "Plano",
                State = "TX",
                Zip = "75024",
                Country = "US"
            };

            public static Address NLContactUs = new Address()
            {
                Address1 = "Kruisweg 583",
                Address2 = "2132 NA Hoofddorp",
                Country = "Netherlands"
            };

            // United States / Default Warehouse Address - Use for US Customer Order Calculation if Address / Identity is null
            public static ShippingAddress UnitedStatesWarehouseAddress = new ShippingAddress()
            {
                Company = "US Warehouse",
                FirstName = "WIN",
                LastName = "Worldwide",
                Address1 = "5800 Democracy Dr",
                Address2 = "",
                City = "Plano",
                State = "TX",
                Zip = "75024-4919",
                Country = "US",
                Phone = "1-800-430-4674",
                Email = "everybody@mywinlife.com"
            };

            // Holland Warehouse Address- Use for Netherlands Order Calculation if Address / Identity is null
            public static ShippingAddress HollandWarehouseAddress = new ShippingAddress()
            {
                Company = "Holland Warehouse",
                FirstName = "WIN",
                LastName = "Worldwide",
                Address1 = "Kruisweg 583",
                Address2 = "",
                City = "Hoofddorp",
                State = "NH",
                Zip = "2132NA",
                Country = "NL",
                Phone = "1-800-430-4674",
                Email = "everybody@mywinlife.com"
            };

            public static string USHours = "(M-F 8 a.m. - 5 p.m. CST)";
            public static string NLHours = "(M-F 13:00 - 18:00 CEST)";
            public static string NLPhone = "+31 20 446 4646";
            public static string Phone = "972.943.5220";
            public static string Email = "support@mywinlife.com";
            public static string Facebook = "http://www.facebook.com/WINWorldwide";
            public static string Twitter = "https://twitter.com/WINWorldwide";
            public static string LinkedIn = "https://www.linkedin.com/company/wellness-international-network";
            public static string YouTube = "https://www.youtube.com/c/myWINlife";
            public static string Pinterest = "https://www.pinterest.com/winworldwide/";
            public static string GooglePlus = "https://plus.google.com/114532244967030869135/posts";
            public static string Blog = "http://www.medium.com/@WINWorldwide";
            public static string HideAndWatch = "http://www.hideandwatch.info/";
            public static string DefaultCompanyMessage = "It Pays to Live Well®";
            // Share Button links
            public static string SubscribeToYouTubeChannel = "https://www.youtube.com/user/winltdusa?sub_confirmation=1";
            public static string ShareGooglePlus = "";

        }

        /// <summary>
        /// EncryptionKeys used for silent logins and other AES encryptions
        /// </summary>
        public static class Encryptions
        {
            public static string General = "SDFN0SD97FN09348590238M4"; // 24 characters

            public static string Key = GlobalSettings.Exigo.Api.CompanyKey + "silentlogin";
            public static string IV = "2m7^JXw5$9nTy3@0"; // Must be 16 characters long                            
        }

        /// <summary>
        /// Encryptions used for silent logins and other AES encryptions
        /// </summary>
        //public static class Encryptions
        //{
        //    public static class General
        //    {
        //        public static string Key = GlobalSettings.Exigo.Api.CompanyKey + "token";
        //        public static string IV  = "xxxxxxxxxxxxxxxx"; // Must be 16 characters long
        //    }                               

        //    public static class SilentLogins
        //    {
        //        public static string Key = GlobalSettings.Exigo.Api.CompanyKey + "silentlogin";
        //        public static string IV  = "xxxxxxxxxxxxxxxx"; // Must be 16 characters long
        //    }
        //}

        /// <summary>
        /// Regular expressions used throughout all websites
        /// </summary>
        public static class RegularExpressions
        {
            public const string EmailAddresses = "[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?";
            public const string LoginName      = "^[a-zA-Z0-9]+$";
            public const string Password       = "(?=^.{8,}$)(?=.*[0-9])(?=.*[a-zA-Z])([a-zA-Z0-9]+)$";
        }
    }

    public enum MarketName
    {
        UnitedStates,
        Belgium,
        Germany,
        Netherlands,
        UnitedKingdom
    }      
    public enum AvatarType
    {
        Tiny,
        Small,
        Default,
        Large
    }
    public enum SocialNetworks
    {
        Facebook   = 1,
        GooglePlus = 2,
        Twitter    = 3,
        Blog       = 4,
        LinkedIn   = 5,
        MySpace    = 6,
        YouTube    = 7,
        Pinterest  = 8
    }
    public static class CustomerStatusTypes
    {
        public const int Active = 1;
        public const int Terminated = 2;
        public const int Inactive = 3;

    }
    public static class NewsDepartments
    {
        public const int Backoffice = 7;
    }
}