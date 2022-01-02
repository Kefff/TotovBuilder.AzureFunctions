using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.AzureFunctions.Abstractions;
using Xunit;
using TotovBuilder.AzureFunctions.Test.Mocks;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetMarketData"/> class.
    /// </summary>
    public class GetMarketDataTest
    {
        [Fact]
        public async Task Run_ShouldFetchData()
        {
            // Arrange
            Mock<IDataFetcher> dataFetcherMock = new Mock<IDataFetcher>();
            dataFetcherMock.Setup(m => m.Fetch(DataType.MarketData)).Returns(Task.FromResult(TestData.MarketDataItemsOnly));

            GetMarketData function = new GetMarketData(dataFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().Be(TestData.MarketDataItemsOnly);
        }
    }
}
