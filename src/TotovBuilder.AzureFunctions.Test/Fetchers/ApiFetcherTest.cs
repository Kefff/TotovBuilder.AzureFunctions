using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.AzureFunctions.Utils;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="ApiFetcher{T}"/> class.
    /// </summary>
    public class ApiFetcherTest
    {
        [Theory]
        [InlineData("/{0}/items", "regular", null, "/regular/items")]
        [InlineData("/{0}/items_{1}", "pve", "fr", "/pve/items_fr")]
        [InlineData("/pvp-season/season", "pvp-season", null, "/pvp-season/season")]
        public void BuildApiPath_ShouldBuildPath(string pathTemplate, string gameModeApiValue, string? language, string expected)
        {
            // Act
            string path = FetcherImplementation.TestBuildApiPath(pathTemplate, gameModeApiValue, language);

            // Assert
            path.Should().Be(expected);
        }

        [Fact]
        public async Task Fetch_ShouldFetchData()
        {
            // Arrange
            FetcherImplementation fetcher = new(() => Task.FromResult(Result.Ok("data")));

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().Be("data");
            fetcher.ExecutionsCount.Should().Be(1);
        }

        [Fact]
        public async Task Fetch_WithAlreadyFetchedData_ShouldNotFetchDataAgain()
        {
            // Arrange
            FetcherImplementation fetcher = new(() => Task.FromResult(Result.Ok("data")));

            // Act
            await fetcher.Fetch();
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.ExecutionsCount.Should().Be(1);
        }

        [Fact]
        public async Task Fetch_WithConcurrentCalls_ShouldFetchDataOnce()
        {
            // Arrange
            FetcherImplementation fetcher = new(async () =>
            {
                await Task.Delay(500);

                return Result.Ok("data");
            });

            // Act
            Result[] results = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => Task.Run(fetcher.Fetch)));

            // Assert
            results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
            fetcher.ExecutionsCount.Should().Be(1);
        }

        [Fact]
        public async Task Fetch_WithFailedFetching_ShouldFailAndFetchAgainOnNextCall()
        {
            // Arrange
            FetcherImplementation fetcher = new(() => Task.FromResult(Result.Fail<string>("Error")));

            // Act
            Result firstResult = await fetcher.Fetch();
            Result secondResult = await fetcher.Fetch();

            // Assert
            firstResult.IsSuccess.Should().BeFalse();
            firstResult.Errors.Single().Message.Should().Be("Items - No data fetched.");
            secondResult.IsSuccess.Should().BeFalse();
            fetcher.FetchedData.Should().BeNull();
            fetcher.ExecutionsCount.Should().Be(2);
        }

        private class FetcherImplementation : ApiFetcher<string>
        {
            public int ExecutionsCount { get; private set; }

            protected override DataType DataType
            {
                get
                {
                    return DataType.Items;
                }
            }

            private readonly System.Func<Task<Result<string>>> Execute;

            public FetcherImplementation(System.Func<Task<Result<string>>> execute)
                : base(new Mock<ILogger>().Object)
            {
                Execute = execute;
            }

            public static string TestBuildApiPath(string pathTemplate, string gameModeApiValue, string? language)
            {
                string path = BuildApiPath(pathTemplate, gameModeApiValue, language);

                return path;
            }

            protected override Task<Result<string>> ExecuteFetch()
            {
                ExecutionsCount++;

                return Execute();
            }
        }
    }
}
