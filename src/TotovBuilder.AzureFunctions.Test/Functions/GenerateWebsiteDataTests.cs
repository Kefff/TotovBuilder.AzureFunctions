using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Utils;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.Model.Builds;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GenerateWebsiteData"/> class.
    /// </summary>
    public class GenerateWebsiteDataTests
    {
        [Fact]
        public async Task Run_ShouldGenerateWebsiteData()
        {
            // Arrange
            ScheduleTrigger scheduleTrigger = new ScheduleTrigger()
            {
                IsPastDue = false,
                ScheduleStatus = new ScheduleStatus()
                {
                    Last = new DateTime(2023, 12, 1, 8, 0, 0),
                    LastUpdated = new DateTime(2023, 12, 1, 8, 30, 0),
                    Next = new DateTime(2023, 12, 2, 8, 0, 0)
                }
            };

            Mock<IConfigurationLoader> configurationLoaderMock = new Mock<IConfigurationLoader>();
            configurationLoaderMock.Setup(m => m.Load()).Verifiable();

            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(new AzureFunctionsConfiguration()
                {
                    AzureBlobStorageWebsiteDataContainerName = "$web",
                    WebsiteChangelogBlobName = "data/changelog.json",
                    WebsiteItemCategoriesBlobName = "data/item-categories.json",
                    WebsiteItemsBlobName = "data/items.json",
                    WebsitePresetsBlobName = "data/presets.json",
                    WebsitePricesBlobName = "data/prices.json",
                    WebsiteTarkovValuesBlobName = "data/tarkov-values.json",
                    WebsiteWebsiteConfigurationBlobName = "data/website-configuration.json"
                })
                .Verifiable();

            Mock<IChangelogFetcher> changelogFetcherMock = new Mock<IChangelogFetcher>();
            changelogFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok<IEnumerable<ChangelogEntry>>(TestData.Changelog)))
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok<IEnumerable<ItemCategory>>(TestData.ItemCategories)))
                .Verifiable();

            Mock<IItemsFetcher> itemsFetcherMock = new Mock<IItemsFetcher>();
            itemsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok<IEnumerable<Item>>(TestData.Items)))
                .Verifiable();

            Mock<IPresetsFetcher> presetsFetcherMock = new Mock<IPresetsFetcher>();
            presetsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok<IEnumerable<InventoryItem>>(TestData.Presets)))
                .Verifiable();

            Mock<IPricesFetcher> pricesFetcherMock = new Mock<IPricesFetcher>();
            pricesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok<IEnumerable<Price>>(TestData.Prices)))
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new Mock<ITarkovValuesFetcher>();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok(TestData.TarkovValues)))
                .Verifiable();

            Mock<IWebsiteConfigurationFetcher> websiteConfigurationFetcherMock = new Mock<IWebsiteConfigurationFetcher>();
            websiteConfigurationFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok(TestData.WebsiteConfiguration)))
                .Verifiable();

            JsonSerializerOptions serializationOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            string expectedChangelog = JsonSerializer.Serialize(TestData.Changelog, serializationOptions);
            string expectedItemCategories = JsonSerializer.Serialize(TestData.ItemCategories, serializationOptions);
            string expectedItems = JsonSerializer.Serialize(TestData.Items, serializationOptions);
            string expectedPresets = JsonSerializer.Serialize(TestData.Presets, serializationOptions);
            string expectedPrices = JsonSerializer.Serialize(TestData.Prices, serializationOptions);
            string expectedTarkovValues = JsonSerializer.Serialize(TestData.TarkovValues, serializationOptions);
            string expectedWebsiteConfiguration = JsonSerializer.Serialize(TestData.WebsiteConfiguration, serializationOptions);

            Mock<IAzureBlobManager> azureBlobManagerMock = new Mock<IAzureBlobManager>();
            azureBlobManagerMock
                .Setup(m => m.Update("data/changelog.json", expectedChangelog))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("data/item-categories.json", expectedItemCategories))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("data/items.json", expectedItems))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("data/presets.json", expectedPresets))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("data/prices.json", expectedPrices))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("data/tarkov-values.json", expectedTarkovValues))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("data/website-configuration.json", expectedWebsiteConfiguration))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();

            GenerateWebsiteData function = new GenerateWebsiteData(
                configurationLoaderMock.Object,
                configurationWrapperMock.Object,
                azureBlobManagerMock.Object,
                changelogFetcherMock.Object,
                itemCategoriesFetcherMock.Object,
                itemsFetcherMock.Object,
                new Mock<ILogger<GenerateWebsiteData>>().Object,
                presetsFetcherMock.Object,
                pricesFetcherMock.Object,
                tarkovValuesFetcherMock.Object,
                websiteConfigurationFetcherMock.Object);

            // Act
            await function.Run(scheduleTrigger);

            // Assert
            azureBlobManagerMock.Verify();
            changelogFetcherMock.Verify();
            configurationLoaderMock.Verify();
            configurationWrapperMock.Verify();
            itemCategoriesFetcherMock.Verify();
            itemsFetcherMock.Verify();
            presetsFetcherMock.Verify();
            pricesFetcherMock.Verify();
            tarkovValuesFetcherMock.Verify();
            websiteConfigurationFetcherMock.Verify();
        }

        [Fact]
        public async Task Run_WithFailedFetch_ShouldDoNothing()
        {
            // Arrange
            ScheduleTrigger scheduleTrigger = new ScheduleTrigger()
            {
                IsPastDue = false,
                ScheduleStatus = new ScheduleStatus()
                {
                    Last = new DateTime(2023, 12, 1, 8, 0, 0),
                    LastUpdated = new DateTime(2023, 12, 1, 8, 30, 0),
                    Next = new DateTime(2023, 12, 2, 8, 0, 0)
                }
            };

            Mock<IConfigurationLoader> configurationLoaderMock = new Mock<IConfigurationLoader>();
            configurationLoaderMock.Setup(m => m.Load()).Verifiable();

            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(new AzureFunctionsConfiguration()
                {
                    AzureBlobStorageWebsiteDataContainerName = "$web",
                    WebsiteChangelogBlobName = "data/changelog.json",
                    WebsiteItemCategoriesBlobName = "data/item-categories.json",
                    WebsiteItemsBlobName = "data/items.json",
                    WebsitePresetsBlobName = "data/presets.json",
                    WebsitePricesBlobName = "data/prices.json",
                    WebsiteTarkovValuesBlobName = "data/tarkov-values.json",
                    WebsiteWebsiteConfigurationBlobName = "data/website-configuration.json"
                })
                .Verifiable();

            Mock<IChangelogFetcher> changelogFetcherMock = new Mock<IChangelogFetcher>();
            changelogFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail<IEnumerable<ChangelogEntry>>("Error")))
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail<IEnumerable<ItemCategory>>("Error")))
                .Verifiable();

            Mock<IItemsFetcher> itemsFetcherMock = new Mock<IItemsFetcher>();
            itemsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail<IEnumerable<Item>>("Error")))
                .Verifiable();

            Mock<IPresetsFetcher> presetsFetcherMock = new Mock<IPresetsFetcher>();
            presetsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail<IEnumerable<InventoryItem>>("Error")))
                .Verifiable();

            Mock<IPricesFetcher> pricesFetcherMock = new Mock<IPricesFetcher>();
            pricesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail<IEnumerable<Price>>("Error")))
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new Mock<ITarkovValuesFetcher>();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail<TarkovValues>("Error")))
                .Verifiable();

            Mock<IWebsiteConfigurationFetcher> websiteConfigurationFetcherMock = new Mock<IWebsiteConfigurationFetcher>();
            websiteConfigurationFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail<WebsiteConfiguration>("Error")))
                .Verifiable();

            Mock<IAzureBlobManager> azureBlobManagerMock = new Mock<IAzureBlobManager>();

            GenerateWebsiteData function = new GenerateWebsiteData(
                configurationLoaderMock.Object,
                configurationWrapperMock.Object,
                azureBlobManagerMock.Object,
                changelogFetcherMock.Object,
                itemCategoriesFetcherMock.Object,
                itemsFetcherMock.Object,
                new Mock<ILogger<GenerateWebsiteData>>().Object,
                presetsFetcherMock.Object,
                pricesFetcherMock.Object,
                tarkovValuesFetcherMock.Object,
                websiteConfigurationFetcherMock.Object);

            // Act
            await function.Run(scheduleTrigger);

            // Assert
            azureBlobManagerMock.Verify(m => m.Update(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            changelogFetcherMock.Verify();
            configurationLoaderMock.Verify();
            configurationWrapperMock.Verify();
            itemCategoriesFetcherMock.Verify();
            itemsFetcherMock.Verify();
            presetsFetcherMock.Verify();
            pricesFetcherMock.Verify();
            tarkovValuesFetcherMock.Verify();
            websiteConfigurationFetcherMock.Verify();
        }

        [Fact]
        public async Task Run_WithFailedUpdate_ShouldDoNothing()
        {
            // Arrange
            ScheduleTrigger scheduleTrigger = new ScheduleTrigger()
            {
                IsPastDue = false,
                ScheduleStatus = new ScheduleStatus()
                {
                    Last = new DateTime(2023, 12, 1, 8, 0, 0),
                    LastUpdated = new DateTime(2023, 12, 1, 8, 30, 0),
                    Next = new DateTime(2023, 12, 2, 8, 0, 0)
                }
            };

            Mock<IConfigurationLoader> configurationLoaderMock = new Mock<IConfigurationLoader>();
            configurationLoaderMock.Setup(m => m.Load()).Verifiable();

            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(new AzureFunctionsConfiguration()
                {
                    AzureBlobStorageWebsiteDataContainerName = "$web",
                    WebsiteChangelogBlobName = "data/changelog.json",
                    WebsiteItemCategoriesBlobName = "data/item-categories.json",
                    WebsiteItemsBlobName = "data/items.json",
                    WebsitePresetsBlobName = "data/presets.json",
                    WebsitePricesBlobName = "data/prices.json",
                    WebsiteTarkovValuesBlobName = "data/tarkov-values.json",
                    WebsiteWebsiteConfigurationBlobName = "data/website-configuration.json"
                })
                .Verifiable();

            Mock<IChangelogFetcher> changelogFetcherMock = new Mock<IChangelogFetcher>();
            changelogFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok<IEnumerable<ChangelogEntry>>(TestData.Changelog)))
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok<IEnumerable<ItemCategory>>(TestData.ItemCategories)))
                .Verifiable();

            Mock<IItemsFetcher> itemsFetcherMock = new Mock<IItemsFetcher>();
            itemsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok<IEnumerable<Item>>(TestData.Items)))
                .Verifiable();

            Mock<IPresetsFetcher> presetsFetcherMock = new Mock<IPresetsFetcher>();
            presetsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok<IEnumerable<InventoryItem>>(TestData.Presets)))
                .Verifiable();

            Mock<IPricesFetcher> pricesFetcherMock = new Mock<IPricesFetcher>();
            pricesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok<IEnumerable<Price>>(TestData.Prices)))
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new Mock<ITarkovValuesFetcher>();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok(TestData.TarkovValues)))
                .Verifiable();

            Mock<IWebsiteConfigurationFetcher> websiteConfigurationFetcherMock = new Mock<IWebsiteConfigurationFetcher>();
            websiteConfigurationFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok(TestData.WebsiteConfiguration)))
                .Verifiable();

            JsonSerializerOptions serializationOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            string expectedChangelog = JsonSerializer.Serialize(TestData.Changelog, serializationOptions);
            string expectedItemCategories = JsonSerializer.Serialize(TestData.ItemCategories, serializationOptions);
            string expectedItems = JsonSerializer.Serialize(TestData.Items, serializationOptions);
            string expectedPresets = JsonSerializer.Serialize(TestData.Presets, serializationOptions);
            string expectedPrices = JsonSerializer.Serialize(TestData.Prices, serializationOptions);
            string expectedTarkovValues = JsonSerializer.Serialize(TestData.TarkovValues, serializationOptions);
            string expectedWebsiteConfiguration = JsonSerializer.Serialize(TestData.WebsiteConfiguration, serializationOptions);

            Mock<IAzureBlobManager> azureBlobManagerMock = new Mock<IAzureBlobManager>();
            azureBlobManagerMock
                .Setup(m => m.Update("data/changelog.json", expectedChangelog))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("data/item-categories.json", expectedItemCategories))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("data/items.json", expectedItems))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("data/presets.json", expectedPresets))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("data/prices.json", expectedPrices))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("data/tarkov-values.json", expectedTarkovValues))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("data/website-configuration.json", expectedWebsiteConfiguration))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();

            GenerateWebsiteData function = new GenerateWebsiteData(
                configurationLoaderMock.Object,
                configurationWrapperMock.Object,
                azureBlobManagerMock.Object,
                changelogFetcherMock.Object,
                itemCategoriesFetcherMock.Object,
                itemsFetcherMock.Object,
                new Mock<ILogger<GenerateWebsiteData>>().Object,
                presetsFetcherMock.Object,
                pricesFetcherMock.Object,
                tarkovValuesFetcherMock.Object,
                websiteConfigurationFetcherMock.Object);

            // Act
            await function.Run(scheduleTrigger);

            // Assert
            azureBlobManagerMock.Verify();
            changelogFetcherMock.Verify();
            configurationLoaderMock.Verify();
            configurationWrapperMock.Verify();
            itemCategoriesFetcherMock.Verify();
            itemsFetcherMock.Verify();
            presetsFetcherMock.Verify();
            pricesFetcherMock.Verify();
            tarkovValuesFetcherMock.Verify();
            websiteConfigurationFetcherMock.Verify();
        }
    }
}
