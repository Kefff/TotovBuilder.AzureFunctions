using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.AzureFunctions.Test.Mocks;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetPresets"/> class.
    /// </summary>
    public class GetPresetsTest
    {
        [Fact]
        public async Task Run_ShouldFetchData()
        {
            // Arrange
            Mock<IDataFetcher> dataFetcherMock = new Mock<IDataFetcher>();
            dataFetcherMock.Setup(m => m.Fetch(DataType.Presets)).Returns(Task.FromResult(TestData.Presets));

            GetPresets function = new GetPresets(dataFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().Be(TestData.Presets);
        }
    }
}
