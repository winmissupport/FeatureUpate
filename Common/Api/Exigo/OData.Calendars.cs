using Common.Api.ExigoOData.Calendars;
using Common;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static CalendarContext ODataCalendars(int sandboxID = 0)
        {
            return GetODataContext<CalendarContext>(((sandboxID > 0) ? sandboxID : GlobalSettings.Exigo.Api.SandboxID));
        }
    }
}