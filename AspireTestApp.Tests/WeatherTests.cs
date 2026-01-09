using System.Net.Http.Json;
using AspireTestApp.Models;
using Microsoft.Extensions.Logging;

namespace AspireTestApp.Tests;

[Collection("AspireAppHost")]
public class WeatherTests(AspireAppHostFixture fixture)
{
    [Fact]
    public async Task GetWeatherForecastReturnsData()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/api/weather?days=5", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>(cancellationToken);
        Assert.NotNull(forecasts);
        Assert.NotEmpty(forecasts);
        Assert.True(forecasts.Length <= 5);
    }

    [Fact]
    public async Task GetWeatherForecastReturnsValidData()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/api/weather?days=3", cancellationToken);
        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>(cancellationToken);

        // Assert
        Assert.NotNull(forecasts);
        foreach (var forecast in forecasts)
        {
            Assert.NotNull(forecast.Id);
            Assert.NotEmpty(forecast.Id);
            Assert.Equal("weather", forecast.PartitionKey);
            Assert.InRange(forecast.TemperatureC, -20, 55);
            Assert.NotNull(forecast.Summary);
            Assert.Equal(32 + (int)(forecast.TemperatureC / 0.5556), forecast.TemperatureF);
        }
    }

    [Fact]
    public async Task GenerateWeatherForecastCreatesNewData()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.PostAsync("/api/weather/generate?days=3", null, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>(cancellationToken);
        Assert.NotNull(forecasts);
        Assert.Equal(3, forecasts.Length);
    }

    [Fact]
    public async Task GetWeatherForecastWithCustomDaysParameter()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        var requestedDays = 7;
        var response = await httpClient.GetAsync($"/api/weather?days={requestedDays}", cancellationToken);
        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>(cancellationToken);

        // Assert
        Assert.NotNull(forecasts);
        Assert.True(forecasts.Length <= requestedDays);
    }

    [Fact]
    public async Task WeatherForecastHasFutureDates()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.PostAsync("/api/weather/generate?days=5", null, cancellationToken);
        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>(cancellationToken);

        // Assert
        Assert.NotNull(forecasts);
        var today = DateOnly.FromDateTime(DateTime.Now);
        
        foreach (var forecast in forecasts)
        {
            Assert.True(forecast.Date > today, $"Forecast date {forecast.Date} should be in the future (after {today})");
        }
    }
}
