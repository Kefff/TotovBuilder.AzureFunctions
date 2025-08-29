using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Abstractions.Items;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="LocalizedItemsFetcher"/> class.
    /// </summary>
    public class LocalizedItemsFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldFetchItemsInAllLanguages()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async (HttpRequestMessage hrm) =>
                {
                    string requestContent = await hrm.Content!.ReadAsStringAsync();
                    string responseContent = requestContent.Contains("lang: en") ? ItemsJsonEN : ItemsJsonFR;
                    HttpResponseMessage response = new(HttpStatusCode.OK)
                    {
                        Content = new StringContent(responseContent)
                    };

                    return response;
                })
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                    .Returns(httpClientWrapperMock.Object)
                    .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                    .Returns(Task.FromResult(Result.Ok()))
                    .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                    .Returns(TestData.ItemCategories)
                    .Verifiable();

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher
                .Setup(m => m.Fetch())
                    .Returns(Task.FromResult(Result.Ok()))
                    .Verifiable();
            itemMissingPropertiesFetcher
                .SetupGet(m => m.FetchedData)
                    .Returns(TestData.ItemMissingProperties)
                    .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                    .Returns(Task.FromResult(Result.Ok()))
                    .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                    .Returns(TestData.TarkovValues)
                    .Verifiable();

            LocalizedItemsFetcher localizedItemsFetcher = new(
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcher.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await localizedItemsFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            localizedItemsFetcher.FetchedData!.ElementAt(0).Language.Should().Be("en");
            localizedItemsFetcher.FetchedData!.ElementAt(0).Items.Should().BeEquivalentTo(ItemsEN);
            localizedItemsFetcher.FetchedData!.ElementAt(1).Language.Should().Be("fr");
            localizedItemsFetcher.FetchedData!.ElementAt(1).Items.Should().BeEquivalentTo(ItemsFR);

            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Fetch_WithAlreadyFetchedData_ShouldDoNothing()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async (HttpRequestMessage hrm) =>
                {
                    string requestContent = await hrm.Content!.ReadAsStringAsync();
                    string responseContent = requestContent.Contains("lang: en") ? ItemsJsonEN : ItemsJsonFR;
                    HttpResponseMessage response = new(HttpStatusCode.OK)
                    {
                        Content = new StringContent(responseContent)
                    };

                    return response;
                })
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
                .Verifiable();

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemMissingPropertiesFetcher
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemMissingProperties)
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            LocalizedItemsFetcher localizedItemsFetcher = new(
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcher.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            await localizedItemsFetcher.Fetch();
            Result result = await localizedItemsFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            localizedItemsFetcher.FetchedData!.ElementAt(0).Language.Should().Be("en");
            localizedItemsFetcher.FetchedData!.ElementAt(0).Items.Should().BeEquivalentTo(ItemsEN);
            localizedItemsFetcher.FetchedData!.ElementAt(1).Language.Should().Be("fr");
            localizedItemsFetcher.FetchedData!.ElementAt(1).Items.Should().BeEquivalentTo(ItemsFR);

            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Fetch_WithNoDataFetched_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(@$"{{
  ""data"": {{
    ""items"": []
  }}
}}")
                }))
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
                .Verifiable();

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemMissingPropertiesFetcher
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemMissingProperties)
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            LocalizedItemsFetcher localizedItemsFetcher = new(
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcher.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await localizedItemsFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Items - No data fetched.");
            localizedItemsFetcher.FetchedData.Should().BeNull();
        }

        private static readonly IItem[] ItemsEN = [
            new Item()
            {
                CategoryId = "faceCover",
                IconLink = "https://assets.tarkov.dev/5e54f76986f7740366043752-icon.webp",
                Id = "5e54f76986f7740366043752",
                ImageLink = "https://assets.tarkov.dev/5e54f76986f7740366043752-image.webp",
                MarketLink = "https://tarkov.dev/item/shroud-half-mask",
                Name = "Shroud half-mask",
                ShortName = "Shroud",
                Weight = 0.1,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/Shroud_half-mask"
            }
        ];

        private const string ItemsJsonEN = @$"{{
  ""data"": {{
    ""items"": [
      {{
        ""categories"": [
          {{
            ""id"": ""5a341c4686f77469e155819e""
          }},
          {{
            ""id"": ""57bef4c42459772e8d35a53b""
          }},
          {{
            ""id"": ""543be5f84bdc2dd4348b456a""
          }},
          {{
            ""id"": ""566162e44bdc2d3f298b4573""
          }},
          {{
            ""id"": ""54009119af1c881c07000029""
          }}
        ],
        ""conflictingItems"": [],
        ""iconLink"": ""https://assets.tarkov.dev/5e54f76986f7740366043752-icon.webp"",
        ""id"": ""5e54f76986f7740366043752"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/5e54f76986f7740366043752-image.webp"",
        ""link"": ""https://tarkov.dev/item/shroud-half-mask"",
        ""name"": ""Shroud half-mask"",
        ""properties"": null,
        ""shortName"": ""Shroud"",
        ""weight"": 0.1,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Shroud_half-mask""
      }}
    ]
  }}
}}";

        private static readonly IItem[] ItemsFR = [
            new Item()
            {
                CategoryId = "faceCover",
                IconLink = "https://assets.tarkov.dev/5e54f76986f7740366043752-icon.webp",
                Id = "5e54f76986f7740366043752",
                ImageLink = "https://assets.tarkov.dev/5e54f76986f7740366043752-image.webp",
                MarketLink = "https://tarkov.dev/item/shroud-half-mask",
                Name = "Demi-masque Shroud",
                ShortName = "Shroud",
                Weight = 0.1,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/Shroud_half-mask"
            }
        ];

        private const string ItemsJsonFR = @$"{{
  ""data"": {{
    ""items"": [
      {{
        ""categories"": [
          {{
            ""id"": ""5a341c4686f77469e155819e""
          }},
          {{
            ""id"": ""57bef4c42459772e8d35a53b""
          }},
          {{
            ""id"": ""543be5f84bdc2dd4348b456a""
          }},
          {{
            ""id"": ""566162e44bdc2d3f298b4573""
          }},
          {{
            ""id"": ""54009119af1c881c07000029""
          }}
        ],
        ""conflictingItems"": [],
        ""iconLink"": ""https://assets.tarkov.dev/5e54f76986f7740366043752-icon.webp"",
        ""id"": ""5e54f76986f7740366043752"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/5e54f76986f7740366043752-image.webp"",
        ""link"": ""https://tarkov.dev/item/shroud-half-mask"",
        ""name"": ""Demi-masque Shroud"",
        ""properties"": null,
        ""shortName"": ""Shroud"",
        ""weight"": 0.1,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Shroud_half-mask""
      }}
    ]
  }}
}}";
    }
}
