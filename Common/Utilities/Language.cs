using Common.Api.ExigoWebService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Linq;

namespace Common
{
    public static partial class GlobalUtilities
    {
        public static string GetCultureCode(int langID)
        {
            switch (langID)
            {
                case Languages.English:
                    return "en-US";
                case Languages.Spanish:
                    return "es-MX";
                case Languages.Dutch:
                    return "nl-NL";
                case Languages.German:
                    return "de-DE";   
                default:
                    return "en-US";
            }
        }

        public static string GetSelectedLanguage(string defaultLanguage = "en-US")
        {
            //If the cookie LanguagePreference exists, we set the culture code appropriately
            var languageCookie = HttpContext.Current.Request.Cookies[GlobalSettings.Globalization.LanguageCookieName];           

            if (languageCookie == null)
            {
                languageCookie = new HttpCookie(GlobalSettings.Globalization.LanguageCookieName);
                languageCookie.Expires = DateTime.Now.AddYears(1);
                languageCookie.Value = defaultLanguage;
                HttpContext.Current.Response.Cookies.Add(languageCookie);
            }


            var language = GlobalSettings.Globalization.AvailableLanguages.Where(c => c.CultureCode == languageCookie.Value).FirstOrDefault();           
            if (language == null)
            {
                languageCookie.Value = GetCultureCode(Languages.English);
            }


            return languageCookie.Value;
        }

        public static int GetSelectedLanguageID(int defaultLanguageID = 0)
        {
            //If the cookie LanguagePreference exists, we set the culture code appropriately
            var languageCookie = HttpContext.Current.Request.Cookies[GlobalSettings.Globalization.LanguageCookieName];

            if (languageCookie == null)
            {
                languageCookie = new HttpCookie(GlobalSettings.Globalization.LanguageCookieName);
                languageCookie.Expires = DateTime.Now.AddYears(1);
                languageCookie.Value = "en-US";
                HttpContext.Current.Response.Cookies.Add(languageCookie);
            }


            var language = GlobalSettings.Globalization.AvailableLanguages.Where(c => c.CultureCode == languageCookie.Value).FirstOrDefault();
           


            return language.LanguageID;
        }
    }
}