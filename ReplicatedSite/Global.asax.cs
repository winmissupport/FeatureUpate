using Common;
using Common.Helpers;
using ReplicatedSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.WebPages;

namespace ReplicatedSite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static DateTime ApplicationStartDate;

        public override void Init()
        {
            this.BeginRequest += new EventHandler(Application_BeginRequest);
            this.PostAuthenticateRequest += new EventHandler(MvcApplication_PostAuthenticateRequest);

            base.Init();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DisplayConfig.RegisterDisplayModes(DisplayModeProvider.Instance.Modes);
            ModelBinderConfig.RegisterModelBinders(ModelBinders.Binders);

            // Set the application's start date for easy reference
            ApplicationStartDate = DateTime.Now;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Get the route data
            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(HttpContext.Current));

            // Account for attribute routing and null routeData
            if (routeData != null && routeData.Values.ContainsKey("MS_DirectRouteMatches"))
            {
                routeData = ((List<RouteData>)routeData.Values["MS_DirectRouteMatches"]).First();
            }


            // If we have an identity and the current identity matches the web alias in the routes, stop here.
            var identity = HttpContext.Current.Items["OwnerWebIdentity"] as ReplicatedSiteIdentity;
            if (routeData == null
                || routeData.Values["webalias"] == null
                || (identity != null && identity.WebAlias.Equals(routeData.Values["webalias"].ToString(), StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }
            
            // SETS CULTURE DONT DELETE
            var countryCookie = HttpContext.Current.Request.Cookies[GlobalSettings.Globalization.CountryCookieName];
            if (countryCookie!= null && countryCookie.Value != "")
            {
                GlobalUtilities.SetCurrentCulture();
                GlobalUtilities.SetCurrentUICulture();
            }

            // Determine some web alias data
            var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), RouteTable.Routes.GetRouteData(new HttpContextWrapper(HttpContext.Current))));
            var currentWebAlias = routeData.Values["webalias"].ToString();
            var defaultWebAlias = Settings.DefaultWebAlias;
            var lastWebAlias = GlobalUtilities.GetLastWebAlias(defaultWebAlias);
            var defaultPage = urlHelper.Action(routeData.Values["action"].ToString(), routeData.Values["controller"].ToString(), new { webalias = lastWebAlias });

            // Silent login logic
            if (currentWebAlias == "silentlogin")
            {
                return;
            }

            // If we need to redirect to another users replicated site, the refreshowner cookie will exist
            var refreshCookie = HttpContext.Current.Request.Cookies["refreshowner"];
            if (refreshCookie != null && refreshCookie.Value != defaultWebAlias)
            {
                // Reset our default page so that we have the new correct web alias value
                GlobalUtilities.SetLastWebAlias(refreshCookie.Value);
                lastWebAlias = refreshCookie.Value;
                var isSmartShopper = Identity.Customer.CustomerTypeID == CustomerTypes.SmartShopper;

                object parameters = new
                {
                    webalias = lastWebAlias
                };

                
                if (isSmartShopper) {
                    HttpContext.Current.Items["OwnerWebIdentity"] = IdentityService.GetIdentity(currentWebAlias);
                    if (HttpContext.Current.Items["OwnerWebIdentity"] != null)
                    {
                        var ownerIdentity = HttpContext.Current.Items["OwnerWebIdentity"].As<ReplicatedSiteIdentity>();
                        ownerIdentity.WebAlias = Identity.Customer.WebAlias;

                        if (ownerIdentity.WebAlias != currentWebAlias)
                        {
                            currentWebAlias = ownerIdentity.WebAlias;

                            defaultPage = urlHelper.Action(routeData.Values["action"].ToString(), routeData.Values["controller"].ToString(), new { webalias = currentWebAlias });
                            HttpContext.Current.Response.RedirectPermanent(defaultPage);
                        }

                        if (Settings.RememberLastWebAliasVisited && currentWebAlias.ToLower() != Settings.DefaultWebAlias.ToLower())
                        {
                            GlobalUtilities.SetLastWebAlias(currentWebAlias);
                        }
                        else
                        {
                            GlobalUtilities.DeleteLastWebAlias();
                        }
                    }
                    else
                    {
                        HttpContext.Current.Response.Redirect(urlHelper.Action("invalidwebalias", "error"));
                    }
                }


                defaultPage = urlHelper.Action(routeData.Values["action"].ToString(), routeData.Values["controller"].ToString(), parameters);

                refreshCookie.Expires = DateTime.Now.AddYears(-2);
                HttpContext.Current.Response.Cookies.Add(refreshCookie);
                HttpContext.Current.Response.RedirectPermanent(defaultPage);
            }

            // If we are an orphan and we don't allow them, redirect to a capture page.
            if (!Settings.AllowOrphans && currentWebAlias.Equals(defaultWebAlias, StringComparison.InvariantCultureIgnoreCase))
            {
                HttpContext.Current.Response.Redirect(urlHelper.Action("webaliasrequired", "error"));
            }


            // If we are an orphan, try to redirect the user back to a previously-visited replicated site
            if (Settings.RememberLastWebAliasVisited
                && currentWebAlias.Equals(defaultWebAlias, StringComparison.InvariantCultureIgnoreCase)
                && !defaultWebAlias.Equals(lastWebAlias, StringComparison.InvariantCultureIgnoreCase))
            {
                HttpContext.Current.Response.Redirect(defaultPage);
            }


            // Attempt to authenticate the web alias
            HttpContext.Current.Items["OwnerWebIdentity"] = IdentityService.GetIdentity(currentWebAlias);
            if (HttpContext.Current.Items["OwnerWebIdentity"] != null)
            {
                // Initialize the selected market
                identity = HttpContext.Current.Items["OwnerWebIdentity"].As<ReplicatedSiteIdentity>();
                var country = (identity.Country != null) ? identity.Country : "US";
                GlobalUtilities.GetSelectedCountryCode(country, false);

                if (Settings.RememberLastWebAliasVisited && currentWebAlias.ToLower() != Settings.DefaultWebAlias.ToLower())
                {
                    GlobalUtilities.SetLastWebAlias(currentWebAlias);
                }
                else
                {
                    GlobalUtilities.DeleteLastWebAlias();
                }
            }
            else
            {
                if (Settings.RememberLastWebAliasVisited)
                {
                    GlobalUtilities.DeleteLastWebAlias();
                    lastWebAlias = defaultWebAlias;
                    HttpContext.Current.Response.Redirect(defaultPage);
                }
                else
                {
                    HttpContext.Current.Response.Redirect(urlHelper.Action("invalidwebalias", "error"));
                }
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            try
            {
                if (GlobalSettings.ErrorLogging.ErrorLoggingEnabled && !Request.IsLocal)
                {
                    ErrorLogger.LogException(Server.GetLastError(), Request.RawUrl);
                }
            }
            catch { }
        }

        void MvcApplication_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                var identity = CustomerIdentity.Deserialize(authCookie.Value);
                if (identity == null)
                {
                    FormsAuthentication.SignOut();
                }
                else
                {
                    HttpContext.Current.User = new GenericPrincipal(identity, null);
                    Context.User = new GenericPrincipal(identity, null);

                }
            }
        }
    }
}