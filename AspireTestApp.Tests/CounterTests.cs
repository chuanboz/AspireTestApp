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
        var counterName = $"test-get-initial-{Guid.NewGuid():N}";

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var counter = await response.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(0, counter); // New counter should start at 0
    }

    [Fact]
    public async Task IncrementCounterReturnsIncrementedValue()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var counterName = $"test-increment-{Guid.NewGuid():N}";

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        
        var getResponse = await httpClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);
        var initialValue = await getResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        
        var postResponse = await httpClient.PostAsync($"/api/counter?name={counterName}", null, cancellationToken);

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
        var counterName = $"test-multiple-{Guid.NewGuid():N}";

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        
        var incrementCount = 3;
        var values = new List<int>();
        
        for (int i = 0; i < incrementCount; i++)
        {
            // Get current value
            var getResponse = await httpClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);
            var currentValue = await getResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
            
            // Increment
            var postResponse = await httpClient.PostAsync($"/api/counter?name={counterName}", null, cancellationToken);
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
            var newValue = await postResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
            
            // Verify increment
            Assert.Equal(currentValue + 1, newValue);
            values.Add(newValue);
        }

        // Assert - Verify values are 1, 2, 3 (since this is a new counter)
        Assert.Equal(1, values[0]);
        Assert.Equal(2, values[1]);
        Assert.Equal(3, values[2]);
    }

    [Fact]
    public async Task CounterValuePersistedAcrossRequests()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var counterName = $"test-persist-{Guid.NewGuid():N}";

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        
        var postResponse = await httpClient.PostAsync($"/api/counter?name={counterName}", null, cancellationToken);
        var valueAfterIncrement = await postResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        
        var getResponse = await httpClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);
        var valueFromGet = await getResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

        // Assert
        Assert.Equal(valueAfterIncrement, valueFromGet);
    }
}
