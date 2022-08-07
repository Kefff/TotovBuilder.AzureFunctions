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
using Xunit;
using TotovBuilder.Model.Test;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetArmorPenetrations"/> class.
    /// </summary>
    public class GetArmorPenetrationsTest
    {
        [Fact]
        public async Task Run_ShouldFetchData()
        {
            // Arrange
            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new Mock<IArmorPenetrationsFetcher>();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>?>(TestData.ArmorPenetrations));

            GetArmorPenetrations function = new GetArmorPenetrations(armorPenetrationsFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(TestData.ArmorPenetrations);
        }

        [Fact]
        public async Task Run_WithoutData_ShouldReturnEmptyResponse()
        {
            // Arrange
            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new Mock<IArmorPenetrationsFetcher>();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>?>(null));

            GetArmorPenetrations function = new GetArmorPenetrations(armorPenetrationsFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(Array.Empty<ArmorPenetration>());
        }
    }
}
