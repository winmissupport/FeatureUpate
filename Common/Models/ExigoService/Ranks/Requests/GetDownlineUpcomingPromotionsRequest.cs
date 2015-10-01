using Common.Kendo;

namespace ExigoService
{
    public class GetDownlineUpcomingPromotionsRequest : DataRequest
    {
        public int DownlineCustomerID { get; set; }
        public int PeriodTypeID { get; set; }
        public int? RankID { get; set; }
        public KendoGridRequest KendoGridRequest { get; set; }
    }
}
