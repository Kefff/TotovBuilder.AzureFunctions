using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.Model.Builds;
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
            Mock<IPresetsFetcher> presetsFetcherMock = new Mock<IPresetsFetcher>();
            presetsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<InventoryItem>?>(TestData.Presets));

            GetPresets function = new GetPresets(presetsFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().Be(TestData.Presets);
        }

        [Fact]
        public async Task Run_WithoutData_ShouldReturnEmptyResponse()
        {
            // Arrange
            Mock<IPresetsFetcher> presetsFetcherMock = new Mock<IPresetsFetcher>();
            presetsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<InventoryItem>?>(null));

            GetPresets function = new GetPresets(presetsFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(Array.Empty<InventoryItem>());
        }
    }
}
