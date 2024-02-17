using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Utils;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.Model.Builds;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using TotovBuilder.Shared.Abstractions.Azure;
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
            configurationLoaderMock.Setup(m => m.WaitForLoading()).Returns(Task.FromResult(Result.Ok())).Verifiable();

            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(new AzureFunctionsConfiguration()
                {
                    AzureBlobStorageWebsiteContainerName = "$web",
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
            string expectedChangelog = JsonSerializer.Serialize(TestData.Changelog as IEnumerable<object>, serializationOptions);
            string expectedItemCategories = JsonSerializer.Serialize(TestData.ItemCategories.Select(ic => ic.Id) as IEnumerable<object>, serializationOptions);
            string expectedItems = JsonSerializer.Serialize(TestData.Items as IEnumerable<object>, serializationOptions);
            string expectedPresets = JsonSerializer.Serialize(TestData.Presets as IEnumerable<object>, serializationOptions);
            string expectedPrices = JsonSerializer.Serialize(TestData.Prices as IEnumerable<object>, serializationOptions);
            string expectedTarkovValues = JsonSerializer.Serialize(TestData.TarkovValues as object, serializationOptions);
            string expectedWebsiteConfiguration = JsonSerializer.Serialize(TestData.WebsiteConfiguration as object, serializationOptions);

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new Mock<IAzureBlobStorageManager>();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/changelog.json", expectedChangelog, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/item-categories.json", expectedItemCategories, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/items.json", expectedItems, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/presets.json", expectedPresets, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/prices.json", expectedPrices, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/tarkov-values.json", expectedTarkovValues, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/website-configuration.json", expectedWebsiteConfiguration, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();

            GenerateWebsiteData function = new GenerateWebsiteData(
                new Mock<ILogger<GenerateWebsiteData>>().Object,
                configurationLoaderMock.Object,
                configurationWrapperMock.Object,
                azureBlobStorageManagerMock.Object,
                changelogFetcherMock.Object,
                itemCategoriesFetcherMock.Object,
                itemsFetcherMock.Object,
                presetsFetcherMock.Object,
                pricesFetcherMock.Object,
                tarkovValuesFetcherMock.Object,
                websiteConfigurationFetcherMock.Object);

            // Act
            await function.Run(scheduleTrigger);

            // Assert
            azureBlobStorageManagerMock.Verify();
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
        public async Task Run_WithFailedConfigurationLoading_ShouldDoNothing()
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
            configurationLoaderMock.Setup(m => m.WaitForLoading()).Returns(Task.FromResult(Result.Fail("Error"))).Verifiable();

            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            Mock<IChangelogFetcher> changelogFetcherMock = new Mock<IChangelogFetcher>();
            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            Mock<IItemsFetcher> itemsFetcherMock = new Mock<IItemsFetcher>();
            Mock<IPresetsFetcher> presetsFetcherMock = new Mock<IPresetsFetcher>();
            Mock<IPricesFetcher> pricesFetcherMock = new Mock<IPricesFetcher>();
            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new Mock<ITarkovValuesFetcher>();
            Mock<IWebsiteConfigurationFetcher> websiteConfigurationFetcherMock = new Mock<IWebsiteConfigurationFetcher>();
            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new Mock<IAzureBlobStorageManager>();

            GenerateWebsiteData function = new GenerateWebsiteData(
                new Mock<ILogger<GenerateWebsiteData>>().Object,
                configurationLoaderMock.Object,
                configurationWrapperMock.Object,
                azureBlobStorageManagerMock.Object,
                changelogFetcherMock.Object,
                itemCategoriesFetcherMock.Object,
                itemsFetcherMock.Object,
                presetsFetcherMock.Object,
                pricesFetcherMock.Object,
                tarkovValuesFetcherMock.Object,
                websiteConfigurationFetcherMock.Object);

            // Act
            await function.Run(scheduleTrigger);

            // Assert
            azureBlobStorageManagerMock.Verify(m => m.UpdateBlob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BlobHttpHeaders>()), Times.Never);
            configurationLoaderMock.Verify();
            configurationWrapperMock.Verify(m => m.Values, Times.Never);
            changelogFetcherMock.Verify(m => m.Fetch(), Times.Never);
            itemCategoriesFetcherMock.Verify(m => m.Fetch(), Times.Never);
            itemsFetcherMock.Verify(m => m.Fetch(), Times.Never);
            presetsFetcherMock.Verify(m => m.Fetch(), Times.Never);
            pricesFetcherMock.Verify(m => m.Fetch(), Times.Never);
            tarkovValuesFetcherMock.Verify(m => m.Fetch(), Times.Never);
            websiteConfigurationFetcherMock.Verify(m => m.Fetch(), Times.Never);

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
            configurationLoaderMock.Setup(m => m.WaitForLoading()).Returns(Task.FromResult(Result.Ok())).Verifiable();

            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(new AzureFunctionsConfiguration()
                {
                    AzureBlobStorageWebsiteContainerName = "$web",
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

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new Mock<IAzureBlobStorageManager>();

            GenerateWebsiteData function = new GenerateWebsiteData(
                new Mock<ILogger<GenerateWebsiteData>>().Object,
                configurationLoaderMock.Object,
                configurationWrapperMock.Object,
                azureBlobStorageManagerMock.Object,
                changelogFetcherMock.Object,
                itemCategoriesFetcherMock.Object,
                itemsFetcherMock.Object,
                presetsFetcherMock.Object,
                pricesFetcherMock.Object,
                tarkovValuesFetcherMock.Object,
                websiteConfigurationFetcherMock.Object);

            // Act
            await function.Run(scheduleTrigger);

            // Assert
            azureBlobStorageManagerMock.Verify(m => m.UpdateBlob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BlobHttpHeaders>()), Times.Never);
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
            configurationLoaderMock.Setup(m => m.WaitForLoading()).Returns(Task.FromResult(Result.Ok())).Verifiable();

            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(new AzureFunctionsConfiguration()
                {
                    AzureBlobStorageWebsiteContainerName = "$web",
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
            string expectedChangelog = JsonSerializer.Serialize(TestData.Changelog as IEnumerable<object>, serializationOptions);
            string expectedItemCategories = JsonSerializer.Serialize(TestData.ItemCategories.Select(ic => ic.Id) as IEnumerable<object>, serializationOptions);
            string expectedItems = JsonSerializer.Serialize(TestData.Items as IEnumerable<object>, serializationOptions);
            string expectedPresets = JsonSerializer.Serialize(TestData.Presets as IEnumerable<object>, serializationOptions);
            string expectedPrices = JsonSerializer.Serialize(TestData.Prices as IEnumerable<object>, serializationOptions);
            string expectedTarkovValues = JsonSerializer.Serialize(TestData.TarkovValues as object, serializationOptions);
            string expectedWebsiteConfiguration = JsonSerializer.Serialize(TestData.WebsiteConfiguration as object, serializationOptions);

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new Mock<IAzureBlobStorageManager>();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/changelog.json", expectedChangelog, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/item-categories.json", expectedItemCategories, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/items.json", expectedItems, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/presets.json", expectedPresets, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/prices.json", expectedPrices, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/tarkov-values.json", expectedTarkovValues, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/website-configuration.json", expectedWebsiteConfiguration, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();

            GenerateWebsiteData function = new GenerateWebsiteData(
                new Mock<ILogger<GenerateWebsiteData>>().Object,
                configurationLoaderMock.Object,
                configurationWrapperMock.Object,
                azureBlobStorageManagerMock.Object,
                changelogFetcherMock.Object,
                itemCategoriesFetcherMock.Object,
                itemsFetcherMock.Object,
                presetsFetcherMock.Object,
                pricesFetcherMock.Object,
                tarkovValuesFetcherMock.Object,
                websiteConfigurationFetcherMock.Object);

            // Act
            await function.Run(scheduleTrigger);

            // Assert
            azureBlobStorageManagerMock.Verify();
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
