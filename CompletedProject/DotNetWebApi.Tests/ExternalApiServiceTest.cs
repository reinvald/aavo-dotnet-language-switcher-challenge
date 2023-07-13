using Xunit;
using DotNetWebApi.Services;

namespace DotNetWebApi.Tests;

public class ExternalApiServiceTest
{
    [Fact]
    public void TestTwoIsEven()
    {
        // instantiating a ExternalApiService requires an injected HttpClient, 
        // so, just provide one
        var externalApiService = new ExternalApiService(new HttpClient());
        Assert.True(externalApiService.IsEven(2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void testMultipleIsEven(int value)
    {
        var externalApiService = new ExternalApiService(new HttpClient());
        Assert.True(externalApiService.IsEven(value));
    }
}