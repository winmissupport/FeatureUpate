using System.Collections.Generic;
using System.Web.Mvc;

namespace Common.Kendo
{
    [ModelBinder(typeof(KendoGridModelBinder))]
    public class KendoGridRequest
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public string Logic { get; set; }

        public string SqlWhereClause
        {
            get { return KendoUtilities.GetSqlWhereClause(this.FilterObjectWrapper.FilterObjects); }
        }
        public string SqlOrderByClause
        {
            get { return KendoUtilities.GetSqlOrderByClause(this.SortObjects); }
        }

        public FilterObjectWrapper FilterObjectWrapper { get; set; }
        public IEnumerable<SortObject> SortObjects { get; set; }
    }
}