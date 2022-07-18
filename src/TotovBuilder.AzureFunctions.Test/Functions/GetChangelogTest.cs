using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.AzureFunctions.Models;
using TotovBuilder.AzureFunctions.Test.Mocks;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetChangelog"/> class.
    /// </summary>
    public class GetChangelogTest
    {
        [Fact]
        public async Task Run_ShouldFetchData()
        {
            // Arrange
            Mock<IChangelogFetcher> changelogFetcherMock = new Mock<IChangelogFetcher>();
            changelogFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<ChangelogEntry[]?>(TestData.Changelog));

            GetChangelog function = new GetChangelog(changelogFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().Be(TestData.Changelog);
        }

        [Fact]
        public async Task Run_WithoutData_ShouldReturnEmptyResponse()
        {
            // Arrange
            Mock<IChangelogFetcher> changelogFetcherMock = new Mock<IChangelogFetcher>();
            changelogFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<ChangelogEntry[]?>(null));

            GetChangelog function = new GetChangelog(changelogFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(Array.Empty<ChangelogEntry>());
        }
    }
}
