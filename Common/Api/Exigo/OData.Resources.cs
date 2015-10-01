using Common;
using Common.Api.ExigoOData.ResourceManager;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Text;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static ResourcesContext ODataResources(int sandboxID = 0)
        {
            return GetODataContext<ResourcesContext>(((sandboxID > 0) ? sandboxID : GlobalSettings.Exigo.Api.SandboxID));
        }
    }
}
