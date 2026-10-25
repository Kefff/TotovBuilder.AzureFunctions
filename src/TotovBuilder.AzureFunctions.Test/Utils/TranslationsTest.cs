using System.Text.Json;
using FluentAssertions;
using TotovBuilder.AzureFunctions.Utils;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Utils
{
    /// <summary>
    /// Represents tests on the <see cref="Translations"/> class.
    /// </summary>
    public class TranslationsTest
    {
        [Theory]
        [InlineData("MOD_PISTOL_GRIP", "Pistol grip")]
        [InlineData("UNKNOWN_KEY", "UNKNOWN_KEY")]
        [InlineData("NOT_A_STRING", "NOT_A_STRING")]
        public void Translate_ShouldReturnTranslationOrKey(string key, string expected)
        {
            // Arrange
            JsonElement translationsJson = JsonDocument.Parse("{ \"MOD_PISTOL_GRIP\": \"Pistol grip\", \"NOT_A_STRING\": 1 }").RootElement;
            Translations translations = new(translationsJson);

            // Act
            string result = translations.Translate(key);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void Translate_WithTranslationsThatAreNotAnObject_ShouldReturnKey()
        {
            // Arrange
            JsonElement translationsJson = JsonDocument.Parse("[\"Pistol grip\"]").RootElement;
            Translations translations = new(translationsJson);

            // Act
            string result = translations.Translate("MOD_PISTOL_GRIP");

            // Assert
            result.Should().Be("MOD_PISTOL_GRIP");
        }
    }
}
