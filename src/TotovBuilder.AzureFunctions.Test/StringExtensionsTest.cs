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
        [InlineData("Left arm", "LeftArm")]
        [InlineData("Single fire", "SingleFire")]
        [InlineData("Full Auto", "FullAuto")]
        [InlineData("Ultra high molecular weight polyethylene", "UltraHighMolecularWeightPolyethylene")]
        [InlineData("high", "High")]
        public void ToPascalCase_ShouldReturnStringInPascalCase(string stringToFormat, string expected)
        {
            // Act
            string formattedString = StringExtensions.ToPascalCase(stringToFormat);

            // Asssert
            formattedString.Should().Be(expected);
        }
    }
}
