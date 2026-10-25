using System.Text.Json;
using static System.Text.Json.JsonElement;

namespace TotovBuilder.AzureFunctions.Utils
{
    /// <summary>
    /// Represents methods for deserializing properties of JSON elements.
    /// </summary>
    public static class JsonDeserializer
    {
        /// <summary>
        /// Tries to deserialize an array.
        /// </summary>
        /// <param name="jsonElement">Json element containing the property to deserialize.</param>
        /// <param name="propertyName">Name of the property to deserialize.</param>
        /// <param name="value">Deserialized value when successful.</param>
        /// <returns><c>true</c> when the deserialization succeeds; otherwise <c>false</c>.</returns>
        public static bool TryDeserializeArray(JsonElement jsonElement, string propertyName, out ArrayEnumerator value)
        {
            value = new ArrayEnumerator();

            if (!TryGetProperty(jsonElement, propertyName, out JsonElement propertyJson)
                || propertyJson.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            value = propertyJson.EnumerateArray();

            return true;
        }

        /// <summary>
        /// Tries to deserialize a boolean value.
        /// </summary>
        /// <param name="jsonElement">Json element containing the property to deserialize.</param>
        /// <param name="propertyName">Name of the property to deserialize.</param>
        /// <param name="value">Deserialized value when successful.</param>
        /// <returns><c>true</c> when the deserialization succeeds; otherwise <c>false</c>.</returns>
        public static bool TryDeserializeBoolean(JsonElement jsonElement, string propertyName, out bool value)
        {
            value = false;

            if (!TryGetProperty(jsonElement, propertyName, out JsonElement propertyJson)
                || (propertyJson.ValueKind != JsonValueKind.False && propertyJson.ValueKind != JsonValueKind.True))
            {
                return false;
            }

            value = propertyJson.GetBoolean();

            return true;
        }

        /// <summary>
        /// Tries to deserialize a double value.
        /// </summary>
        /// <param name="jsonElement">Json element containing the property to deserialize.</param>
        /// <param name="propertyName">Name of the property to deserialize.</param>
        /// <param name="value">Deserialized value when successful.</param>
        /// <returns><c>true</c> when the deserialization succeeds; otherwise <c>false</c>.</returns>
        public static bool TryDeserializeDouble(JsonElement jsonElement, string propertyName, out double value)
        {
            value = 0;

            if (!TryGetProperty(jsonElement, propertyName, out JsonElement propertyJson)
                || propertyJson.ValueKind != JsonValueKind.Number)
            {
                return false;
            }

            value = propertyJson.GetDouble();

            return true;
        }

        /// <summary>
        /// Tries to deserialize an object value.
        /// </summary>
        /// <param name="jsonElement">Json element containing the property to deserialize.</param>
        /// <param name="propertyName">Name of the property to deserialize.</param>
        /// <param name="value">Deserialized value when successful.</param>
        /// <returns><c>true</c> when the deserialization succeeds; otherwise <c>false</c>.</returns>
        public static bool TryDeserializeObject(JsonElement jsonElement, string propertyName, out JsonElement value)
        {
            value = new JsonElement();

            if (!TryGetProperty(jsonElement, propertyName, out JsonElement propertyJson)
                || propertyJson.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            value = propertyJson;

            return true;
        }

        /// <summary>
        /// Tries to deserialize a string value.
        /// </summary>
        /// <param name="jsonElement">Json element containing the property to deserialize.</param>
        /// <param name="propertyName">Name of the property to deserialize.</param>
        /// <param name="value">Deserialized value when successful.</param>
        /// <returns><c>true</c> when the deserialization succeeds; otherwise <c>false</c>.</returns>
        public static bool TryDeserializeString(JsonElement jsonElement, string propertyName, out string value)
        {
            value = string.Empty;

            if (!TryGetProperty(jsonElement, propertyName, out JsonElement propertyJson)
                || propertyJson.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            value = propertyJson.GetString()!;

            return true;
        }

        /// <summary>
        /// Tries to get a property of a JSON object.
        /// </summary>
        /// <param name="jsonElement">Json element containing the property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">Property value when successful.</param>
        /// <returns><c>true</c> when the element is an object containing the property; otherwise <c>false</c>.</returns>
        private static bool TryGetProperty(JsonElement jsonElement, string propertyName, out JsonElement value)
        {
            value = new JsonElement();

            return jsonElement.ValueKind == JsonValueKind.Object
                && jsonElement.TryGetProperty(propertyName, out value);
        }
    }
}
