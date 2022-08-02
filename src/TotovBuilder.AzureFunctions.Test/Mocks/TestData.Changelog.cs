using System;
using TotovBuilder.Model;

namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    /// <summary>
    /// Represents test data.
    /// </summary>
    public static partial class TestData
    {
        public static ChangelogEntry[] Changelog = new ChangelogEntry[]
        {
            new ChangelogEntry()
            {
                Changes = new ChangelogChange[]
                {
                    new ChangelogChange()
                    {
                        Language = "en",
                        Text = "Added a thing."
                    },
                    new ChangelogChange()
                    {
                        Language = "fr",
                        Text = "Ajout d'une chose."
                    }
                },
                Date = new DateTime(2022, 1, 2),
                Version = "1.1.0",
            },
            new ChangelogEntry()
            {
                Changes = new ChangelogChange[]
                {
                    new ChangelogChange()
                    {
                        Language = "en",
                        Text = "Initial release."
                    },
                    new ChangelogChange()
                    {
                        Language = "fr",
                        Text = "Sortie initiale."
                    }
                },
                Date = new DateTime(2022, 1, 1),
                Version = "1.0.0"
            }
        };

        public const string ChangelogJson = @"[
    {
      ""version"": ""1.1.0"",
      ""date"": ""2022-01-02T00:00:00+01:00"",
      ""changes"": [
        {
          ""language"": ""en"",
          ""text"": ""Added a thing.""
        },
        {
          ""language"": ""fr"",
          ""text"": ""Ajout d'une chose.""
        }
      ]
    },
    {
      ""version"": ""1.0.0"",
      ""date"": ""2022-01-01T00:00:00+01:00"",
      ""changes"": [
        {
          ""language"": ""en"",
          ""text"": ""Initial release.""
        },
        {
          ""language"": ""fr"",
          ""text"": ""Sortie initiale.""
        }
      ]
    }
]";
    }
}
