using ExigoService;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace Common
{
    public static partial class GlobalUtilities
    {
        /// <summary>
        /// Sets the CultureCode of the site based on the current market.
        /// </summary>
        public static void SetCurrentCulture( string cultureCode = "")
        {
            if (cultureCode.IsEmpty())
            {


                var country = GetSelectedCountryCode();
                //if (string.IsNullOrEmpty(country))
                //{
                //    country = "US";
                //}
                var market = GlobalSettings.Markets.AvailableMarkets.Where(c => c.Countries.Contains(country)).FirstOrDefault();

                cultureCode = market.CultureCode;
            }
            //if (!HttpContext.Current.Request.IsAuthenticated) return;

            var culture = CultureInfo.CreateSpecificCulture(cultureCode);
            if (culture.Name == "en-GB")
            {
                culture.NumberFormat.CurrencySymbol = "€";
            }
            Thread.CurrentThread.CurrentCulture = culture;
        }

        /// <summary>
        /// Sets the CurrentUICulture of the site based on the user's language preferences.
        /// </summary>
        public static void SetCurrentUICulture(string cultureCode = "")
        {
            //if (!HttpContext.Current.Request.IsAuthenticated) return;
            if (cultureCode.IsEmpty())
            {
                var country = GetSelectedCountryCode();
                if (string.IsNullOrEmpty(country))
                {
                    country = "US";
                }
                var market = GlobalSettings.Markets.AvailableMarkets.Where(c => c.Countries.Contains(country)).FirstOrDefault();
                cultureCode = market.CultureCode;
            }

            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(cultureCode);       
        }


        /// <summary>
        /// Gets the configuration for the market name provided.
        /// </summary>
        public static IMarketConfiguration GetMarketConfiguration(MarketName marketName)
        {
            return GlobalSettings.Markets.AvailableMarkets.FirstOrDefault(c => c.Name == marketName).GetConfiguration();
        }

        /// <summary>
        /// Get the selected country code 
        /// </summary>
        /// <returns></returns>
        public static string GetSelectedCountryCode(string countryCode = "", bool overwrite = true)
        {
            try
            {
                var cookie = HttpContext.Current.Request.Cookies[GlobalSettings.Globalization.CountryCookieName];


                if (!countryCode.IsEmpty() && overwrite)
                {
                    cookie = new HttpCookie(GlobalSettings.Globalization.CountryCookieName);
                    cookie.Value = countryCode;
                    HttpContext.Current.Response.Cookies.Add(cookie);

                    return countryCode;
                }

                if (cookie != null && !cookie.Value.IsEmpty())
                {
                    return cookie.Value;
                }
                else
                {
                    cookie = new HttpCookie(GlobalSettings.Globalization.CountryCookieName);
                    cookie.Value = countryCode;

                    HttpContext.Current.Response.Cookies.Add(cookie);
                    var market = GlobalSettings.Markets.AvailableMarkets.Where(c => c.Countries.Contains(countryCode)).FirstOrDefault();
                    var cultureCode = market.CultureCode;
                    SetCurrentCulture(cultureCode);

                    return cookie.Value;
                }
            }
            catch
            {
                return "US";
            }
        }

        /// <summary>
        /// Get the selected country code 
        /// </summary>
        /// <returns></returns>
        public static string SetSelectedCountryCode(string countryCode)
        {
            var cookie = HttpContext.Current.Request.Cookies[GlobalSettings.Globalization.CountryCookieName];

            cookie = new HttpCookie(GlobalSettings.Globalization.CountryCookieName);
            cookie.Value = countryCode;
            HttpContext.Current.Response.Cookies.Add(cookie);

            return countryCode;
        }


        /// <summary>
        /// Gets the configuration for the country code provided
        /// </summary>
        public static IMarketConfiguration GetMarketConfigurationByCountry(string countryCode = null)
        {
            var country = (countryCode == null) ?  GlobalUtilities.GetSelectedCountryCode() : countryCode;

            var market = GlobalSettings.Markets.AvailableMarkets.FirstOrDefault(c => c.Countries.Contains(country));

            if (market != null)
            {
                return market.GetConfiguration();
            }
            else
            {
                return new UnitedStatesMarket().GetConfiguration();
            }
        }

        /// <summary>
        /// Finds requested ssl status
        /// </summary>
        public static bool GetHttpsStatus(string urlPath) // Ticket 66982 26June2015 JWJ
        {
            if (urlPath.Contains("payment")) { return true; }
            else if (urlPath.Contains("register")) { return true; }
            else if (urlPath.Contains("orderlist")) { return true; }
            else if (urlPath.Contains("review")) { return true; }
            else if (urlPath.Contains("login")) { return true; }
            else if (urlPath.Contains("account")) { return true; }
            else if (urlPath.Contains("pendingautoorderlist")) { return true; }
            else {return false;}
               
            
        }
    }
}