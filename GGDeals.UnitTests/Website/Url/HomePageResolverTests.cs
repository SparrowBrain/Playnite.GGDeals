using GGDeals.Website.Url;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Website.Url
{
    public class HomePageResolverTests
    {
        [Theory]
        [AutoMoqData]
        public void Resolve(HomePageResolver sut)
        {
            // Act
            var actual = sut.Resolve();

            // Assert
            Assert.Equal("https://gg.deals", actual);
        }
    }
}