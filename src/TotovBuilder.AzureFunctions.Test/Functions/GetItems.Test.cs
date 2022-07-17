//using System.Threading.Tasks;
//using FluentAssertions;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
//using TotovBuilder.AzureFunctions.Functions;
//using TotovBuilder.AzureFunctions.Models;
//using TotovBuilder.AzureFunctions.Test.Mocks;
//using Xunit;

//namespace TotovBuilder.AzureFunctions.Test.Functions
//{
//    /// <summary>
//    /// Represents tests on the <see cref="GetItems"/> class.
//    /// </summary>
//    public class GetItemsTest
//    {
//        [Fact]
//        public async Task Run_ShouldFetchData()
//        {
//            // Arrange
//            Mock<IItemsFetcher> itemsFetcherMock = new Mock<IItemsFetcher>();
//            itemsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<Item[]>(TestData.Items));

//            GetItems function = new GetItems(itemsFetcherMock.Object);

//            // Act
//            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

//            // Assert
//            result.Should().BeOfType<OkObjectResult>();
//            ((OkObjectResult)result).Value.Should().Be(TestData.Quests);
//        }
//    }
//}
