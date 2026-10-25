using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
using TotovBuilder.Model.Utils;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="QuestsFetcher"/> class.
    /// </summary>
    public class QuestsFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldFetchTranslatedQuests()
        {
            // Arrange
            QuestsFetcher fetcher = CreateFetcher(ApiRequesterMock.Create(
                new Dictionary<string, string>()
                {
                    { "/regular/tasks", TestData.TasksJson }
                },
                new Dictionary<string, string>()
                {
                    { "/regular/tasks_fr", @"{ ""data"": { ""5a27b80086f774429a5d7e20 name"": ""Oeil d'aigle"" } }" }
                }));

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.GetQuest("5a27b80086f774429a5d7e20").Should().BeEquivalentTo(new Quest()
            {
                Id = "5a27b80086f774429a5d7e20",
                Name = "Oeil d'aigle",
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/Eagle_Eye"
            });
        }

        [Fact]
        public async Task GetQuest_WithUnknownQuest_ShouldThrow()
        {
            // Arrange
            QuestsFetcher fetcher = CreateFetcher(ApiRequesterMock.CreateForPricesAndBarters());
            await fetcher.Fetch();

            // Act
            Action act = () => fetcher.GetQuest("unknown-quest");

            // Assert
            act.Should().Throw<InvalidDataException>().WithMessage("Quest \"unknown-quest\" not found.");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Fetch_WithFailedOrInvalidTasksFile_ShouldFail(bool isFileFound)
        {
            // Arrange
            Dictionary<string, string> dataJsonByPath = [];

            if (isFileFound)
            {
                dataJsonByPath.Add("/regular/tasks", @"{ ""data"": { ""invalid"": {} } }");
            }

            QuestsFetcher fetcher = CreateFetcher(ApiRequesterMock.Create(
                dataJsonByPath,
                new Dictionary<string, string>()
                {
                    { "/regular/tasks_fr", TestData.TasksTranslationsJson }
                }));

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Quests - No data fetched.");
        }

        private static QuestsFetcher CreateFetcher(Mock<IApiRequester> apiRequesterMock)
        {
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration);

            return new QuestsFetcher(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                "fr",
                new Mock<ILogger<QuestsFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object);
        }
    }
}
