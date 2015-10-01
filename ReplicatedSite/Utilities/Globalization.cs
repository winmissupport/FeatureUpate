using Common;
using ExigoService;
using System.Linq;
using System.Web;

namespace ReplicatedSite
{
    public static class Utilities
    {
        /// <summary>
        /// Gets the market the website is currently using.
        /// </summary>
        /// <returns>The Market object representing the current market.</returns>
        public static Market GetCurrentMarket()
        {
            // Get the user's country to see which market we are in
            var country = (HttpContext.Current.Request.IsAuthenticated) ? Identity.Customer.Country : GlobalSettings.Company.Address.Country;
            var market = GlobalSettings.Markets.AvailableMarkets.Where(c => c.Countries.Contains(country)).FirstOrDefault();

            // If we didn't find a market for the user's country, get the first default market
            if (market == null) market = GlobalSettings.Markets.AvailableMarkets.Where(c => c.IsDefault == true).FirstOrDefault();

            // If we didn't find a default market, get the first market we find
            if (market == null) market = GlobalSettings.Markets.AvailableMarkets.FirstOrDefault();

            // Return the market
            return market;
        }
        /// <summary>
        /// Gets the market the website is currently using.
        /// </summary>
        /// <returns>The Market object representing the current market.</returns>
        public static Market GetCurrentSelectedMarket()
        {
            // Get the user's country to see which market we are in
            var country = (HttpContext.Current.Request.IsAuthenticated) ? Identity.Customer.Country : GlobalSettings.Company.Address.Country;
            var market = GlobalSettings.Markets.AvailableMarkets.Where(c => c.Countries.Contains(country)).FirstOrDefault();

            // If we didn't find a market for the user's country, get the first default market
            if (market == null) market = GlobalSettings.Markets.AvailableMarkets.Where(c => c.IsDefault == true).FirstOrDefault();

            // If we didn't find a default market, get the first market we find
            if (market == null) market = GlobalSettings.Markets.AvailableMarkets.FirstOrDefault();

            // Return the market
            return market;
        }

        /// <summary>
        /// Gets the language the user's preference is set to.
        /// </summary>
        /// <returns>The Language object representing the current user's language preference.</returns>
        public static Language GetCurrentSelectedLanguage()
        {
            // Get the user's language preference based on their saved preference
            var languageCookie = HttpContext.Current.Request.Cookies[GlobalSettings.Globalization.LanguageCookieName];
            var language = GlobalSettings.Globalization.AvailableLanguages.FirstOrDefault(l => l.CultureCode == "en-US");

            if (languageCookie != null && !languageCookie.Value.IsEmpty())
            {
                language = GlobalSettings.Globalization.AvailableLanguages.FirstOrDefault(l => l.CultureCode == languageCookie.Value);
            }

            return language;
        }

        /// <summary>
        /// Set the current language (Language Cookie) of the site
        /// </summary>
        /// <param name="lang">Must be a valid culture code</param>
        public static void SetCurrentLanguage(string lang) 
        {
            // Get the user's language preference based on their saved preference
            var languageCookie = HttpContext.Current.Request.Cookies[GlobalSettings.Globalization.LanguageCookieName];

            if (languageCookie != null && !languageCookie.Value.IsEmpty())
            {
                languageCookie.Value = lang;
                HttpContext.Current.Response.Cookies.Add(languageCookie);
            }
        }

                /// <summary>
        /// Set the current country (Country Cookie) of the site
        /// </summary>
        /// <param name="country">Must be a valid Exigo country code</param>
        public static void SetCurrentCountry(string country) 
        {
            // Get the user's language preference based on their saved preference
            var countryCookie = HttpContext.Current.Request.Cookies[GlobalSettings.Globalization.CountryCookieName];

            if (countryCookie != null && !countryCookie.Value.IsEmpty())
            {
                countryCookie.Value = country;
                HttpContext.Current.Response.Cookies.Add(countryCookie);
            }
        }

        

        /// <summary>
        /// Gets the language the user's preference is set to.
        /// </summary>
        /// <returns>The Language object representing the current user's language preference.</returns>
        public static Language GetCurrentLanguage()
        {
            // Get the user's language preference based on their saved preference
            var languageID = (HttpContext.Current.Request.IsAuthenticated) ? Identity.Customer.LanguageID : Languages.English;
            var language = GlobalSettings.Globalization.AvailableLanguages.Where(c => c.LanguageID == languageID).FirstOrDefault();

            // If we couldn't find the user's preferred language, return the first one we find.
            if (language == null) language = GlobalSettings.Globalization.AvailableLanguages.FirstOrDefault();

            // Return the language
            return language;
        }

    }
}