using TotovBuilder.Model;

namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    /// <summary>
    /// Represents test data.
    /// </summary>
    public static partial class TestData
    {
        public static Quest[] Quests = new Quest[]
        {
            new Quest()
            {
                Id = "59675d6c86f7740a842fc482",
                Name = "Ice Cream Cones",
                Merchant = "prapor",
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/Ice_Cream_Cones"
            },
            new Quest()
            {
                Id = "5a27bb1e86f7741f27621b7e",
                Name = "Cargo X - Part 1",
                Merchant = "prapor",
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/Cargo_X_-_Part_1"
            }
        };

        public const string QuestsJson = @"{
  ""data"": {
    ""tasks"": [
      {
        ""id"": ""59675d6c86f7740a842fc482"",
        ""name"": ""Ice Cream Cones"",
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Ice_Cream_Cones"",
        ""trader"": {
          ""normalizedName"": ""prapor""
        }
      },
      {
        ""id"": ""5a27bb1e86f7741f27621b7e"",
        ""name"": ""Cargo X - Part 1"",
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Cargo_X_-_Part_1"",
        ""trader"": {
          ""normalizedName"": ""prapor""
        }
      }
    ]
  }
}";
    }
}
