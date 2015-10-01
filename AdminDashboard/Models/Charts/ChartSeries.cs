using System.Collections.Generic;

public class ChartSeries
{
    public ChartSeries()
    {
        Values = new List<decimal>();
    }

    #region Properties
    public string SeriesName { get; set; }
    public List<decimal> Values { get; set; }
    #endregion
}