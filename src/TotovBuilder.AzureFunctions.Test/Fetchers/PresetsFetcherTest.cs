using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.AzureFunctions.Models.Builds;
using TotovBuilder.AzureFunctions.Test.Mocks;
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
            Mock<ILogger<PresetsFetcher>> loggerMock = new Mock<ILogger<PresetsFetcher>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzurePresetsBlobName = "presets.json"
            });

            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.PresetsJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            PresetsFetcher fetcher = new PresetsFetcher(loggerMock.Object, blobFetcherMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<InventoryItem>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Presets);
        }

        [Fact]
        public async void Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<ILogger<PresetsFetcher>> loggerMock = new Mock<ILogger<PresetsFetcher>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzurePresetsBlobName = "presets.json"
            });

            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(@"[
  {
    ""invalid"": {}
  },
  {
    ""content"": [],
    ""ignorePrice"": false,
    ""itemId"": ""57dc2fa62459775949412633"",
    ""modSlots"": [
      {
        ""item"": {
          ""content"": [],
          ""ignorePrice"": false,
          ""itemId"": ""57e3dba62459770f0c32322b"",
          ""modSlots"": [],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_pistol_grip""
      },
      {
        ""item"": {
          ""content"": [],
          ""ignorePrice"": false,
          ""itemId"": ""57dc347d245977596754e7a1"",
          ""modSlots"": [],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_stock""
      },
      {
        ""item"": {
          ""content"": [
            {
              ""content"": [],
              ""ignorePrice"": false,
              ""itemId"": ""56dfef82d2720bbd668b4567"",
              ""modSlots"": [],
              ""quantity"": 30
            }
          ],
          ""ignorePrice"": false,
          ""itemId"": ""564ca99c4bdc2d16268b4589"",
          ""modSlots"": [],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_magazine""
      },
      {
        ""item"": {
          ""content"": [],
          ""ignorePrice"": false,
          ""itemId"": ""57dc324a24597759501edc20"",
          ""modSlots"": [],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_muzzle""
      },
      {
        ""item"": {
          ""content"": [],
          ""ignorePrice"": false,
          ""itemId"": ""57dc334d245977597164366f"",
          ""modSlots"": [],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_reciever""
      },
      {
        ""item"": {
          ""content"": [],
          ""ignorePrice"": false,
          ""itemId"": ""59d36a0086f7747e673f3946"",
          ""modSlots"": [
            {
              ""item"": {
                ""content"": [],
                ""ignorePrice"": false,
                ""itemId"": ""57dc32dc245977596d4ef3d3"",
                ""modSlots"": [],
                ""quantity"": 1
              },
              ""modSlotName"": ""mod_handguard""
            }
          ],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_gas_block""
      }
    ],
    ""quantity"": 1
  }
]
")));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<InventoryItem>>(It.IsAny<DataType>())).Returns(value: null);

            PresetsFetcher fetcher = new PresetsFetcher(loggerMock.Object, blobFetcherMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<InventoryItem>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(new InventoryItem[]
            {
                new InventoryItem()
                {
                    ItemId = "57dc2fa62459775949412633",
                    ModSlots = new InventoryModSlot[]
                    {
                        new InventoryModSlot()
                        {
                            Item = new InventoryItem()
                            {
                                ItemId = "57e3dba62459770f0c32322b",
                            },
                            ModSlotName = "mod_pistol_grip"
                        },
                        new InventoryModSlot()
                        {
                            Item = new InventoryItem()
                            {
                                ItemId = "57dc347d245977596754e7a1",
                            },
                            ModSlotName = "mod_stock"
                        },
                        new InventoryModSlot()
                        {
                            Item = new InventoryItem()
                            {
                                Content = new InventoryItem[]
                                {
                                    new InventoryItem()
                                    {
                                        ItemId = "56dfef82d2720bbd668b4567",
                                        Quantity = 30
                                    }
                                },
                                ItemId = "564ca99c4bdc2d16268b4589",
                            },
                            ModSlotName = "mod_magazine"
                        },
                        new InventoryModSlot()
                        {
                            Item = new InventoryItem()
                            {
                                ItemId = "57dc324a24597759501edc20",
                            },
                            ModSlotName = "mod_muzzle"
                        },
                        new InventoryModSlot()
                        {
                            Item = new InventoryItem()
                            {
                                ItemId = "57dc334d245977597164366f",
                            },
                            ModSlotName = "mod_reciever"
                        },
                        new InventoryModSlot()
                        {
                            Item = new InventoryItem()
                            {
                                ItemId = "59d36a0086f7747e673f3946",
                                ModSlots = new InventoryModSlot[]
                                {
                                    new InventoryModSlot()
                                    {
                                        Item = new InventoryItem()
                                        {
                                            ItemId = "57dc32dc245977596d4ef3d3"
                                        },
                                        ModSlotName = "mod_handguard"
                                    }
                                }
                            },
                            ModSlotName = "mod_gas_block"
                        }
                    }
                }
            });
        }
    }
}
