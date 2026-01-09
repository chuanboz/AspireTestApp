using Microsoft.Extensions.Logging;

namespace AspireTestApp.Tests;

[Collection("AspireAppHost")]
public class WebTests(AspireAppHostFixture fixture)
{
    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("webfrontend");
        var response = await httpClient.GetAsync("/", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCounterPageReturnsOkStatusCode()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("webfrontend");
        var response = await httpClient.GetAsync("/counter", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        Assert.Contains("Counter", content);
    }

    [Fact]
    public async Task GetWeatherPageReturnsOkStatusCode()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("webfrontend");
        var response = await httpClient.GetAsync("/weather", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        Assert.Contains("Weather", content);
    }

    [Fact]
    public async Task WebFrontendCanReachApiService()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var webClient = app.CreateHttpClient("webfrontend");
        var apiClient = app.CreateHttpClient("apiservice");
        
        var webResponse = await webClient.GetAsync("/counter", cancellationToken);
        var apiResponse = await apiClient.GetAsync("/api/counter", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, webResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
    }
}
