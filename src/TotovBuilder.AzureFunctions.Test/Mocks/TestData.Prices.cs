using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    /// <summary>
    /// Represents test data.
    /// </summary>
    public static partial class TestData
    {
        public static Item[] Prices = new Item[]
        {
            new Item()
            {
                Id = "5447a9cd4bdc2dbd208b4567",
                Prices = new Price[]
                {
                    new Price()
                    {
                        CurrencyName = "RUB",
                        Merchant = "mechanic",
                        MerchantLevel = 3,
                        QuestId = "5ae327c886f7745c7b3f2f3f",
                        Value = 67809,
                        ValueInMainCurrency = 67809
                    },
                    new Price()
                    {
                        CurrencyName = "USD",
                        Merchant = "peacekeeper",
                        MerchantLevel = 2,
                        Value = 915,
                        ValueInMainCurrency = 96990
                    },
                    new Price()
                    {
                        CurrencyName = "RUB",
                        Merchant = "flea-market",
                        Value = 85164,
                        ValueInMainCurrency = 85164
                    }
                }
            },
            new Item()
            {
                Id = "5448ba0b4bdc2d02308b456c",
                Prices = new Price[]
                {                    
                    new Price()
                    {
                        CurrencyName = "RUB",
                        Merchant = "flea-market",
                        Value = 87576,
                        ValueInMainCurrency = 87576
                    }
                }
            }
        };

        public const string PricesJson = @"{
  ""data"": {
    ""items"": [
      {
        ""id"": ""5447a9cd4bdc2dbd208b4567"",
        ""buyFor"": [
          {
            ""vendor"": {
              ""trader"": {
                ""normalizedName"": ""mechanic""
              },
              ""minTraderLevel"": 3,
              ""taskUnlock"": {
                ""id"": ""5ae327c886f7745c7b3f2f3f""
              }
            },
            ""price"": 67809,
            ""currency"": ""RUB"",
            ""priceRUB"": 67809
          },
          {
            ""vendor"": {
              ""trader"": {
                ""normalizedName"": ""peacekeeper""
              },
              ""minTraderLevel"": 2,
              ""taskUnlock"": null
            },
            ""price"": 915,
            ""currency"": ""USD"",
            ""priceRUB"": 96990
          },
          {
            ""vendor"": {
              ""normalizedName"": ""flea-market""
            },
            ""price"": 85164,
            ""currency"": ""RUB"",
            ""priceRUB"": 85164
          }
        ]
      },
      {
        ""id"": ""5448ba0b4bdc2d02308b456c"",
        ""buyFor"": [
          {
            ""vendor"": {
              ""normalizedName"": ""flea-market""
            },
            ""price"": 87576,
            ""currency"": ""RUB"",
            ""priceRUB"": 87576
          }
        ]
      }
    ]
  }
}";
    }
}
