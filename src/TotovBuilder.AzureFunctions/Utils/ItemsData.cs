using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using static TotovBuilder.AzureFunctions.Utils.JsonDeserializer;

namespace TotovBuilder.AzureFunctions.Utils
{
    /// <summary>
    /// Represents methods for reading the API items file.
    /// </summary>
    public static class ItemsData
    {
        /// <summary>
        /// Gets the items of the <c>data</c> property of the items file.
        /// </summary>
        /// <param name="itemsDataJson">Content of the <c>data</c> property of the items file.</param>
        /// <param name="itemsPath">Path of the items file.</param>
        /// <param name="logger">Logger used to log the error when there are no items.</param>
        /// <returns>Json element representing the items by item ID.</returns>
        public static Result<JsonElement> GetItems(JsonElement itemsDataJson, string itemsPath, ILogger logger)
        {
            if (!TryDeserializeObject(itemsDataJson, "items", out JsonElement itemsJson)
                || !itemsJson.EnumerateObject().Any())
            {
                string error = string.Format(Properties.Resources.InvalidApiResponseData, itemsPath, itemsDataJson.GetRawText());
                logger.LogError(error);

                return Result.Fail(error);
            }

            return Result.Ok(itemsJson);
        }

        /// <summary>
        /// Indicates whether an item is a preset.
        /// </summary>
        /// <param name="itemJson">Json element representing the item.</param>
        /// <returns><c>true</c> if the item is a preset; otherwise <c>false</c>.</returns>
        public static bool IsPreset(JsonElement itemJson)
        {
            return TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson)
                && TryDeserializeString(propertiesJson, "propertiesType", out string propertiesType)
                && propertiesType == "ItemPropertiesPreset";
        }
    }
}
