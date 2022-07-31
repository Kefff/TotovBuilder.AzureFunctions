using TotovBuilder.AzureFunctions.Models.Builds;

namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    /// <summary>
    /// Represents test data.
    /// </summary>
    public static partial class TestData
    {
        public static InventoryItem[] Presets = new InventoryItem[]
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
            },
            new InventoryItem()
            {
                ItemId = "5fc22d7c187fea44d52eda44",
                ModSlots = new InventoryModSlot[]
                {
                    new InventoryModSlot()
                    {
                        Item = new InventoryItem()
                        {
                            ItemId = "57c55efc2459772d2c6271e7",
                        },
                        ModSlotName = "mod_pistol_grip",
                    },
                    new InventoryModSlot()
                    {
                        Item = new InventoryItem()
                        {
                            ItemId = "5fc23426900b1d5091531e15",
                        },
                        ModSlotName = "mod_magazine",
                    },
                    new InventoryModSlot()
                    {
                        Item = new InventoryItem()
                        {
                            ItemId = "5649be884bdc2d79388b4577",
                            ModSlots = new InventoryModSlot[]
                            {
                                new InventoryModSlot()
                                {
                                    Item = new InventoryItem()
                                    {
                                        ItemId = "5fc2369685fd526b824a5713"
                                    },
                                    ModSlotName = "mod_stock_000"
                                }
                            }
                        },
                        ModSlotName = "mod_stock_001",
                    },
                    new InventoryModSlot()
                    {
                        Item = new InventoryItem()
                        {
                            ItemId = "5fc278107283c4046c581489",
                            ModSlots = new InventoryModSlot[]
                            {
                                new InventoryModSlot()
                                {
                                    Item = new InventoryItem()
                                    {
                                        ItemId = "5fc23678ab884124df0cd590",
                                        ModSlots = new InventoryModSlot[]
                                        {
                                            new InventoryModSlot()
                                            {
                                                Item = new InventoryItem()
                                                {
                                                    ItemId = "5fc23636016cce60e8341b05"
                                                },
                                                ModSlotName = "mod_muzzle",
                                            },
                                            new InventoryModSlot()
                                            {
                                                Item = new InventoryItem()
                                                {
                                                    ItemId = "5fc2360f900b1d5091531e19"
                                                },
                                                ModSlotName = "mod_gas_block",
                                            }
                                        }
                                    },
                                    ModSlotName = "mod_barrel",
                                },
                                new InventoryModSlot()
                                {
                                    Item = new InventoryItem()
                                    {
                                        ItemId = "5fc235db2770a0045c59c683",
                                    },
                                    ModSlotName = "mod_handguard",
                                }
                            }
                        },
                        ModSlotName = "mod_reciever",
                    }
                }
            }
        };

        public const string PresetsJson = @"[
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
  },
  {
    ""content"": [],
    ""ignorePrice"": false,
    ""itemId"": ""5fc22d7c187fea44d52eda44"",
    ""modSlots"": [
      {
        ""item"": {
          ""content"": [],
          ""ignorePrice"": false,
          ""itemId"": ""57c55efc2459772d2c6271e7"",
          ""modSlots"": [],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_pistol_grip""
      },
      {
        ""item"": {
          ""content"": [],
          ""ignorePrice"": false,
          ""itemId"": ""5fc23426900b1d5091531e15"",
          ""modSlots"": [],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_magazine""
      },
      {
        ""item"": {
          ""content"": [],
          ""ignorePrice"": false,
          ""itemId"": ""5649be884bdc2d79388b4577"",
          ""modSlots"": [
            {
              ""item"": {
                ""content"": [],
                ""ignorePrice"": false,
                ""itemId"": ""5fc2369685fd526b824a5713"",
                ""modSlots"": [],
                ""quantity"": 1
              },
              ""modSlotName"": ""mod_stock_000""
            }
          ],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_stock_001""
      },
      {
        ""item"": {
          ""content"": [],
          ""ignorePrice"": false,
          ""itemId"": ""5fc278107283c4046c581489"",
          ""modSlots"": [
            {
              ""item"": {
                ""content"": [],
                ""ignorePrice"": false,
                ""itemId"": ""5fc23678ab884124df0cd590"",
                ""modSlots"": [
                  {
                    ""item"": {
                      ""content"": [],
                      ""ignorePrice"": false,
                      ""itemId"": ""5fc23636016cce60e8341b05"",
                      ""modSlots"": [],
                      ""quantity"": 1
                    },
                    ""modSlotName"": ""mod_muzzle""
                  },
                  {
                    ""item"": {
                      ""content"": [],
                      ""ignorePrice"": false,
                      ""itemId"": ""5fc2360f900b1d5091531e19"",
                      ""modSlots"": [],
                      ""quantity"": 1
                    },
                    ""modSlotName"": ""mod_gas_block""
                  }
                ],
                ""quantity"": 1
              },
              ""modSlotName"": ""mod_barrel""
            },
            {
              ""item"": {
                ""content"": [],
                ""ignorePrice"": false,
                ""itemId"": ""5fc235db2770a0045c59c683"",
                ""modSlots"": [],
                ""quantity"": 1
              },
              ""modSlotName"": ""mod_handguard""
            }
          ],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_reciever""
      }
    ],
    ""quantity"": 1
  }
]";
    }
}
