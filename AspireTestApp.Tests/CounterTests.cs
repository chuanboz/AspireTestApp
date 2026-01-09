using System.Net.Http.Json;
using AspireTestApp.Models;
using Microsoft.Extensions.Logging;

namespace AspireTestApp.Tests;

[Collection("AspireAppHost")]
public class CounterTests(AspireAppHostFixture fixture)
{
    [Fact]
    public async Task GetCounterReturnsInitialValue()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/api/counter", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var counter = await response.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.True(counter >= 0);
    }

    [Fact]
    public async Task IncrementCounterReturnsIncrementedValue()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        
        var getResponse = await httpClient.GetAsync("/api/counter", cancellationToken);
        var initialValue = await getResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        
        var postResponse = await httpClient.PostAsync("/api/counter", null, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        var newValue = await postResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(initialValue + 1, newValue);
    }

    [Fact]
    public async Task IncrementCounterMultipleTimesIncreasesValue()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        
        var incrementCount = 3;
        var values = new List<int>();
        
        for (int i = 0; i < incrementCount; i++)
        {
            // Get current value
            var getResponse = await httpClient.GetAsync("/api/counter", cancellationToken);
            var currentValue = await getResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
            
            // Increment
            var postResponse = await httpClient.PostAsync("/api/counter", null, cancellationToken);
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
            var newValue = await postResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
            
            // Verify increment
            Assert.Equal(currentValue + 1, newValue);
            values.Add(newValue);
            
            // Small delay
            await Task.Delay(10, cancellationToken);
        }

        // Assert - Verify values are increasing
        for (int i = 1; i < values.Count; i++)
        {
            Assert.True(values[i] > values[i - 1], 
                $"Value at index {i} should be greater than previous value");
        }
    }

    [Fact]
    public async Task CounterValuePersistedAcrossRequests()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        
        var postResponse = await httpClient.PostAsync("/api/counter", null, cancellationToken);
        var valueAfterIncrement = await postResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        
        var getResponse = await httpClient.GetAsync("/api/counter", cancellationToken);
        var valueFromGet = await getResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

        // Assert
        Assert.Equal(valueAfterIncrement, valueFromGet);
    }
}
