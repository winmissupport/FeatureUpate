namespace ExigoService
{
    public class ResourceCategory : IResourceCategory
    {
        public int ResourceCategoryID { get; set; }
        public string ResourceCategoryDescription { get; set; }
        //2015-08-19
        //Ivan S.
        //64467
        //Added a new field to the extended table to be able to sort the categories
        public int? ResourceCategoryOrder { get; set; }
    }
}
