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
using TotovBuilder.Model.Test;
using TotovBuilder.Model.Utils;
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
            ScheduleTrigger scheduleTrigger = new()
            {
                IsPastDue = false,
                ScheduleStatus = new ScheduleStatus()
                {
                    Last = new DateTime(2023, 12, 1, 8, 0, 0),
                    LastUpdated = new DateTime(2023, 12, 1, 8, 30, 0),
                    Next = new DateTime(2023, 12, 2, 8, 0, 0)
                }
            };

            Mock<IConfigurationLoader> configurationLoaderMock = new();
            configurationLoaderMock
                .Setup(m => m.WaitForLoading())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();

            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IChangelogFetcher> changelogFetcherMock = new();
            changelogFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            changelogFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.Changelog)
                .Verifiable();

            Mock<IGameModeLocalizedPricesFetcher> gameModeLocalizedPricesFetcherMock = new();
            gameModeLocalizedPricesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            gameModeLocalizedPricesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns([
                        new GameModeLocalizedPrices()
                        {
                            GameMode = new GameMode()
                            {
                                ApiQueryValue = "regular",
                                Name = "pvp"
                            },
                            Language = "en",
                            Prices = [.. TestData.Prices, .. TestData.Barters]
                        },
                        new GameModeLocalizedPrices()
                        {
                            GameMode = new GameMode()
                            {
                                ApiQueryValue = "regular",
                                Name = "pvp"
                            },
                            Language = "fr",
                            Prices = [.. TestData.Prices, .. TestData.Barters]
                        },
                        new GameModeLocalizedPrices()
                        {
                            GameMode = new GameMode()
                            {
                                ApiQueryValue = "pve",
                                Name = "pve"
                            },
                            Language = "en",
                            Prices = [.. TestData.Prices, .. TestData.Barters]
                        },
                        new GameModeLocalizedPrices()
                        {
                            GameMode = new GameMode()
                            {
                                ApiQueryValue = "pve",
                                Name = "pve"
                            },
                            Language = "fr",
                            Prices = [.. TestData.Prices, .. TestData.Barters]
                        }
                    ])
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();

            Mock<ILocalizedItemsFetcher> localizedItemsFetcherMock = new();
            localizedItemsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            localizedItemsFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(
                    [
                        new LocalizedItems()
                        {
                            Items = TestData.Items,
                            Language = "en"
                        },
                        new LocalizedItems()
                        {
                            Items = TestData.Items,
                            Language = "fr"
                        }
                    ])
                .Verifiable();

            Mock<IPresetsFetcher> presetsFetcherMock = new();
            presetsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            presetsFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.Presets)
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

            Mock<IWebsiteConfigurationFetcher> websiteConfigurationFetcherMock = new();
            websiteConfigurationFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            websiteConfigurationFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.WebsiteConfiguration)
                .Verifiable();

            JsonSerializerOptions serializationOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            string expectedChangelog = JsonSerializer.Serialize(TestData.Changelog as IEnumerable<object>, serializationOptions);
            string expectedItemCategories = JsonSerializer.Serialize(TestData.ItemCategories.Select(ic => ic.Id) as IEnumerable<object>, serializationOptions);
            string expectedItems = JsonSerializer.Serialize(TestData.Items as IEnumerable<object>, serializationOptions);
            string expectedPresets = JsonSerializer.Serialize(TestData.Presets as IEnumerable<object>, serializationOptions);
            string expectedPrices = JsonSerializer.Serialize(TestData.Prices.Concat(TestData.Barters) as IEnumerable<object>, serializationOptions);
            string expectedTarkovValues = JsonSerializer.Serialize(TestData.TarkovValues as object, serializationOptions);
            string expectedWebsiteConfiguration = JsonSerializer.Serialize(TestData.WebsiteConfiguration as object, serializationOptions);

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/changelog.json", expectedChangelog, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/items_en.json", expectedItems, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/items_fr.json", expectedItems, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/presets.json", expectedPresets, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/prices_pvp_en.json", expectedPrices, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/prices_pvp_fr.json", expectedPrices, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/prices_pve_en.json", expectedPrices, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/prices_pve_fr.json", expectedPrices, It.IsAny<BlobHttpHeaders>()))
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

            GenerateWebsiteData function = new(
                new Mock<ILogger<GenerateWebsiteData>>().Object,
                configurationLoaderMock.Object,
                configurationWrapperMock.Object,
                azureBlobStorageManagerMock.Object,
                changelogFetcherMock.Object,
                localizedItemsFetcherMock.Object,
                gameModeLocalizedPricesFetcherMock.Object,
                itemCategoriesFetcherMock.Object,
                presetsFetcherMock.Object,
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
            localizedItemsFetcherMock.Verify();
            gameModeLocalizedPricesFetcherMock.Verify();
            presetsFetcherMock.Verify();
            tarkovValuesFetcherMock.Verify();
            websiteConfigurationFetcherMock.Verify();
        }

        [Fact]
        public async Task Run_WithFailedConfigurationLoading_ShouldDoNothing()
        {
            // Arrange
            ScheduleTrigger scheduleTrigger = new()
            {
                IsPastDue = false,
                ScheduleStatus = new ScheduleStatus()
                {
                    Last = new DateTime(2023, 12, 1, 8, 0, 0),
                    LastUpdated = new DateTime(2023, 12, 1, 8, 30, 0),
                    Next = new DateTime(2023, 12, 2, 8, 0, 0)
                }
            };

            Mock<IConfigurationLoader> configurationLoaderMock = new();
            configurationLoaderMock
                .Setup(m => m.WaitForLoading())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new();
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            Mock<IChangelogFetcher> changelogFetcherMock = new();
            Mock<IGameModeLocalizedPricesFetcher> gameModeLocalizedPricesFetcherMock = new();
            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            Mock<ILocalizedItemsFetcher> gameModeLocalizedItemsFetcherMock = new();
            Mock<IPresetsFetcher> presetsFetcherMock = new();
            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            Mock<IWebsiteConfigurationFetcher> websiteConfigurationFetcherMock = new();

            GenerateWebsiteData function = new(
                new Mock<ILogger<GenerateWebsiteData>>().Object,
                configurationLoaderMock.Object,
                configurationWrapperMock.Object,
                azureBlobStorageManagerMock.Object,
                changelogFetcherMock.Object,
                gameModeLocalizedItemsFetcherMock.Object,
                gameModeLocalizedPricesFetcherMock.Object,
                itemCategoriesFetcherMock.Object,
                presetsFetcherMock.Object,
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
            gameModeLocalizedItemsFetcherMock.Verify(m => m.Fetch(), Times.Never);
            presetsFetcherMock.Verify(m => m.Fetch(), Times.Never);
            gameModeLocalizedPricesFetcherMock.Verify(m => m.Fetch(), Times.Never);
            tarkovValuesFetcherMock.Verify(m => m.Fetch(), Times.Never);
            websiteConfigurationFetcherMock.Verify(m => m.Fetch(), Times.Never);

        }

        [Fact]
        public async Task Run_WithFailedFetch_ShouldDoNothing()
        {
            // Arrange
            ScheduleTrigger scheduleTrigger = new()
            {
                IsPastDue = false,
                ScheduleStatus = new ScheduleStatus()
                {
                    Last = new DateTime(2023, 12, 1, 8, 0, 0),
                    LastUpdated = new DateTime(2023, 12, 1, 8, 30, 0),
                    Next = new DateTime(2023, 12, 2, 8, 0, 0)
                }
            };

            Mock<IConfigurationLoader> configurationLoaderMock = new();
            configurationLoaderMock
                .Setup(m => m.WaitForLoading())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();

            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IChangelogFetcher> changelogFetcherMock = new();
            changelogFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            changelogFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns((IEnumerable<ChangelogEntry>?)null)
                .Verifiable();

            Mock<ILocalizedItemsFetcher> localizedItemsFetcherMock = new();
            localizedItemsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            localizedItemsFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns((IEnumerable<LocalizedItems>?)null)
                .Verifiable();

            Mock<IGameModeLocalizedPricesFetcher> gameModeLocalizedPricesFetcherMock = new();
            gameModeLocalizedPricesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            gameModeLocalizedPricesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns((IEnumerable<GameModeLocalizedPrices>?)null)
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();

            Mock<IPresetsFetcher> presetsFetcherMock = new();
            presetsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            presetsFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns((IEnumerable<InventoryItem>?)null)
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns((TarkovValues?)null)
                .Verifiable();

            Mock<IWebsiteConfigurationFetcher> websiteConfigurationFetcherMock = new();
            websiteConfigurationFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            websiteConfigurationFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns((WebsiteConfiguration?)null)
                .Verifiable();

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new();

            GenerateWebsiteData function = new(
                new Mock<ILogger<GenerateWebsiteData>>().Object,
                configurationLoaderMock.Object,
                configurationWrapperMock.Object,
                azureBlobStorageManagerMock.Object,
                changelogFetcherMock.Object,
                localizedItemsFetcherMock.Object,
                gameModeLocalizedPricesFetcherMock.Object,
                itemCategoriesFetcherMock.Object,
                presetsFetcherMock.Object,
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
            localizedItemsFetcherMock.Verify();
            gameModeLocalizedPricesFetcherMock.Verify();
            presetsFetcherMock.Verify();
            tarkovValuesFetcherMock.Verify();
            websiteConfigurationFetcherMock.Verify();
        }

        [Fact]
        public async Task Run_WithFailedUpdate_ShouldDoNothing()
        {
            // Arrange
            ScheduleTrigger scheduleTrigger = new()
            {
                IsPastDue = false,
                ScheduleStatus = new ScheduleStatus()
                {
                    Last = new DateTime(2023, 12, 1, 8, 0, 0),
                    LastUpdated = new DateTime(2023, 12, 1, 8, 30, 0),
                    Next = new DateTime(2023, 12, 2, 8, 0, 0)
                }
            };

            Mock<IConfigurationLoader> configurationLoaderMock = new();
            configurationLoaderMock
                .Setup(m => m.WaitForLoading())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();

            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IChangelogFetcher> changelogFetcherMock = new();
            changelogFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            changelogFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.Changelog)
                .Verifiable();

            Mock<ILocalizedItemsFetcher> gameModeLocalizedItemsFetcherMock = new();
            gameModeLocalizedItemsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            gameModeLocalizedItemsFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(
                    [
                        new LocalizedItems()
                        {
                            Items = TestData.Items,
                            Language = "en"
                        },
                        new LocalizedItems()
                        {
                            Items = TestData.Items,
                            Language = "fr"
                        }
                    ])
                .Verifiable();

            Mock<IGameModeLocalizedPricesFetcher> gameModeLocalizedPricesFetcherMock = new();
            gameModeLocalizedPricesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            gameModeLocalizedPricesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns([
                        new GameModeLocalizedPrices()
                        {
                            GameMode = new GameMode()
                            {
                                ApiQueryValue = "regular",
                                Name = "pvp"
                            },
                            Language = "en",
                            Prices = [.. TestData.Prices, .. TestData.Barters]
                        },
                        new GameModeLocalizedPrices()
                        {
                            GameMode = new GameMode()
                            {
                                ApiQueryValue = "regular",
                                Name = "pvp"
                            },
                            Language = "fr",
                            Prices = [.. TestData.Prices, .. TestData.Barters]
                        },
                        new GameModeLocalizedPrices()
                        {
                            GameMode = new GameMode()
                            {
                                ApiQueryValue = "pve",
                                Name = "pve"
                            },
                            Language = "en",
                            Prices = [.. TestData.Prices, .. TestData.Barters]
                        },
                        new GameModeLocalizedPrices()
                        {
                            GameMode = new GameMode()
                            {
                                ApiQueryValue = "pve",
                                Name = "pve"
                            },
                            Language = "fr",
                            Prices = [.. TestData.Prices, .. TestData.Barters]
                        }
                    ])
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();

            Mock<IPresetsFetcher> presetsFetcherMock = new();
            presetsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            presetsFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.Presets)
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

            Mock<IWebsiteConfigurationFetcher> websiteConfigurationFetcherMock = new();
            websiteConfigurationFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            websiteConfigurationFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.WebsiteConfiguration)
                .Verifiable();

            JsonSerializerOptions serializationOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            string expectedChangelog = JsonSerializer.Serialize(TestData.Changelog as IEnumerable<object>, serializationOptions);
            string expectedItemCategories = JsonSerializer.Serialize(TestData.ItemCategories.Select(ic => ic.Id) as IEnumerable<object>, serializationOptions);
            string expectedItems = JsonSerializer.Serialize(TestData.Items as IEnumerable<object>, serializationOptions);
            string expectedPresets = JsonSerializer.Serialize(TestData.Presets as IEnumerable<object>, serializationOptions);
            string expectedPrices = JsonSerializer.Serialize(TestData.Prices.Concat(TestData.Barters) as IEnumerable<object>, serializationOptions);
            string expectedTarkovValues = JsonSerializer.Serialize(TestData.TarkovValues as object, serializationOptions);
            string expectedWebsiteConfiguration = JsonSerializer.Serialize(TestData.WebsiteConfiguration as object, serializationOptions);

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/changelog.json", expectedChangelog, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/items_en.json", expectedItems, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/items_fr.json", expectedItems, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/presets.json", expectedPresets, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/prices_pvp_en.json", expectedPrices, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/prices_pvp_fr.json", expectedPrices, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/prices_pve_en.json", expectedPrices, It.IsAny<BlobHttpHeaders>()))
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();
            azureBlobStorageManagerMock
                .Setup(m => m.UpdateBlob("$web", "data/prices_pve_fr.json", expectedPrices, It.IsAny<BlobHttpHeaders>()))
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

            GenerateWebsiteData function = new(
                new Mock<ILogger<GenerateWebsiteData>>().Object,
                configurationLoaderMock.Object,
                configurationWrapperMock.Object,
                azureBlobStorageManagerMock.Object,
                changelogFetcherMock.Object,
                gameModeLocalizedItemsFetcherMock.Object,
                gameModeLocalizedPricesFetcherMock.Object,
                itemCategoriesFetcherMock.Object,
                presetsFetcherMock.Object,
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
            gameModeLocalizedItemsFetcherMock.Verify();
            gameModeLocalizedPricesFetcherMock.Verify();
            presetsFetcherMock.Verify();
            tarkovValuesFetcherMock.Verify();
            websiteConfigurationFetcherMock.Verify();
        }
    }
}
