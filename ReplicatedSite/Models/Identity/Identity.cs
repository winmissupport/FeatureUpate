﻿using System.Web;
using ReplicatedSite.Services;


namespace ReplicatedSite
{
    public class Identity
    {
        public static CustomerIdentity Customer
        {
            get
            {
                var identity = HttpContext.Current.User.Identity as CustomerIdentity;
                return identity;
            }
        }
        public static ReplicatedSiteIdentity Owner
        {
            get
            {
                var identity = (HttpContext.Current.Items["OwnerWebIdentity"] as ReplicatedSiteIdentity);

                if (identity == null && Settings.RememberLastWebAliasVisited)
                {
                    var lastWebAlias = Settings.DefaultWebAlias;

                    var cookie = HttpContext.Current.Request.Cookies["LastWebAlias"];
                    if (cookie != null && cookie.Value.IsNotNullOrEmpty())
                    {
                        lastWebAlias = cookie.Value;
                    }

                    identity = IdentityService.GetIdentity(lastWebAlias);
                    HttpContext.Current.Items["OwnerWebIdentity"] = identity;
                }

                return identity;
            }
        }
    }
}