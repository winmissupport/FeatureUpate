﻿using Common;
using Common.Api.ExigoWebService;

namespace ExigoService
{
    public static partial class Exigo
    {
        private static ExigoApi GetWebServiceContext(int sandboxID = 0)
        {
            return CreateWebServiceContext(sandboxID);
        }
        public static ExigoApi CreateWebServiceContext(int sandboxID)
        {
            // Determine which URL we should use
            var url = GetWebServiceUrl(sandboxID);

            // Create the context
            return new ExigoApi
            {
                ApiAuthenticationValue = new ApiAuthentication
                {
                    LoginName = GlobalSettings.Exigo.Api.LoginName,
                    Password = GlobalSettings.Exigo.Api.Password,
                    Company = GlobalSettings.Exigo.Api.CompanyKey
                },
                Url = url
            };
        }

        private static string GetWebServiceUrl()
        {
            return GetWebServiceUrl(GlobalSettings.Exigo.Api.SandboxID);
        }
        private static string GetWebServiceUrl(int sandboxID)
        {
            var urlFormat = "http://{0}.exigo.com/3.0/ExigoApi.asmx";
            var cname = "api";

            if (sandboxID > 0)
            {
                cname = "sandboxapi" + sandboxID;
            }

            return string.Format(urlFormat, cname);
        }
    }
}