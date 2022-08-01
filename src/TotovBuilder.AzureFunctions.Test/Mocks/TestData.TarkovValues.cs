using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    /// <summary>
    /// Represents test data.
    /// </summary>
    public static partial class TestData
    {
        public static TarkovValues TarkovValues = new TarkovValues()
        {
            ArmorPenetrationEfficiencies = new string[]
            {
                "> 20",
                "13 - 20",
                "9 - 13",
                "5 - 9",
                "3 - 5",
                "1 - 3",
                "< 1"
            },
            ChestHp = 85,
            Currencies = new Currency[]
            {
                new Currency()
                {
                    IconName = "dollar-sign",
                    ItemId = "5696686a4bdc2da3298b456a",
                    MainCurrency = false,
                    Name = "USD"
                },
                new Currency()
                {
                    IconName = "euro-sign",
                    ItemId = "569668774bdc2da2298b4568",
                    MainCurrency = false,
                    Name = "EUR"
                },
                new Currency()
                {
                    IconName = "ruble-sign",
                    ItemId = "5449016a4bdc2d6f028b456f",
                    MainCurrency = true,
                    Name = "RUB"
                }
            },
            HeavyEncumbermentWeight = 65,
            LightEncumbermentWeight = 24,
            Merchants = new Merchant[]
            {
                new Merchant()
                {
                    MaxLevel = 4,
                    MinLevel = 1,
                    Name = "prapor",
                    ShowInFilter = true
                },
                new Merchant()
                {
                    MaxLevel = 4,
                    MinLevel = 1,
                    Name = "therapist",
                    ShowInFilter = true
                },
                new Merchant()
                {
                    MaxLevel = 4,
                    MinLevel = 1,
                    Name = "skier",
                    ShowInFilter = true
                },
                new Merchant()
                {
                    MaxLevel = 1,
                    MinLevel = 1,
                    Name = "fence",
                    ShowInFilter = false
                },
                new Merchant()
                {
                    MaxLevel = 4,
                    MinLevel = 1,
                    Name = "peacekeeper",
                    ShowInFilter = true
                },
                new Merchant()
                {
                    MaxLevel = 4,
                    MinLevel = 1,
                    Name = "mechanic",
                    ShowInFilter = true
                },
                new Merchant()
                {
                    MaxLevel = 4,
                    MinLevel = 1,
                    Name = "ragman",
                    ShowInFilter = true
                },
                new Merchant()
                {
                    MaxLevel = 4,
                    MinLevel = 1,
                    Name = "jaeger",
                    ShowInFilter = true
                },
                new Merchant()
                {
                    MaxLevel = 0,
                    MinLevel = 0,
                    Name = "flea-market",
                    ShowInFilter = true
                },
                new Merchant()
                {
                    MaxLevel = 0,
                    MinLevel = 0,
                    Name = "items-without-merchant",
                    ShowInFilter = true
                }
            },
            RicochetChances = new RicochetChance[]
            {
                new RicochetChance()
                {
                    Name = "High",
                    XMaxValue = 1,
                    XMinValue = 0.9
                },
                new RicochetChance()
                {
                    Name = "Medium",
                    XMaxValue = 0.8,
                    XMinValue = 0.7
                },
                new RicochetChance()
                {
                    Name = "Low",
                    XMaxValue = 0.6,
                    XMinValue = 0.1
                }
            }
        };

        public const string TarkovValuesJson = @"{
  ""armorPenetrationEfficiencies"": [
    ""> 20"",
    ""13 - 20"",
    ""9 - 13"",
    ""5 - 9"",
    ""3 - 5"",
    ""1 - 3"",
    ""< 1""
  ],
  ""chestHp"": 85,
  ""currencies"": [
    {
      ""iconName"": ""dollar-sign"",
      ""itemId"": ""5696686a4bdc2da3298b456a"",
      ""mainCurrency"": false,
      ""name"": ""USD""
    },
    {
      ""iconName"": ""euro-sign"",
      ""itemId"": ""569668774bdc2da2298b4568"",
      ""mainCurrency"": false,
      ""name"": ""EUR""
    },
    {
      ""iconName"": ""ruble-sign"",
      ""itemId"": ""5449016a4bdc2d6f028b456f"",
      ""mainCurrency"": true,
      ""name"": ""RUB""
    }
  ],
  ""heavyEncumbermentWeight"": 65,
  ""lightEncumbermentWeight"": 24,
  ""merchants"": [
    {
      ""name"": ""prapor"",
      ""minLevel"": 1,
      ""maxLevel"": 4,
      ""showInFilter"": true
    },
    {
      ""name"": ""therapist"",
      ""minLevel"": 1,
      ""maxLevel"": 4,
      ""showInFilter"": true
    },
    {
      ""name"": ""skier"",
      ""minLevel"": 1,
      ""maxLevel"": 4,
      ""showInFilter"": true
    },
    {
      ""name"": ""fence"",
      ""minLevel"": 1,
      ""maxLevel"": 1,
      ""showInFilter"": false
    },
    {
      ""name"": ""peacekeeper"",
      ""minLevel"": 1,
      ""maxLevel"": 4,
      ""showInFilter"": true
    },
    {
      ""name"": ""mechanic"",
      ""minLevel"": 1,
      ""maxLevel"": 4,
      ""showInFilter"": true
    },
    {
      ""name"": ""ragman"",
      ""minLevel"": 1,
      ""maxLevel"": 4,
      ""showInFilter"": true
    },
    {
      ""name"": ""jaeger"",
      ""minLevel"": 1,
      ""maxLevel"": 4,
      ""showInFilter"": true
    },
    {
      ""name"": ""flea-market"",
      ""minLevel"": 0,
      ""maxLevel"": 0,
      ""showInFilter"": true
    },
    {
      ""name"": ""items-without-merchant"",
      ""minLevel"": 0,
      ""maxLevel"": 0,
      ""showInFilter"": true
    }
  ],
  ""ricochetChances"": [
    {
      ""name"": ""High"",
      ""xMinValue"": 0.9,
      ""xMaxValue"": 1
    },
    {
      ""name"": ""Medium"",
      ""xMinValue"": 0.7,
      ""xMaxValue"": 0.8
    },
    {
      ""name"": ""Low"",
      ""xMinValue"": 0.1,
      ""xMaxValue"": 0.6
    }
  ]
}";
    }
}
