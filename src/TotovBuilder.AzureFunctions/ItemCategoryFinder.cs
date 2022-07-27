using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions
{
    public class ItemCategoryFinder
    {
        private readonly IItemCategoriesFetcher _itemCategoriesFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCategoryFinder"/> class.
        /// </summary>
        /// <param name="itemCategoriesFetcher">Item categories fetcher.</param>
        public ItemCategoryFinder(IItemCategoriesFetcher itemCategoriesFetcher)
        {
            _itemCategoriesFetcher = itemCategoriesFetcher;
        }

        /// <summary>
        /// Finds an item category from a tarkov item category ID.
        /// When no matching item category is found, return the "other" item category.
        /// </summary>
        /// <param name="tarkovCategoryIds">Tarkov item category IDs.</param>
        /// <returns>Item category.</returns>
        public async Task<ItemCategory> FindFromTarkovCategoryId(string tarkovCategoryId)
        {
            IEnumerable<ItemCategory> itemCategories = await _itemCategoriesFetcher.Fetch() ?? Array.Empty<ItemCategory>();
            ItemCategory? result = itemCategories.FirstOrDefault(ic => ic.Types.Any(tic => tic.Id == tarkovCategoryId));

            if (result == null)
            {
                result = new ItemCategory() { Id = "other" };
            }

            return result;
        }
    }
}
