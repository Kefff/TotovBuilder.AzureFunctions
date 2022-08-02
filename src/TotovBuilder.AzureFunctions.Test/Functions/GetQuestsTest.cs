using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.Model;
using TotovBuilder.AzureFunctions.Test.Mocks;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetQuests"/> class.
    /// </summary>
    public class GetQuestsTest
    {
        [Fact]
        public async Task Run_ShouldFetchData()
        {
            // Arrange
            Mock<IQuestsFetcher> questsFetcherMock = new Mock<IQuestsFetcher>();
            questsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Quest>?>(TestData.Quests));

            GetQuests function = new GetQuests(questsFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().Be(TestData.Quests);
        }

        [Fact]
        public async Task Run_WithoutData_ShouldReturnEmptyResponse()
        {
            // Arrange
            Mock<IQuestsFetcher> questsFetcherMock = new Mock<IQuestsFetcher>();
            questsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Quest>?>(null));

            GetQuests function = new GetQuests(questsFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(Array.Empty<Quest>());
        }
    }
}
