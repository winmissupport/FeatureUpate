using Common;
using Common.Api.ExigoOData.LoggingContext;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static LoggingContext ODataLogging(int sandboxID = 0)
        {
            return GetODataContext<LoggingContext>(((sandboxID > 0) ? sandboxID : GlobalSettings.Exigo.Api.SandboxID));
        }
    }
}