using Common;
using Common.Api.ExigoOData;
using Common.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static ItemCategory GetItemCategory(int itemCategoryID)
        {
            var context = Exigo.OData();


            // Get the nodes
            var categories      = new List<ItemCategory>();
            var rowcount        = 50;
            var lastResultCount = rowcount;
            var callsMade       = 0;

            while (lastResultCount == rowcount)
            {
                // Get the data
                var results = context.WebCategories
                    .Where(c => c.WebID == 1)
                    .OrderBy(c => c.ParentID)
                    .OrderBy(c => c.SortOrder)
                    .Skip(callsMade * rowcount)
                    .Take(rowcount)
                    .Select(c => c)
                    .ToList();

                results.ForEach(c =>
                {
                    categories.Add((ItemCategory)c);
                });

                callsMade++;
                lastResultCount = results.Count;
            }


            // Recursively populate the children
            var category = categories.Where(c => c.ItemCategoryID == itemCategoryID).FirstOrDefault();
            if (category == null) return null;

            category.Subcategories = GetItemCategorySubcategories(category, categories);

            return category;
        }
        private static IEnumerable<ItemCategory> GetItemCategorySubcategories(ItemCategory parentCategory, IEnumerable<ItemCategory> categories)
        {
            var subCategories = categories.Where(c => c.ParentItemCategoryID == parentCategory.ItemCategoryID).ToList();

            foreach (var subCategory in subCategories)
            {
                subCategory.Subcategories = GetItemCategorySubcategories(subCategory, categories);
            }

            return subCategories;
        }

        public static IEnumerable<Item> GetItems(GetItemsRequest request)
        {
            // If we don't have what we need to make this call, stop here.
            if (request.Configuration == null)
                throw new InvalidRequestException("ExigoService.GetItems() requires an OrderConfiguration.");

            if (request.Configuration.CategoryID == 0 && request.CategoryID == null && request.ItemCodes.Length == 0)
                throw new InvalidRequestException("ExigoService.GetItems() requires either a CategoryID or a collection of item codes."); ;


            // Set some defaults
            if (request.CategoryID == null && request.ItemCodes.Length == 0)
            {
                request.CategoryID = request.Configuration.CategoryID;
            }

            // Create the contexts we will use
            var context = Exigo.OData();


            // Determine how many categories we need to pull based on the levels. Currently designed to go one level deep.
            var categoryIDs = new List<int>();
            if (request.CategoryID != null)
            {
                categoryIDs.Add((int)request.CategoryID);

                if (request.IncludeChildCategories)
                {
                    // Get the child categories
                    var ids = context.WebCategories
                        .Where(c => c.WebID == 1)
                        .Where(c => c.ParentID == (int)request.CategoryID)
                        .Select(c => new
                        {
                            c.WebCategoryID
                        }).ToList();

                    categoryIDs.AddRange(ids.Select(c => c.WebCategoryID));
                }
            }


            // If we requested a specific category, get the item codes in the category
            if (categoryIDs.Count > 0)
            {
                var categoryItemCodes = context.WebCategoryItems
                    .Where(c => c.WebID == 1)
                    .Where(categoryIDs.ToOrExpression<WebCategoryItem, int>("WebCategoryID"))
                    .Select(c => new
                    {
                        c.Item.ItemCode
                    }).ToList();

                var existingItemCodes = request.ItemCodes.ToList();
                existingItemCodes.AddRange(categoryItemCodes.Select(c => c.ItemCode).Distinct().ToList());
                request.ItemCodes = existingItemCodes.ToArray();
            }


            // If we don't have any items, stop here.
            if (request.ItemCodes.Length == 0) yield break;


            // Get the data
            var query = context.ItemWarehousePrices.Expand("Item")
                    .Where(c => c.WarehouseID == request.Configuration.WarehouseID)
                    .Where(c => c.PriceTypeID == request.Configuration.PriceTypeID)
                    .Where(c => c.CurrencyCode == request.Configuration.CurrencyCode);

            if (request.ItemCodes != null && request.ItemCodes.Count() > 0)
            {
                query = query.Where(request.ItemCodes.ToList().ToOrExpression<ItemWarehousePrice, string>("Item.ItemCode"));
            }

            var odataItems = query.ToList();
            var items = odataItems.Select(c => (ExigoService.Item)c).ToList();


            // Populate the group members and dynamic kits
            PopulateGroupMembers(items, request.Configuration);
            PopulateDynamicKitMembers(items);


            // Return the data
            foreach (var item in items)
            {
                yield return item;
            }
        }
        public static IEnumerable<Item> GetItems(IEnumerable<ShoppingCartItem> shoppingCartItems, IOrderConfiguration configuration)
        {
            // If we don't have what we need to make this call, stop here.
            if (configuration == null)
                throw new InvalidRequestException("ExigoService.GetItems() requires an OrderConfiguration.");

            if (shoppingCartItems.Count() == 0)
                yield break;


            // Create the contexts we will use
            var context = Exigo.OData();


            // Get the data
            var apiItems = context.ItemWarehousePrices.Expand("Item/GroupMembers")
                    .Where(c => c.WarehouseID == configuration.WarehouseID)
                    .Where(c => c.PriceTypeID == configuration.PriceTypeID)
                    .Where(c => c.CurrencyCode == configuration.CurrencyCode)
                    .Where(shoppingCartItems.Select(c => c.ItemCode).Distinct().ToList().ToOrExpression<ItemWarehousePrice, string>("Item.ItemCode"))
                    .ToList();


            // Loop through each of our cart items, and populate it with the known data
            var results = new List<Item>();
            foreach (var apiItem in apiItems)
            {
                var cartItems = shoppingCartItems.Where(c => c.ItemCode == apiItem.Item.ItemCode).ToList();
                foreach (var cartItem in cartItems)
                {
                    var newItem                 = (Item)apiItem;
                    newItem.ID                  = cartItem.ID;
                    newItem.Quantity            = cartItem.Quantity;
                    newItem.ParentItemCode      = cartItem.ParentItemCode;
                    newItem.GroupMasterItemCode = cartItem.GroupMasterItemCode;
                    newItem.DynamicKitCategory  = cartItem.DynamicKitCategory;
                    newItem.Type                = cartItem.Type;

                    yield return newItem;
                }
            }
        }

        // v2 methods
        public static IEnumerable<Item> GetItemList(GetItemListRequest request)
        {
            // If we don't have what we need to make this call, stop here.
            if (request.Configuration == null)
                throw new InvalidRequestException("ExigoService.GetItemList() requires an OrderConfiguration.");

            if (request.Configuration.CategoryID == 0 && request.CategoryID == null)
                throw new InvalidRequestException("ExigoService.GetItemList() requires either a CategoryID or a collection of item codes."); ;


            // Set some defaults
            if (request.CategoryID == null)
            {
                request.CategoryID = request.Configuration.CategoryID;
            }


            // Create the contexts we will use
            var context = Exigo.OData();

            // Get the item codes used in this category
            var itemCodes = context.WebCategoryItems
                    .Where(c => c.WebID == 1)
                    .Where(c => c.WebCategoryID == request.CategoryID)
                    .Select(c => new { c.Item.ItemCode })
                    .ToList()
                    .Select(c => c.ItemCode)
                    .ToList();



            // Get the item details
            var query = GetItemsQueryable(request.Configuration, itemCodes);
            var apiItems = query.Select(c => new 
            {
                ItemID                  = c.Item.ItemID,
                ItemCode                = c.Item.ItemCode,
                ItemDescription         = c.Item.ItemDescription,
                IsGroupMaster           = c.Item.IsGroupMaster,
                IsVirtual               = c.Item.IsVirtual,
                GroupMembersDescription = c.Item.GroupMembersDescription,
                IsDynamicKitMaster      = c.Item.IsDynamicKitMaster,
                ItemTypeID              = c.Item.ItemTypeID,
                SmallImageUrl           = c.Item.SmallImageUrl,
                AllowOnAutoOrder        = c.Item.AllowOnAutoOrder,
                CurrencyCode            = c.CurrencyCode,
                Price                   = c.Price,
                BV                      = c.BusinessVolume,
                CV                      = c.CommissionableVolume
            }).ToList();
            var items = (List<Item>)apiItems.ToNonAnonymousList(typeof(Item));


            // Populate the group members and dynamic kits
            PopulateAdditionalItemData(items, request.Configuration);


            // Return the converted items
            foreach (var item in items)
            {
                yield return item;
            }
        }
        public static Item GetItemDetail(GetItemDetailRequest request)
        {
            // If we don't have what we need to make this call, stop here.
            if (request.Configuration == null)
                throw new InvalidRequestException("ExigoService.GetItemDetail() requires an OrderConfiguration.");

            if (request.ItemCode.IsNullOrEmpty())
                throw new InvalidRequestException("ExigoService.GetItemDetail() requires an item code.");


            // Get the item details
            var query   = GetItemsQueryable(request.Configuration, new[] { request.ItemCode }, "Item");
            var apiItem = query.Select(c => c).FirstOrDefault();
            var item    = (ExigoService.Item)apiItem;


            // Populate the group members and dynamic kits
            PopulateAdditionalItemData(new[] { item }, request.Configuration);


            // Return the converted item
            return item;
        }
        public static IEnumerable<Item> GetCartItems(GetCartItemsRequest request)
        {
            // If we don't have what we need to make this call, stop here.
            if (request.Configuration == null)
                throw new InvalidRequestException("ExigoService.GetItemList() requires an OrderConfiguration.");

            if (request.ShoppingCartItems == null || request.ShoppingCartItems.Count() == 0)
                yield break;


            // Get the item codes used in this category
            var itemCodes = request.ShoppingCartItems.Select(c => c.ItemCode).ToList();


            // Get the item details
            var query = GetItemsQueryable(request.Configuration, itemCodes);
            var apiItems = query.Select(c => new
            {
                ItemID                  = c.Item.ItemID,
                ItemCode                = c.Item.ItemCode,
                ItemDescription         = c.Item.ItemDescription,
                IsGroupMaster           = c.Item.IsGroupMaster,
                IsVirtual               = c.Item.IsVirtual,
                GroupMembersDescription = c.Item.GroupMembersDescription,
                IsDynamicKitMaster      = c.Item.IsDynamicKitMaster,
                ItemTypeID              = c.Item.ItemTypeID,
                TinyImageUrl            = c.Item.TinyImageUrl,
                AllowOnAutoOrder        = c.Item.AllowOnAutoOrder,
                CurrencyCode            = c.CurrencyCode,
                Price                   = c.Price,
                BV                      = c.BusinessVolume,
                CV                      = c.CommissionableVolume
            }).ToList();
            var items = (List<Item>)apiItems.ToNonAnonymousList(typeof(Item));


            // Populate the shopping cart item detail into the items
            foreach (var cartItem in request.ShoppingCartItems)
            {
                var item = items.Where(c => c.ItemCode == cartItem.ItemCode).FirstOrDefault();
                if (item == null) continue;

                item.ID                  = cartItem.ID;
                item.Quantity            = cartItem.Quantity;
                item.ParentItemCode      = cartItem.ParentItemCode;
                item.GroupMasterItemCode = cartItem.GroupMasterItemCode;
                item.DynamicKitCategory  = cartItem.DynamicKitCategory;
                item.Type                = cartItem.Type;
            }


            // Populate the group members and dynamic kits
            PopulateAdditionalItemData(items, request.Configuration);


            // Return the converted items
            foreach (var item in items)
            {
                yield return item;
            }
        }



        private static IQueryable<ItemWarehousePrice> GetItemsQueryable(IOrderConfiguration configuration, IEnumerable<string> itemCodes , params string[] expansions)
        {
            var query = Exigo.OData().ItemWarehousePrices;
            if (expansions != null && expansions.Length > 0)
            {
                query = query.Expand(string.Join(",", expansions));
            }

            return query
                    .Where(c => c.WarehouseID == configuration.WarehouseID)
                    .Where(c => c.PriceTypeID == configuration.PriceTypeID)
                    .Where(c => c.CurrencyCode == configuration.CurrencyCode)
                    .Where(itemCodes.ToList().ToOrExpression<ItemWarehousePrice, string>("Item.ItemCode"))
                    .AsQueryable();
        }

        private static void PopulateAdditionalItemData(IEnumerable<Item> items, IOrderConfiguration configuration)
        {
            GlobalUtilities.RunAsyncTasks(
                () => { PopulateGroupMembers(items, configuration); },
                () => { PopulateDynamicKitMembers(items); }
            );
        }
        private static void PopulateGroupMembers(IEnumerable<Item> items, IOrderConfiguration configuration)
        {
            // Determine if we have any group master items
            var groupMasterItemCodes = items.Where(c => c.IsGroupMaster).Select(c => c.ItemCode).ToList();
            if (groupMasterItemCodes.Count == 0) return;

            // Get the item group members
            var context = Exigo.OData();
            var apiItemGroupMembers = context.ItemGroupMembers
                .Where(groupMasterItemCodes.ToOrExpression<Common.Api.ExigoOData.ItemGroupMember, string>("MasterItemCode"))
                .ToList();

            // Bind the item group members to the items
            var itemGroupMembers = apiItemGroupMembers.Select(c => (ItemGroupMember)c).ToList();

            // Get a collection of images for each of these group members
            var memberItemCodes = apiItemGroupMembers.Select(c => c.ItemCode).ToList();
            var memberData = Exigo.OData().ItemWarehousePrices
                .Where(i => i.CurrencyCode == configuration.CurrencyCode)
                .Where(i => i.PriceTypeID == configuration.PriceTypeID)
                .Where(memberItemCodes.ToOrExpression<Common.Api.ExigoOData.ItemWarehousePrice, string>("Item.ItemCode"))
                .Select(i => new AdditionalDataCollection()
                {
                    ItemCode = i.Item.ItemCode,
                    LargeImage = i.Item.LargeImageUrl,
                    SmallImage = i.Item.SmallImageUrl,
                    Price = i.Price,
                    RetailPrice = i.Other1Price,
                    ShortDescription = i.Item.ShortDetail,
                    LongDescription1 = i.Item.LongDetail,
                    LongDescription2 = i.Item.LongDetail2,
                    AllowOnAutoOrder = i.Item.AllowOnAutoOrder
                }).ToList();
            

            foreach (var groupMasterItemCode in groupMasterItemCodes)
            {
                var item = items.Where(c => c.ItemCode == groupMasterItemCode).FirstOrDefault();
                if (item == null) continue;

                item.GroupMembers = itemGroupMembers
                    .Where(c => c.MasterItemCode == groupMasterItemCode)
                    .OrderBy(c => c.SortOrder)
                    .ToList();
                
                // Populate the item's basic details for cart purposes
                foreach (var groupMember in item.GroupMembers)
                {
                    var data = memberData.FirstOrDefault(i => i.ItemCode == groupMember.ItemCode) ?? new AdditionalDataCollection();
                    groupMember.Item = groupMember.Item ?? new Item();
                    groupMember.Item.ItemCode = groupMember.ItemCode;
                    groupMember.Item.GroupMasterItemCode = groupMasterItemCode;
                    groupMember.Item.LargeImageUrl = data.LargeImage;
                    groupMember.Item.SmallImageUrl = data.SmallImage;
                    groupMember.Item.Price = data.Price;
                    groupMember.Item.OtherPrice1 = data.RetailPrice;
                    groupMember.Item.ShortDetail1 = data.ShortDescription;
                    groupMember.Item.LongDetail1 = data.LongDescription1;
                    groupMember.Item.LongDetail2 = data.LongDescription2;
                    groupMember.Item.AllowOnAutoOrder = data.AllowOnAutoOrder;
                }
            }
        }
        public class AdditionalDataCollection
        {
            public string ItemCode { get; set; }
            public string TinyImage { get; set; }
            public string SmallImage { get; set; }
            public string LargeImage { get; set; }
            public decimal Price { get; set; }
            public decimal RetailPrice { get; set; }
            public string ShortDescription { get; set; }
            public string LongDescription1 { get; set; }
            public string LongDescription2 { get; set; }
            public bool AllowOnAutoOrder { get; set; }
        }

        private static void PopulateDynamicKitMembers(IEnumerable<Item> items)
        {
            // Determine if we have any dynamic kit items
            var dynamicKitMasterItemCodes = items.Where(c => c.IsDynamicKitMaster).Select(c => c.ItemCode).ToList();
            if (dynamicKitMasterItemCodes.Count == 0) return;

            // Get the dynamic kit data
            var context = Exigo.OData();
            var apiItemDynamicKitCagtegoryMembers = context.ItemDynamicKitCategoryMembers.Expand("MasterItem,DynamicKitCategory/DynamicKitCategoryItemMembers/DynamicKitCategory,DynamicKitCategory/DynamicKitCategoryItemMembers/Item")
                .Where(dynamicKitMasterItemCodes.ToOrExpression<Common.Api.ExigoOData.ItemDynamicKitCategoryMember, string>("MasterItem.ItemCode"))
                .ToList();

            // Bind the item group members to the items
            foreach (var dynamicKitMasterItemCode in dynamicKitMasterItemCodes)
            {
                var item = items.Where(c => c.ItemCode == dynamicKitMasterItemCode).FirstOrDefault();
                if (item == null) continue;

                var apiCategories = apiItemDynamicKitCagtegoryMembers.Where(c => c.MasterItem.ItemCode == dynamicKitMasterItemCode).ToList();
                item.DynamicKitCategories = apiCategories.Select(c => (DynamicKitCategory)c).ToList();

                foreach (var category in item.DynamicKitCategories)
                {
                    foreach (var categoryItem in category.Items)
                    {
                        categoryItem.ParentItemCode = dynamicKitMasterItemCode;
                    }
                }
            }
        }
    }
}