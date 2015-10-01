using System.Collections.Generic;

namespace ExigoService
{
    public class ItemCategory : IItemCategory
    {
        public int ItemCategoryID { get; set; }
        public string ItemCategoryDescription { get; set; }
        public int SortOrder { get; set; }

        public int? ParentItemCategoryID { get; set; }
        public IEnumerable<ItemCategory> Subcategories { get; set; }
        public IEnumerable<Item> SubcategoryItems { get; set; }
    }
}
