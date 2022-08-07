using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.Model;
using Xunit;
using TotovBuilder.Model.Test;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetTarkovValues"/> class.
    /// </summary>
    public class GetTarkovValuesTest
    {
        [Fact]
        public async Task Run_ShouldFetchData()
        {
            // Arrange
            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new Mock<ITarkovValuesFetcher>();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<TarkovValues?>(TestData.TarkovValues));

            GetTarkovValues function = new GetTarkovValues(tarkovValuesFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(TestData.TarkovValues);
        }

        [Fact]
        public async Task Run_WithoutData_ShouldReturnEmptyResponse()
        {
            // Arrange
            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new Mock<ITarkovValuesFetcher>();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<TarkovValues?>(null));

            GetTarkovValues function = new GetTarkovValues(tarkovValuesFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(new TarkovValues());
        }
    }
}
