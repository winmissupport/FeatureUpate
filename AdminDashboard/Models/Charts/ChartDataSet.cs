using System.Collections.Generic;

namespace AdminDashboard.Models.FusionCharts
{
    public class ChartDataSet : FusionChartDataSet
    {
        public ChartDataSet()
        {
            Sets = new List<ChartDataSetDetail>();
        }
        public ChartDataSet(string seriesName)
        {
            base.SeriesName = seriesName;
            Sets = new List<ChartDataSetDetail>();
        }

        public List<ChartDataSetDetail> Sets { get; set; }
    }
}