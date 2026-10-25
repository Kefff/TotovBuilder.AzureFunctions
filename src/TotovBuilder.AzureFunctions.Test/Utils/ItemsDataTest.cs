using System.Linq;
using System.Text.Json;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Utils;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Utils
{
    /// <summary>
    /// Represents tests on the <see cref="ItemsData"/> class.
    /// </summary>
    public class ItemsDataTest
    {
        [Fact]
        public void GetItems_ShouldGetItems()
        {
            // Arrange
            JsonElement itemsDataJson = JsonDocument.Parse("{ \"items\": { \"item-1\": { \"id\": \"item-1\" } } }").RootElement;

            // Act
            Result<JsonElement> result = ItemsData.GetItems(itemsDataJson, "/regular/items", new Mock<ILogger>().Object);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.GetProperty("item-1").GetProperty("id").GetString().Should().Be("item-1");
        }

        [Theory]
        [InlineData("{ \"items\": {} }")]
        [InlineData("{ \"items\": [] }")]
        [InlineData("{ \"invalid\": {} }")]
        [InlineData("[]")]
        public void GetItems_WithoutItems_ShouldFail(string itemsDataJson)
        {
            // Act
            Result<JsonElement> result = ItemsData.GetItems(JsonDocument.Parse(itemsDataJson).RootElement, "/regular/items", new Mock<ILogger>().Object);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().StartWith("/regular/items - Invalid API response data");
        }

        [Theory]
        [InlineData("{ \"properties\": { \"propertiesType\": \"ItemPropertiesPreset\" } }", true)]
        [InlineData("{ \"properties\": { \"propertiesType\": \"ItemPropertiesWeapon\" } }", false)]
        [InlineData("{ \"properties\": null }", false)]
        [InlineData("{}", false)]
        public void IsPreset_ShouldIndicateWhetherItemIsPreset(string itemJson, bool expected)
        {
            // Act
            bool result = ItemsData.IsPreset(JsonDocument.Parse(itemJson).RootElement);

            // Assert
            result.Should().Be(expected);
        }
    }
}
