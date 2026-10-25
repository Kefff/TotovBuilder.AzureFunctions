using System.Linq;
using System.Text.Json;
using FluentAssertions;
using TotovBuilder.AzureFunctions.Utils;
using Xunit;
using static System.Text.Json.JsonElement;

namespace TotovBuilder.AzureFunctions.Test.Utils
{
    /// <summary>
    /// Represents tests on the <see cref="JsonDeserializer"/> class.
    /// </summary>
    public class JsonDeserializerTest
    {
        [Theory]
        [InlineData("{ \"value\": [\"123456789\"] }", true)]
        [InlineData("[\"123456789\"]", false)]
        public void TryDeserializeArray_ShouldTryToDeserializeArray(string json, bool expected)
        {
            // Arrange
            JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

            // Act
            bool result = JsonDeserializer.TryDeserializeArray(jsonElement, "value", out ArrayEnumerator arrayEnumerator);

            // Assert
            result.Should().Be(expected);

            if (result)
            {
                arrayEnumerator.First().GetString().Should().Be("123456789");
            }
        }

        [Theory]
        [InlineData("{ \"value\": true }", true)]
        [InlineData("{ \"value\": \"123456789\"}", false)]
        [InlineData("{ \"invalid\": true }", false)]
        [InlineData("[123456789]", false)]
        public void TryDeserializeBoolean_ShouldTryToDeserializeBoolean(string json, bool expected)
        {
            // Arrange
            JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

            // Act
            bool result = JsonDeserializer.TryDeserializeBoolean(jsonElement, "value", out bool resultValue);

            // Assert
            result.Should().Be(expected);

            if (result)
            {
                resultValue.Should().Be(true);
            }
        }

        [Theory]
        [InlineData("{ \"value\": 123456789 }", true)]
        [InlineData("{ \"value\": \"123456789\"}", false)]
        [InlineData("{ \"invalid\": 123456789 }", false)]
        [InlineData("[123456789]", false)]
        public void TryDeserializeDouble_ShouldTryToDeserializeDouble(string json, bool expected)
        {
            // Arrange
            JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

            // Act
            bool result = JsonDeserializer.TryDeserializeDouble(jsonElement, "value", out double resultValue);

            // Assert
            result.Should().Be(expected);

            if (result)
            {
                resultValue.Should().Be(123456789);
            }
        }

        [Theory]
        [InlineData("{ \"value\": { \"id\": \"123456789\" } }", true)]
        [InlineData("{ \"value\": \"123456789\" }", false)]
        [InlineData("{ \"invalid\": { \"id\": \"123456789\" } }", false)]
        [InlineData("[{ \"id\": \"123456789\" }]", false)]
        public void TryDeserializeObject_ShouldTryToDeserializeObject(string json, bool expected)
        {
            // Arrange
            JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

            // Act
            bool result = JsonDeserializer.TryDeserializeObject(jsonElement, "value", out JsonElement resultValue);

            // Assert
            result.Should().Be(expected);

            if (result)
            {
                resultValue.GetProperty("id").GetString().Should().Be("123456789");
            }
        }

        [Theory]
        [InlineData("{ \"value\": \"123456789\" }", true)]
        [InlineData("{ \"value\": 123456789 }", false)]
        [InlineData("{ \"invalid\": \"123456789\" }", false)]
        [InlineData("[\"123456789\"]", false)]
        public void TryDeserializeString_ShouldTryToDeserializeString(string json, bool expected)
        {
            // Arrange
            JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

            // Act
            bool result = JsonDeserializer.TryDeserializeString(jsonElement, "value", out string resultValue);

            // Assert
            result.Should().Be(expected);

            if (result)
            {
                resultValue.Should().Be("123456789");
            }
        }
    }
}
