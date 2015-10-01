using AdminDashboard.Filters;
using System.Web.Mvc;

namespace AdminDashboard
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            Common.Filters.GlobalFilters.Register(filters);
            filters.Add(new BackofficeAuthorizeAttribute());
        }
    }
}