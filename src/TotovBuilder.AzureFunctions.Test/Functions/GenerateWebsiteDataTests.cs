using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
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

            Mock<IAzureBlobManager> azureBlobManagerMock = new Mock<IAzureBlobManager>();
            azureBlobManagerMock
                .Setup(m => m.Update("$web", "data/changelog.json", TestData.Changelog))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("$web", "data/item-categories.json", TestData.ItemCategories))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("$web", "data/items.json", TestData.Items))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("$web", "data/presets.json", TestData.Presets))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("$web", "data/prices.json", TestData.Prices))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("$web", "data/tarkov-values.json", TestData.TarkovValues))
                .Verifiable();
            azureBlobManagerMock
                .Setup(m => m.Update("$web", "data/website-configuration.json", TestData.WebsiteConfiguration))
                .Verifiable();

            GenerateWebsiteData function = new GenerateWebsiteData(
                configurationLoaderMock.Object,
                configurationWrapperMock.Object,
                azureBlobManagerMock.Object,
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
            configurationLoaderMock.Verify();
            configurationWrapperMock.Verify();
            azureBlobManagerMock.Verify();
            changelogFetcherMock.Verify();
            itemCategoriesFetcherMock.Verify();
            itemsFetcherMock.Verify();
            presetsFetcherMock.Verify();
            pricesFetcherMock.Verify();
            tarkovValuesFetcherMock.Verify();
            websiteConfigurationFetcherMock.Verify();
        }

        [Fact]
        public async Task Run_WithFailedFetch_ShouldGenerateWebsiteData()
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
                presetsFetcherMock.Object,
                pricesFetcherMock.Object,
                tarkovValuesFetcherMock.Object,
                websiteConfigurationFetcherMock.Object);

            // Act
            await function.Run(scheduleTrigger);

            // Assert
            configurationLoaderMock.Verify();
            configurationWrapperMock.Verify();
            azureBlobManagerMock.Verify();
            changelogFetcherMock.Verify();
            itemCategoriesFetcherMock.Verify();
            itemsFetcherMock.Verify();
            presetsFetcherMock.Verify();
            pricesFetcherMock.Verify();
            tarkovValuesFetcherMock.Verify();
            websiteConfigurationFetcherMock.Verify();
        }
    }
}
