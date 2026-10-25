using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Abstractions.Items;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using TotovBuilder.Model.Utils;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="ItemsFetcher"/> class.
    /// </summary>
    public class ItemsFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnItems()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.Create(TestData.ItemsJson, TestData.ItemsTranslationsJson);

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
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

            ItemsFetcher itemsFetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            Mock<ILocalizedItemsFetcher> localizedItemsFetcherMock = new();
            localizedItemsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            localizedItemsFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(() => [
                    new LocalizedItems()
                    {
                        Items = [.. itemsFetcher.FetchedData!],
                        Language = "en"
                    }
                ]);

            PresetsFetcher presetsFetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                localizedItemsFetcherMock.Object);

            // Act
            Result itemsFetchResult = await itemsFetcher.Fetch();

            Result presetsFetchResult = await presetsFetcher.Fetch(); // The PresetsFetcher updates item properties

            // Assert
            itemsFetchResult.IsSuccess.Should().BeTrue();
            itemsFetcher.FetchedData.Should().NotBeNull();
            presetsFetchResult.IsSuccess.Should().BeTrue();
            presetsFetcher.FetchedData.Should().NotBeNull();

            IEnumerable<IItem> orderedResult = localizedItemsFetcherMock.Object.FetchedData!
                .Single(fd => fd.Language == "en")
                .Items
                    .OrderBy(i => $"{i.CategoryId} - {i.Name}");
            IEnumerable<Item> expected = TestData.Items.OrderBy(i => $"{i.CategoryId} - {i.Name}");

            orderedResult.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_WithoutStackMaxSize_ShouldReturnItemsStackableOnce()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.Create(@"{
  ""data"": {
    ""items"": {
      ""569668774bdc2da2298b4568"": {
        ""categories"": [
          ""543be5dd4bdc2deb348b4569"",
          ""5661632d4bdc2d903d8b456b"",
          ""54009119af1c881c07000029""
        ],
        ""conflictingItems"": [],
        ""iconLink"": ""https://assets.tarkov.dev/569668774bdc2da2298b4568-icon.webp"",
        ""id"": ""569668774bdc2da2298b4568"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/569668774bdc2da2298b4568-image.webp"",
        ""link"": ""https://tarkov.dev/item/euros"",
        ""name"": ""Euros"",
        ""properties"": null,
        ""shortName"": ""EUR"",
        ""weight"": 0,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Euros""
      }
    }
  }
}");

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
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

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();

            Item[] expected =
            [
                new Item()
                {
                    CategoryId = "currency",
                    IconLink = "https://assets.tarkov.dev/569668774bdc2da2298b4568-icon.webp",
                    Id = "569668774bdc2da2298b4568",
                    ImageLink = "https://assets.tarkov.dev/569668774bdc2da2298b4568-image.webp",
                    MarketLink = "https://tarkov.dev/item/euros",
                    MaxStackableAmount = 1,
                    Name = "Euros",
                    ShortName = "EUR",
                    Weight = 0,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Euros"
                }
            ];

            fetcher.FetchedData.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
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

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.Create(@"{
  ""data"": {
    ""items"": {
      ""invalid-item"": {},
      ""5c1d0c5f86f7744bb2683cf0"": {
        ""categories"": [
          ""5c164d2286f774194c5e69fa"",
          ""543be5e94bdc2df1348b4568""
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg"",
        ""id"": ""5c1d0c5f86f7744bb2683cf0"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg"",
        ""link"": ""https://tarkov.dev/item/terragroup-labs-keycard-blue"",
        ""name"": ""TerraGroup Labs keycard (Blue)"",
        ""properties"": {
          ""propertiesType"": ""ItemPropertiesKey""
        },
        ""shortName"": ""Blue"",
        ""weight"": 0.01,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)""
      }
    }
  }
}");

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
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

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new Item[]
            {
                new()
                {
                    CategoryId = "other",
                    IconLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg",
                    Id = "5c1d0c5f86f7744bb2683cf0",
                    ImageLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg",
                    MarketLink = "https://tarkov.dev/item/terragroup-labs-keycard-blue",
                    MaxStackableAmount = 1,
                    Name = "TerraGroup Labs keycard (Blue)",
                    ShortName = "Blue",
                    Weight = 0.01,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)"
                }
            }, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_WithBarrel_ShouldReadAccuracyModifier()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.Create(@"{
  ""data"": {
    ""items"": {
      ""test-barrel"": {
        ""categories"": [
          ""555ef6e44bdc2de9068b457e""
        ],
        ""conflictingItems"": [],
        ""iconLink"": ""https://assets.tarkov.dev/test-barrel-icon.webp"",
        ""id"": ""test-barrel"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/test-barrel-image.webp"",
        ""link"": ""https://tarkov.dev/item/test-barrel"",
        ""name"": ""Test barrel"",
        ""properties"": {
          ""propertiesType"": ""ItemPropertiesBarrel"",
          ""accuracyModifier"": -0.05,
          ""ergonomics"": -2,
          ""recoilModifier"": -0.03,
          ""slots"": []
        },
        ""shortName"": ""TB"",
        ""weight"": 0.5,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Test_barrel""
      }
    }
  }
}");

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
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

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new Item[]
            {
                new RangedWeaponMod()
                {
                    AccuracyModifierPercentage = -0.05,
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = -2,
                    IconLink = "https://assets.tarkov.dev/test-barrel-icon.webp",
                    Id = "test-barrel",
                    ImageLink = "https://assets.tarkov.dev/test-barrel-image.webp",
                    MarketLink = "https://tarkov.dev/item/test-barrel",
                    Name = "Test barrel",
                    RecoilModifierPercentage = -0.03,
                    ShortName = "TB",
                    Weight = 0.5,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Test_barrel"
                }
            }, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_ShouldReadRequiredModSlotsBlindingAmmunitionArmorSlotCaptionsAndDefaultPresets()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.Create(@"{
  ""data"": {
    ""items"": {
      ""test-armor"": {
        ""categories"": [
          ""5448e54d4bdc2dcc718b4568""
        ],
        ""conflictingItems"": [],
        ""iconLink"": ""https://assets.tarkov.dev/test-armor-icon.webp"",
        ""id"": ""test-armor"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/test-armor-image.webp"",
        ""link"": ""https://tarkov.dev/item/test-armor"",
        ""name"": ""Test armor"",
        ""properties"": {
          ""propertiesType"": ""ItemPropertiesArmor"",
          ""armorSlots"": [
            {
              ""allowedPlates"": [
                ""test-plate""
              ],
              ""name"": ""Front_plate"",
              ""nameId"": ""Front_plate"",
              ""zones"": [
                ""Armor Zone Front""
              ]
            }
          ],
          ""class"": 4,
          ""durability"": 50
        },
        ""shortName"": ""TA"",
        ""weight"": 5,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Test_armor""
      },
      ""test-armor-preset"": {
        ""categories"": [
          ""5448e54d4bdc2dcc718b4568""
        ],
        ""conflictingItems"": [],
        ""iconLink"": ""https://assets.tarkov.dev/test-armor-preset-icon.webp"",
        ""id"": ""test-armor-preset"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/test-armor-preset-image.webp"",
        ""link"": ""https://tarkov.dev/item/test-armor-preset"",
        ""name"": ""Test armor preset"",
        ""properties"": {
          ""propertiesType"": ""ItemPropertiesPreset"",
          ""baseItem"": ""test-armor"",
          ""default"": true
        },
        ""shortName"": ""TAP"",
        ""weight"": 5,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Test_armor_preset""
      },
      ""test-barrel"": {
        ""categories"": [
          ""555ef6e44bdc2de9068b457e""
        ],
        ""conflictingItems"": [],
        ""iconLink"": ""https://assets.tarkov.dev/test-barrel-icon.webp"",
        ""id"": ""test-barrel"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/test-barrel-image.webp"",
        ""link"": ""https://tarkov.dev/item/test-barrel"",
        ""name"": ""Test barrel"",
        ""properties"": {
          ""propertiesType"": ""ItemPropertiesBarrel"",
          ""ergonomics"": -2,
          ""recoilModifier"": -0.03,
          ""slots"": [
            {
              ""filters"": {
                ""allowedItems"": []
              },
              ""name"": ""MOD_MUZZLE"",
              ""nameId"": ""mod_muzzle"",
              ""required"": true
            },
            {
              ""filters"": {
                ""allowedItems"": []
              },
              ""name"": ""MOD_GAS_BLOCK"",
              ""nameId"": ""mod_gas_block"",
              ""required"": false
            }
          ]
        },
        ""shortName"": ""TB"",
        ""weight"": 0.5,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Test_barrel""
      },
      ""test-flashbang-ammunition"": {
        ""categories"": [
          ""5485a8684bdc2da71d8b4567""
        ],
        ""conflictingItems"": [],
        ""iconLink"": ""https://assets.tarkov.dev/test-flashbang-ammunition-icon.webp"",
        ""id"": ""test-flashbang-ammunition"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/test-flashbang-ammunition-image.webp"",
        ""link"": ""https://tarkov.dev/item/test-flashbang-ammunition"",
        ""name"": ""Test flashbang ammunition"",
        ""properties"": {
          ""propertiesType"": ""ItemPropertiesAmmo"",
          ""accuracyModifier"": 0,
          ""ammoType"": ""flashbang"",
          ""armorDamage"": 0,
          ""caliber"": ""Caliber23x75"",
          ""damage"": 0,
          ""durabilityBurnFactor"": 1,
          ""fragmentationChance"": 0,
          ""heavyBleedModifier"": 0,
          ""initialSpeed"": 210,
          ""lightBleedModifier"": 0,
          ""penetrationPower"": 0,
          ""projectileCount"": 1,
          ""recoilModifier"": 0,
          ""stackMaxSize"": 20,
          ""tracer"": false
        },
        ""shortName"": ""TFA"",
        ""stackMaxSize"": 20,
        ""weight"": 0.1,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Test_flashbang_ammunition""
      }
    }
  }
}", @"{
  ""data"": {
    ""Front_plate"": ""FR. PLATE""
  }
}");

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
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

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();

            Armor armor = (Armor)fetcher.FetchedData!.Single(i => i.Id == "test-armor");
            armor.DefaultPresetId.Should().Be("test-armor-preset");
            armor.ModSlots.Single().Caption.Should().Be("FR. PLATE");

            RangedWeaponMod barrel = (RangedWeaponMod)fetcher.FetchedData!.Single(i => i.Id == "test-barrel");
            barrel.ModSlots.Select(ms => ms.Required).Should().Equal(true, false);

            Ammunition ammunition = (Ammunition)fetcher.FetchedData!.Single(i => i.Id == "test-flashbang-ammunition");
            ammunition.Blinding.Should().BeTrue();
            ammunition.MaxStackableAmount.Should().Be(20);
        }

        [Fact]
        public async Task Fetch_WithFailedItemsFetching_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = new();
            apiRequesterMock
                .Setup(m => m.RequestData(It.IsAny<string>()))
                .Returns(Task.FromResult(Result.Fail<JsonElement>("Error")))
                .Verifiable();
            apiRequesterMock
                .Setup(m => m.RequestTranslations(It.IsAny<string>()))
                .Returns(Task.FromResult(Result.Ok(new Translations(new JsonElement()))))
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

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Items - No data fetched.");
            fetcher.FetchedData.Should().BeNull();
        }

        [Fact]
        public async Task Fetch_WithNotImplementedItemCategory_ShouldIgnoreData()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.Create(@"{
  ""data"": {
    ""items"": {
      ""5c1d0c5f86f7744bb2683cf0"": {
        ""categories"": [
          ""NotImplementedItemType""
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg"",
        ""id"": ""5c1d0c5f86f7744bb2683cf0"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg"",
        ""link"": ""https://tarkov.dev/item/terragroup-labs-keycard-blue"",
        ""name"": ""TerraGroup Labs keycard (Blue)"",
        ""properties"": {
          ""propertiesType"": ""ItemPropertiesKey""
        },
        ""shortName"": ""Blue"",
        ""weight"": 0.01,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)""
      }
    }
  }
}");

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(new ItemCategory[]
                {
                    new()
                    {
                        Id = "NotImplementedItemCategory",
                        Types =
                        [
                            new ItemType()
                            {
                                Id = "NotImplementedItemType",
                                Name = "Not implemented item type"
                            }
                        ]
                    }
                })
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

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEmpty();
        }

        [Fact]
        public async Task Fetch_WithFailedRawDataFetching_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.Create(@"{
  ""data"": {
    ""items"": {
      ""5ab8ebf186f7742d8b372e80"": {
        ""categories"": [
          ""5448e53e4bdc2d60728b4567""
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5ab8ebf186f7742d8b372e80-icon.jpg"",
        ""id"": ""5ab8ebf186f7742d8b372e80"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/5ab8ebf186f7742d8b372e80-image.jpg"",
        ""link"": ""https://tarkov.dev/item/sso-attack-2-raid-backpack"",
        ""name"": ""SSO Attack 2 raid backpack"",
        ""properties"": {
          ""propertiesType"": ""ItemPropertiesBackpack"",
          ""capacity"": 35
        },
        ""shortName"": ""Attack 2"",
        ""weight"": 6.12,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/SSO_Attack_2_raid_backpack""
      }
    }
  }
}");

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
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

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Items - No data fetched.");
        }

        [Fact]
        public async Task Fetch_WithInvalidPreset_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.Create(@"{
  ""data"": {
    ""items"": {
      ""5c1d0c5f86f7744bb2683cf0"": {
        ""categories"": [
          ""5c164d2286f774194c5e69fa"",
          ""543be5e94bdc2df1348b4568""
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg"",
        ""id"": ""5c1d0c5f86f7744bb2683cf0"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg"",
        ""link"": ""https://tarkov.dev/item/terragroup-labs-keycard-blue"",
        ""name"": ""TerraGroup Labs keycard (Blue)"",
        ""properties"": {
          ""propertiesType"": ""ItemPropertiesKey""
        },
        ""shortName"": ""Blue"",
        ""weight"": 0.01,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)""
      },
      ""preset-with-non-existing-base-item"": {
        ""categories"": [
          ""57bef4c42459772e8d35a53b"",
          ""543be5f84bdc2dd4348b456a"",
          ""566162e44bdc2d3f298b4573"",
          ""54009119af1c881c07000029""
        ],
        ""iconLink"": ""https://assets.tarkov.dev/preset-with-non-existing-base-item-icon.jpg"",
        ""id"": ""preset-with-non-existing-base-item"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/preset-with-non-existing-base-item-image.jpg"",
        ""link"": ""https://tarkov.dev/item/preset-with-non-existing-base-item"",
        ""name"": ""Non existing base item"",
        ""properties"": {
          ""propertiesType"": ""ItemPropertiesPreset"",
          ""baseItem"": ""non-existing-base-item"",
          ""moa"": null
        },
        ""shortName"": ""NEBI"",
        ""weight"": 0.01,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/preset-with-non-existing-base-item""
      }
    }
  }
}");

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
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

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new Item[]
            {
                new()
                {
                    CategoryId = "other",
                    IconLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg",
                    Id = "5c1d0c5f86f7744bb2683cf0",
                    ImageLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg",
                    MarketLink = "https://tarkov.dev/item/terragroup-labs-keycard-blue",
                    MaxStackableAmount = 1,
                    Name = "TerraGroup Labs keycard (Blue)",
                    ShortName = "Blue",
                    Weight = 0.01,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)"
                }
            }, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_WithPresetWithNotModdableBaseItem_ShouldReturnItemForThisPreset()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.Create(@"{
  ""data"": {
    ""items"": {
      ""5c1d0c5f86f7744bb2683cf0"": {
        ""categories"": [
          ""5c164d2286f774194c5e69fa"",
          ""543be5e94bdc2df1348b4568""
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg"",
        ""id"": ""5c1d0c5f86f7744bb2683cf0"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg"",
        ""link"": ""https://tarkov.dev/item/terragroup-labs-keycard-blue"",
        ""name"": ""TerraGroup Labs keycard (Blue)"",
        ""properties"": {
          ""propertiesType"": ""ItemPropertiesKey""
        },
        ""shortName"": ""Blue"",
        ""weight"": 0.01,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)""
      },
      ""preset-with-non-moddable-base-item"": {
        ""categories"": [
          ""5c164d2286f774194c5e69fa"",
          ""543be5e94bdc2df1348b4568""
        ],
        ""iconLink"": ""https://assets.tarkov.dev/preset-with-non-moddable-base-item-icon.jpg"",
        ""id"": ""preset-with-non-moddable-base-item"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/preset-with-non-moddable-base-item-image.jpg"",
        ""link"": ""https://tarkov.dev/item/preset-with-non-moddable-base-item"",
        ""name"": ""Non moddable base item"",
        ""properties"": {
          ""propertiesType"": ""ItemPropertiesPreset"",
          ""baseItem"": ""5c1d0c5f86f7744bb2683cf0"",
          ""moa"": null
        },
        ""shortName"": ""NMBI"",
        ""weight"": 0.01,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/preset-with-non-moddable-base-item""
      }
    }
  }
}");

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
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

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new Item[]
            {
                new()
                {
                    CategoryId = "other",
                    IconLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg",
                    Id = "5c1d0c5f86f7744bb2683cf0",
                    ImageLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg",
                    MarketLink = "https://tarkov.dev/item/terragroup-labs-keycard-blue",
                    MaxStackableAmount = 1,
                    Name = "TerraGroup Labs keycard (Blue)",
                    ShortName = "Blue",
                    Weight = 0.01,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)"
                },
                new()
                {
                    CategoryId = "other",
                    IconLink = "https://assets.tarkov.dev/preset-with-non-moddable-base-item-icon.jpg",
                    Id = "preset-with-non-moddable-base-item",
                    ImageLink = "https://assets.tarkov.dev/preset-with-non-moddable-base-item-image.jpg",
                    MarketLink = "https://tarkov.dev/item/preset-with-non-moddable-base-item",
                    MaxStackableAmount = 1,
                    Name = "Non moddable base item",
                    ShortName = "NMBI",
                    Weight = 0.01,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/preset-with-non-moddable-base-item"
                }
            }, options => options.RespectingRuntimeTypes());
        }
    }
}
