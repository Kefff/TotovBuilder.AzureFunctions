using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="ApiRequester"/> class.
    /// </summary>
    public class ApiRequesterTest
    {
        private const string DataJson = @"{
  ""data"": {
    ""items"": {
      ""5447a9cd4bdc2dbd208b4567"": {
        ""id"": ""5447a9cd4bdc2dbd208b4567""
      }
    }
  }
}";

        private const string TranslationsJson = @"{
  ""data"": {
    ""5447a9cd4bdc2dbd208b4567 Name"": ""Colt M4A1 5.56x45 assault rifle"",
    ""MOD_PISTOL_GRIP"": ""Pistol grip""
  }
}";

        [Fact]
        public async Task RequestData_ShouldRequestData()
        {
            // Arrange
            HttpRequestMessage? sentRequest = null;
            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Callback((HttpRequestMessage request) => sentRequest = request)
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(DataJson) }))
                .Verifiable();

            ApiRequester requester = CreateRequester(httpClientWrapperMock.Object, TestData.AzureFunctionsConfiguration);

            // Act
            Result<JsonElement> result = await requester.RequestData("/regular/items");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.GetProperty("items").GetProperty("5447a9cd4bdc2dbd208b4567").GetProperty("id").GetString().Should().Be("5447a9cd4bdc2dbd208b4567");
            sentRequest!.Method.Should().Be(HttpMethod.Get);
            sentRequest.RequestUri.Should().Be(new Uri("https://localhost/regular/items"));
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
        }

        [Fact]
        public async Task RequestData_WithConcurrentRequestsOfTheSameFile_ShouldRequestTheFileOnce()
        {
            // Arrange
            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(500);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(DataJson) };
                })
                .Verifiable();

            ApiRequester requester = CreateRequester(httpClientWrapperMock.Object, TestData.AzureFunctionsConfiguration);

            // Act
            Result<JsonElement>[] results = await Task.WhenAll(
                requester.RequestData("/regular/items"),
                requester.RequestData("/regular/items"));

            // Assert
            results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
            results[1].Value.Should().Be(results[0].Value);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
        }

        [Fact]
        public async Task RequestData_WithAlreadyRequestedFile_ShouldNotRequestTheFileAgain()
        {
            // Arrange
            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(DataJson) }))
                .Verifiable();

            ApiRequester requester = CreateRequester(httpClientWrapperMock.Object, TestData.AzureFunctionsConfiguration);

            // Act
            await requester.RequestData("/regular/items");
            await requester.RequestData("/pve/items");
            Result<JsonElement> result = await requester.RequestData("/regular/items");

            // Assert
            result.IsSuccess.Should().BeTrue();
            httpClientWrapperMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath == "/regular/items")), Times.Once);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath == "/pve/items")), Times.Once);
        }

        [Fact]
        public async Task RequestData_AfterAFailure_ShouldRequestTheFileAgain()
        {
            // Arrange
            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .SetupSequence(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .ThrowsAsync(new HttpRequestException())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(DataJson) });

            ApiRequester requester = CreateRequester(httpClientWrapperMock.Object, TestData.AzureFunctionsConfiguration);

            // Act
            Result<JsonElement> firstResult = await requester.RequestData("/regular/items");
            Result<JsonElement> secondResult = await requester.RequestData("/regular/items");

            // Assert
            firstResult.IsSuccess.Should().BeFalse();
            secondResult.IsSuccess.Should().BeTrue();
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Exactly(2));
        }

        [Fact]
        public async Task RequestData_WithInvalidConfiguration_ShouldFail()
        {
            // Arrange
            Mock<IHttpClientWrapper> httpClientWrapperMock = new();

            ApiRequester requester = CreateRequester(httpClientWrapperMock.Object, new AzureFunctionsConfiguration());

            // Act
            Result<JsonElement> result = await requester.RequestData("/regular/items");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Invalid configuration.");
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Never);
        }

        [Fact]
        public async Task RequestData_WithTimeout_ShouldFail()
        {
            // Arrange
            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(2000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(DataJson) };
                })
                .Verifiable();

            ApiRequester requester = CreateRequester(
                httpClientWrapperMock.Object,
                new AzureFunctionsConfiguration()
                {
                    ApiBaseUrl = "https://localhost",
                    ExecutionTimeout = 1
                });

            // Act
            Result<JsonElement> result = await requester.RequestData("/regular/items");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("/regular/items - Execution delay exceeded.");
        }

        [Fact]
        public async Task RequestData_WithError_ShouldFail()
        {
            // Arrange
            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Throws<Exception>();

            ApiRequester requester = CreateRequester(httpClientWrapperMock.Object, TestData.AzureFunctionsConfiguration);

            // Act
            Result<JsonElement> result = await requester.RequestData("/regular/items");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().StartWith("/regular/items - Error while requesting the API");
        }

        [Fact]
        public async Task RequestData_WithErrorStatusCode_ShouldFail()
        {
            // Arrange
            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("{\"error\":\"Not found\"}") }));

            ApiRequester requester = CreateRequester(httpClientWrapperMock.Object, TestData.AzureFunctionsConfiguration);

            // Act
            Result<JsonElement> result = await requester.RequestData("/regular/items");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().StartWith("/regular/items - Error while requesting the API");
        }

        [Theory]
        [InlineData("{ \"data\": {} }")]
        [InlineData("{ \"data\": [] }")]
        [InlineData("{ \"data\": \"\" }")]
        [InlineData("{ \"data\": null }")]
        [InlineData("{ \"invalid\": { \"items\": {} } }")]
        [InlineData("[]")]
        [InlineData("{ data\": {} }")]
        public async Task RequestData_WithInvalidData_ShouldFail(string responseContent)
        {
            // Arrange
            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(responseContent) }));

            ApiRequester requester = CreateRequester(httpClientWrapperMock.Object, TestData.AzureFunctionsConfiguration);

            // Act
            Result<JsonElement> result = await requester.RequestData("/regular/items");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().StartWith("/regular/items - Invalid API response data");
        }

        [Fact]
        public async Task RequestTranslations_ShouldRequestTranslations()
        {
            // Arrange
            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TranslationsJson) }))
                .Verifiable();

            ApiRequester requester = CreateRequester(httpClientWrapperMock.Object, TestData.AzureFunctionsConfiguration);

            // Act
            Result<Translations> firstResult = await requester.RequestTranslations("/regular/items_en");
            Result<Translations> secondResult = await requester.RequestTranslations("/regular/items_en");

            // Assert
            firstResult.IsSuccess.Should().BeTrue();
            firstResult.Value.Translate("5447a9cd4bdc2dbd208b4567 Name").Should().Be("Colt M4A1 5.56x45 assault rifle");
            firstResult.Value.Translate("MOD_PISTOL_GRIP").Should().Be("Pistol grip");
            secondResult.Value.Should().BeSameAs(firstResult.Value);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
        }

        [Fact]
        public async Task RequestTranslations_WithError_ShouldFail()
        {
            // Arrange
            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Throws<Exception>();

            ApiRequester requester = CreateRequester(httpClientWrapperMock.Object, TestData.AzureFunctionsConfiguration);

            // Act
            Result<Translations> result = await requester.RequestTranslations("/regular/items_en");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().StartWith("/regular/items_en - Error while requesting the API");
        }

        private static ApiRequester CreateRequester(IHttpClientWrapper httpClientWrapper, AzureFunctionsConfiguration configuration)
        {
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(configuration);

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapper);

            return new ApiRequester(
                new Mock<ILogger<ApiRequester>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object);
        }
    }
}
