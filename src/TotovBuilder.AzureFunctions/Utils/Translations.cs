using System.Text.Json;

namespace TotovBuilder.AzureFunctions.Utils
{
    /// <summary>
    /// Represents the translations of an API file in a language.
    /// </summary>
    public class Translations
    {
        /// <summary>
        /// Translated texts by translation key.
        /// </summary>
        private readonly Dictionary<string, string> Texts = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="Translations"/> class.
        /// </summary>
        /// <param name="translationsJson">Content of the <c>data</c> property of an API translations file.</param>
        public Translations(JsonElement translationsJson)
        {
            if (translationsJson.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            foreach (JsonProperty translationJson in translationsJson.EnumerateObject())
            {
                if (translationJson.Value.ValueKind == JsonValueKind.String)
                {
                    Texts[translationJson.Name] = translationJson.Value.GetString()!;
                }
            }
        }

        /// <summary>
        /// Translates a translation key.
        /// </summary>
        /// <param name="key">Translation key.</param>
        /// <returns>Translated text, or the key itself when it has no translation.</returns>
        public string Translate(string key)
        {
            return Texts.TryGetValue(key, out string? text) ? text : key;
        }
    }
}
