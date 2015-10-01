using System;
using System.Net;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Common.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ValidateAntiForgeryTokenOnAllPostsAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;

            //  Only validate POSTs
            if (request.HttpMethod == WebRequestMethods.Http.Post)
            {
                try
                {


                    //  Ajax POSTs and normal form posts have to be treated differently when it comes
                    //  to validating the AntiForgeryToken
                    if (request.IsAjaxRequest())
                    {
                        var antiForgeryCookie = request.Cookies[AntiForgeryConfig.CookieName];

                        var cookieValue = antiForgeryCookie != null
                            ? antiForgeryCookie.Value
                            : null;

                        var token = request.Headers["__RequestVerificationToken"] ?? request.Form["__RequestVerificationToken"];

                        AntiForgery.Validate(cookieValue, token);
                    }
                    else
                    {
                        new ValidateAntiForgeryTokenAttribute()
                            .OnAuthorization(filterContext);
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }
    }
}
