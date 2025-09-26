using System.Collections.Generic;
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
using TotovBuilder.Model.Builds;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using TotovBuilder.Model.Utils;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="PresetsFetcher"/> class.
    /// </summary>
    public class PresetsFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnPresets()
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
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PresetsJson) };
                })
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
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
                            Language = "en",
                        }
                    ])
                .Verifiable();

            PresetsFetcher fetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                localizedItemsFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();

            IEnumerable<InventoryItem> orderedResult = fetcher.FetchedData!.OrderBy(p => p.ItemId);
            IEnumerable<InventoryItem> expected = TestData.Presets.OrderBy(i => i.ItemId);

            orderedResult.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnOnlyValidData()
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
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""items"": [
      {
      },
      {
        ""containsItems"": [
            {
            ""item"": {
                ""id"": ""5a16b7e1fcdbcb00165aa6c9""
            },
            ""quantity"": 1
            }
        ],
        ""id"": ""test-preset-not-existing""
      },
      {
        ""containsItems"": [
            {
            ""item"": {
                ""id"": ""5a16b7e1fcdbcb00165aa6c9""
            },
            ""quantity"": 1
            }
        ],
        ""id"": ""test-preset-face-shield-alone""
      }
    ]
  }
}") };
                })
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
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
                            Language = "en",
                        }
                    ])
                .Verifiable();

            PresetsFetcher fetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                localizedItemsFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new InventoryItem[]
                {
                    new()
                    {
                        ItemId = "test-preset-face-shield-alone"
                    }
                });
        }


        [Fact]
        public async Task Fetch_WithoutValidData_ShouldFail()
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
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""items"": [
      {
        ""containsItems"": [
            {
            ""item"": {
                ""id"": ""5a16b7e1fcdbcb00165aa6c9""
            },
            ""quantity"": 1
            }
        ],
        ""id"": ""test-preset-not-existing""
      }
    ]
  }
}") };
                })
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
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
                            Language = "en",
                        }
                    ])
                .Verifiable();

            PresetsFetcher fetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                localizedItemsFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Message.Should().Be("Presets - No data fetched.");
        }

        [Fact]
        public async Task Fetch_WithIncompatibleAmmunitionInMagazine_ShouldTryFindingASlotUntilMaximumTriesAndReturnNothing()
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
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""items"": [
      {
        ""containsItems"": [
            {
            ""item"": {
                ""id"": ""601aa3d2b2bcb34913271e6d""
            },
            ""quantity"": 30
            }
        ],
        ""id"": ""test-preset-magazine-with-incompatible-ammunition""
      }
    ]
  }
}") };
                })
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<ILocalizedItemsFetcher> localizedItemsFetcherMock = new();
            localizedItemsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            localizedItemsFetcherMock.SetupGet(m => m.FetchedData)
                .Returns(
                    [
                        new LocalizedItems()
                        {
                            Items = new Item[]
                            {
                                new Ammunition()
                                {
                                    AccuracyModifierPercentage = -0.05,
                                    ArmorDamagePercentage = 0.76,
                                    //Blinding = , // TODO : MISSING FROM API
                                    Caliber = "Caliber762x39",
                                    CategoryId = "ammunition",
                                    DurabilityBurnModifierPercentage = 0.7,
                                    FleshDamage = 47,
                                    FragmentationChance = 0.05,
                                    HeavyBleedingChance = 0.1,
                                    IconLink = "https://assets.tarkov.dev/601aa3d2b2bcb34913271e6d-icon.jpg",
                                    Id = "601aa3d2b2bcb34913271e6d",
                                    ImageLink = "https://assets.tarkov.dev/601aa3d2b2bcb34913271e6d-image.jpg",
                                    LightBleedingChance = 0.1,
                                    MarketLink = "https://tarkov.dev/item/762x39mm-mai-ap",
                                    MaxStackableAmount = 60,
                                    Name = "7.62x39mm MAI AP",
                                    PenetratedArmorLevel = 5,
                                    PenetrationPower = 58,
                                    Projectiles = 1,
                                    RecoilModifier = 10,
                                    ShortName = "MAI AP",
                                    Subsonic = false,
                                    Tracer = false,
                                    Velocity = 730,
                                    Weight = 0.012,
                                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/7.62x39mm_MAI_AP"
                                },
                                new Magazine()
                                {
                                    AcceptedAmmunitionIds =
                                    [
                                        "5c0d5e4486f77478390952fe",
                                        "61962b617c6c7b169525f168",
                                        "56dfef82d2720bbd668b4567",
                                        "56dff026d2720bb8668b4567",
                                        "56dff061d2720bb5668b4567",
                                        "56dff0bed2720bb0668b4567",
                                        "56dff216d2720bbd668b4568",
                                        "56dff2ced2720bb4668b4567",
                                        "56dff338d2720bbd668b4569",
                                        "56dff3afd2720bba668b4567",
                                        "56dff421d2720b5f5a8b4567",
                                        "56dff4a2d2720bbd668b456a",
                                        "56dff4ecd2720b5f5a8b4568"
                                    ],
                                    Capacity = 30,
                                    CategoryId = "magazine",
                                    ErgonomicsModifier = -3,
                                    IconLink = "https://assets.tarkov.dev/564ca99c4bdc2d16268b4589-icon.jpg",
                                    Id = "564ca99c4bdc2d16268b4589",
                                    ImageLink = "https://assets.tarkov.dev/564ca99c4bdc2d16268b4589-image.jpg",
                                    MalfunctionPercentage = 0.07,
                                    MarketLink = "https://tarkov.dev/item/ak-74-545x39-6l20-30-round-magazine",
                                    Name = "AK-74 5.45x39 6L20 30-round magazine",
                                    ShortName = "6L20",
                                    Weight = 0.215,
                                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/AK-74_5.45x39_6L20_30-round_magazine"
                                },
                                new Magazine()
                                {
                                    AcceptedAmmunitionIds =
                                    [
                                        "5c0d5e4486f77478390952fe",
                                        "61962b617c6c7b169525f168",
                                        "56dfef82d2720bbd668b4567",
                                        "56dff026d2720bb8668b4567",
                                        "56dff061d2720bb5668b4567",
                                        "56dff0bed2720bb0668b4567",
                                        "56dff216d2720bbd668b4568",
                                        "56dff2ced2720bb4668b4567",
                                        "56dff338d2720bbd668b4569",
                                        "56dff3afd2720bba668b4567",
                                        "56dff421d2720b5f5a8b4567",
                                        "56dff4a2d2720bbd668b456a",
                                        "56dff4ecd2720b5f5a8b4568"
                                    ],
                                    BaseItemId = "564ca99c4bdc2d16268b4589",
                                    Capacity = 30,
                                    CategoryId = "magazine",
                                    ErgonomicsModifier = -3,
                                    IconLink = "https://assets.tarkov.dev/preset-magazine-with-incompatible-ammunition.jpg",
                                    Id = "test-preset-magazine-with-incompatible-ammunition",
                                    ImageLink = "https://assets.tarkov.dev/preset-magazine-with-incompatible-ammunition.jpg",
                                    MalfunctionPercentage = 0.07,
                                    MarketLink = "https://tarkov.dev/item/preset-magazine-with-incompatible-ammunition",
                                    Name = "Magazine with incompatible ammunition",
                                    ShortName = "MWIA",
                                    Weight = 0.215,
                                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/preset-magazine-with-incompatible-ammunition"
                                }
                            },
                            Language = "en",
                        }
                    ])
                .Verifiable();

            PresetsFetcher fetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                localizedItemsFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new InventoryItem[]
            {
                new()
                {
                    ItemId = "test-preset-magazine-with-incompatible-ammunition"
                }
            });
        }

        [Fact]
        public async Task Fetch_WithNonMagazineItemContainingAmmunition_ShouldTryFindingASlotUntilMaximumTriesAndReturnNothing()
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
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""items"": [
      {
        ""containsItems"": [
            {
              ""item"": {
                  ""id"": ""601aa3d2b2bcb34913271e6d""
              },
              ""quantity"": 30
            }
        ],
        ""id"": ""test-preset-non-magazine-item-with-ammunition""
      }
    ]
  }
}") };
                })
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
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
                            Items = new Item[]
                            {
                                new Ammunition()
                                {
                                    AccuracyModifierPercentage = -0.05,
                                    ArmorDamagePercentage = 0.76,
                                    //Blinding = , // TODO : MISSING FROM API
                                    Caliber = "Caliber762x39",
                                    CategoryId = "ammunition",
                                    DurabilityBurnModifierPercentage = 0.7,
                                    FleshDamage = 47,
                                    FragmentationChance = 0.05,
                                    HeavyBleedingChance = 0.1,
                                    IconLink = "https://assets.tarkov.dev/601aa3d2b2bcb34913271e6d-icon.jpg",
                                    Id = "601aa3d2b2bcb34913271e6d",
                                    ImageLink = "https://assets.tarkov.dev/601aa3d2b2bcb34913271e6d-image.jpg",
                                    LightBleedingChance = 0.1,
                                    MarketLink = "https://tarkov.dev/item/762x39mm-mai-ap",
                                    MaxStackableAmount = 60,
                                    Name = "7.62x39mm MAI AP",
                                    PenetratedArmorLevel = 5,
                                    PenetrationPower = 58,
                                    Projectiles = 1,
                                    RecoilModifier = 10,
                                    ShortName = "MAI AP",
                                    Subsonic = false,
                                    Tracer = false,
                                    Velocity = 730,
                                    Weight = 0.012,
                                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/7.62x39mm_MAI_AP"
                                },
                                new RangedWeaponMod()
                                {
                                    CategoryId = "rangedWeaponMod",
                                    ErgonomicsModifier = -2,
                                    IconLink = "https://assets.tarkov.dev/57dc324a24597759501edc20-icon.jpg",
                                    Id = "57dc324a24597759501edc20",
                                    ImageLink = "https://assets.tarkov.dev/57dc324a24597759501edc20-image.jpg",
                                    MarketLink = "https://tarkov.dev/item/aks-74u-545x39-muzzle-brake-6p26-0-20",
                                    Name = "AKS-74U 5.45x39 muzzle brake (6P26 0-20)",
                                    RecoilModifierPercentage = -0.08,
                                    ShortName = "6P26 0-20",
                                    Weight = 0.1,
                                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/AKS-74U_5.45x39_muzzle_brake_(6P26_0-20)"
                                },
                                new RangedWeaponMod()
                                {
                                    BaseItemId = "57dc324a24597759501edc20",
                                    CategoryId = "rangedWeaponMod",
                                    ErgonomicsModifier = -2,
                                    IconLink = "https://assets.tarkov.dev/preset-non-magazine-item-with-ammunition.jpg",
                                    Id = "test-preset-non-magazine-item-with-ammunition",
                                    ImageLink = "https://assets.tarkov.dev/preset-non-magazine-item-with-ammunition.jpg",
                                    MarketLink = "https://tarkov.dev/item/preset-non-magazine-item-with-ammunition",
                                    Name = "Non magazine with ammunition",
                                    RecoilModifierPercentage = -0.08,
                                    ShortName = "NMWA",
                                    Weight = 0.1,
                                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/preset-non-magazine-item-with-ammunition"
                                }
                            },
                            Language = "en",
                        }
                    ])
                .Verifiable();

            PresetsFetcher fetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                localizedItemsFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new InventoryItem[]
            {
                new()
                {
                    ItemId = "test-preset-non-magazine-item-with-ammunition"
                }
            });
        }

        [Fact]
        public async Task Fetch_WithMoreModslotThanContainedItems_ShouldStopConstructingPresetWhenNoContainedItemRemains()
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
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
          ""data"": {
            ""items"": [
              {
                ""containsItems"": [
                  {
                    ""item"": {
                      ""id"": ""57e3dba62459770f0c32322b""
                    },
                    ""quantity"": 1
                  }
                ],
                ""id"": ""584147732459775a2b6d9f12""
              }
            ]
          }
        }") };
                })
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
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
                        }
                    ])
                .Verifiable();

            PresetsFetcher fetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                localizedItemsFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();

            IEnumerable<InventoryItem> orderedResult = fetcher.FetchedData!.OrderBy(p => p.ItemId);
            IEnumerable<InventoryItem> expected = new InventoryItem[]
            {
                new()
                {
                    ItemId = "584147732459775a2b6d9f12",
                    ModSlots =
                    [
                        new InventoryItemModSlot()
                        {
                            Item = new InventoryItem()
                            {
                                ItemId = "57e3dba62459770f0c32322b",
                            },
                            ModSlotName = "mod_pistol_grip"
                        }
                    ]
                }
            };

            orderedResult.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Fetch_WithNonModdableContainedItems_ShouldStopConstructingPresetWhenNoContainedItemRemains()
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
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
          ""data"": {
            ""items"": [
              {
                ""containsItems"": [
                  {
                    ""item"": {
                      ""id"": ""5894a05586f774094708ef75""
                    },
                    ""quantity"": 1
                  },
                  {
                    ""item"": {
                      ""id"": ""5efb0da7a29a85116f6ea05f""
                    },
                    ""quantity"": 10
                  },
                  {
                    ""item"": {
                      ""id"": ""5c3df7d588a4501f290594e5""
                    },
                    ""quantity"": 20
                  }
                ],
                ""id"": ""5a8ae43686f774377b73cfb3""
              }
            ]
          }
        }") };
                })
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
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
                        }
                    ])
                .Verifiable();

            PresetsFetcher fetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                localizedItemsFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();

            IEnumerable<InventoryItem> orderedResult = fetcher.FetchedData!.OrderBy(p => p.ItemId);
            IEnumerable<InventoryItem> expected = new InventoryItem[]            {
                new()
                {
                    ItemId = "5a8ae43686f774377b73cfb3",
                    ModSlots =
                    [
                        new InventoryItemModSlot()
                        {
                            Item = new InventoryItem()
                            {
                                Content =
                                [
                                    new InventoryItem()
                                    {
                                        ItemId = "5efb0da7a29a85116f6ea05f",
                                        Quantity = 30
                                    }
                                ],
                                ItemId = "5894a05586f774094708ef75",
                            },
                            ModSlotName = "mod_magazine"
                        }
                    ]
                }
            };

            orderedResult.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Fetch_WithFailedItemsFetch_ShouldFail()
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
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PresetsJson) };
                })
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<ILocalizedItemsFetcher> localizedItemsFetcherMock = new();
            localizedItemsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();

            PresetsFetcher fetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                localizedItemsFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Presets - No data fetched.");

            localizedItemsFetcherMock.Verify();
        }
    }
}
