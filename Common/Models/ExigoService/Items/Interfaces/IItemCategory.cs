using System.Collections.Generic;

namespace ExigoService
{
    public interface IItemCategory
    {
        int ItemCategoryID { get; set; }
        string ItemCategoryDescription { get; set; }
        int SortOrder { get; set; }        
        
        int? ParentItemCategoryID { get; set; }
        IEnumerable<ItemCategory> Subcategories { get; set; }
    }
}
