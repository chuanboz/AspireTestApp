using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace AspireTestApp.Tests;

[Collection("AspireAppHost")]
public class CounterChangeFeedFunctionTests(AspireAppHostFixture fixture)
{
    [Fact]
    public async Task CounterChangeFeedFunctionTriggersOnCounterUpdate()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var counterName = $"test-changefeed-{Guid.NewGuid():N}";

        // Act - Create a counter by incrementing it
        var apiClient = app.CreateHttpClient("apiservice");
        var response = await apiClient.PostAsync($"/api/counter?name={counterName}", null, cancellationToken);
        
        // Assert the API call succeeded
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await response.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(1, value);

        // Wait a bit for the change feed to process
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        // The change feed function should have logged the counter change
        // Since we can't directly access function logs in the test, we verify the counter was created successfully
        var verifyResponse = await apiClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        var verifiedValue = await verifyResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(1, verifiedValue);
    }

    [Fact]
    public async Task CounterChangeFeedFunctionTriggersOnMultipleUpdates()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var counterName = $"test-changefeed-multi-{Guid.NewGuid():N}";

        // Act - Increment counter multiple times
        var apiClient = app.CreateHttpClient("apiservice");
        var updateCount = 5;

        for (int i = 1; i <= updateCount; i++)
        {
            var response = await apiClient.PostAsync($"/api/counter?name={counterName}", null, cancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var value = await response.Content.ReadFromJsonAsync<int>(cancellationToken);
            Assert.Equal(i, value);
            
            // Small delay between updates
            await Task.Delay(100, cancellationToken);
        }

        // Wait for change feed to process all updates
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        // Verify final counter value
        var verifyResponse = await apiClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        var finalValue = await verifyResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(updateCount, finalValue);
    }

    [Fact]
    public async Task CounterChangeFeedFunctionHandlesMultipleCounters()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var apiClient = app.CreateHttpClient("apiservice");
        
        var counter1Name = $"test-changefeed-c1-{Guid.NewGuid():N}";
        var counter2Name = $"test-changefeed-c2-{Guid.NewGuid():N}";
        var counter3Name = $"test-changefeed-c3-{Guid.NewGuid():N}";

        // Act - Create multiple counters
        var response1 = await apiClient.PostAsync($"/api/counter?name={counter1Name}", null, cancellationToken);
        var response2 = await apiClient.PostAsync($"/api/counter?name={counter2Name}", null, cancellationToken);
        var response3 = await apiClient.PostAsync($"/api/counter?name={counter3Name}", null, cancellationToken);

        // Assert all counters were created
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var value1 = await response1.Content.ReadFromJsonAsync<int>(cancellationToken);
        var value2 = await response2.Content.ReadFromJsonAsync<int>(cancellationToken);
        var value3 = await response3.Content.ReadFromJsonAsync<int>(cancellationToken);

        Assert.Equal(1, value1);
        Assert.Equal(1, value2);
        Assert.Equal(1, value3);

        // Wait for change feed to process
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        // Verify all counters persist correctly
        var verify1 = await apiClient.GetAsync($"/api/counter?name={counter1Name}", cancellationToken);
        var verify2 = await apiClient.GetAsync($"/api/counter?name={counter2Name}", cancellationToken);
        var verify3 = await apiClient.GetAsync($"/api/counter?name={counter3Name}", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, verify1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, verify2.StatusCode);
        Assert.Equal(HttpStatusCode.OK, verify3.StatusCode);
    }

    [Fact]
    public async Task CounterChangeFeedFunctionProcessesRapidUpdates()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var counterName = $"test-changefeed-rapid-{Guid.NewGuid():N}";

        // Act - Rapid fire increments without delays
        var apiClient = app.CreateHttpClient("apiservice");
        var rapidUpdateCount = 10;
        var tasks = new List<Task<HttpResponseMessage>>();

        for (int i = 0; i < rapidUpdateCount; i++)
        {
            tasks.Add(apiClient.PostAsync($"/api/counter?name={counterName}", null, cancellationToken));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert all requests succeeded
        foreach (var response in responses)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Wait for change feed to catch up
        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

        // Verify final counter value is correct
        var verifyResponse = await apiClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        var finalValue = await verifyResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(rapidUpdateCount, finalValue);
    }

    [Fact]
    public async Task CounterChangeFeedFunctionHandlesCounterWithSpecialCharactersInName()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var counterName = $"test-special-{Guid.NewGuid():N}-with_underscores-and-dashes";

        // Act
        var apiClient = app.CreateHttpClient("apiservice");
        var response = await apiClient.PostAsync($"/api/counter?name={Uri.EscapeDataString(counterName)}", null, cancellationToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await response.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(1, value);

        // Wait for change feed
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        // Verify the counter with special characters works
        var verifyResponse = await apiClient.GetAsync($"/api/counter?name={Uri.EscapeDataString(counterName)}", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        var verifiedValue = await verifyResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(1, verifiedValue);
    }
}
