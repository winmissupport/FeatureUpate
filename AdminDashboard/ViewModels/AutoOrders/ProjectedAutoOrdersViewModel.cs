using System;
using System.Collections.Generic;

namespace AdminDashboard.ViewModels
{
    public class ProjectedAutoOrdersViewModel
    {
        public IEnumerable<ProjectedAutoOrderDetail> Details { get; set; }

        public decimal HighestTotal { get; set; }
        public DateTime HighestTotalDate { get; set; }
        public DayOfWeek HighestTotalDayOfWeek { get; set; }
        public decimal AverageTotal { get; set; }
        public decimal LowestTotal { get; set; }
        public DateTime LowestTotalDate { get; set; }
        public DayOfWeek LowestTotalDayOfWeek { get; set; }
        public string ProjectedAutoOrdersChartXml { get; set; }
    }

    public class ProjectedAutoOrderDetail
    {
        public int Month { get; set; }
        public int Day { get; set; }
        public int Year { get; set; }
        public DateTime Date
        {
            get
            {
                return new DateTime(this.Year, this.Month, this.Day);
            }
            set
            {
                this.Month = value.Month;
                this.Day = value.Day;
                this.Year = value.Year;
            }
        }
        public DayOfWeek WeekDay
        {
            get
            {
                return this.Date.DayOfWeek;
            }
        }

        public decimal Total { get; set; }
    }
}