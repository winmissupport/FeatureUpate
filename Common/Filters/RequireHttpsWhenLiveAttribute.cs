using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Common.Filters
{
    public class RequireHttpsWhenLiveAttribute : RequireHttpsAttribute
    {
        protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsLocal)
            {
                base.HandleNonHttpsRequest(filterContext);
            }
        }
    }
}
