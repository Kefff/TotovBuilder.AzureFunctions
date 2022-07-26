using FluentAssertions;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test
{
    /// <summary>
    /// Represents tests on the <see cref="StringExtensions"/> class.
    /// </summary>
    public class StringExtensionsTest
    {
        [Theory]
        [InlineData("THORAX", "Thorax")]
        [InlineData("Full Auto", "Full auto")]
        public void ToStringCase_ShouldReturnStringMatchingStringCase(string stringToFormat, string expected)
        {
            // Act
            string formattedString = StringExtensions.ToStringCase(stringToFormat);

            // Asssert
            formattedString.Should().Be(expected);
        }
    }
}
