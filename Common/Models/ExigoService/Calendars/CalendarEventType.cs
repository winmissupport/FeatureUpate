﻿namespace ExigoService
{
    public class CalendarEventType : ICalendarEventType
    {
        public int CalendarEventTypeID { get; set; }
        public string CalendarEventTypeDescription { get; set; }
        public string Color { get; set; }
    }
}