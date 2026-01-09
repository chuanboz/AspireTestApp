using System.Net.Http.Json;

namespace AspireTestApp.Tests;

[Collection("AspireAppHost")]
public class CounterPageIntegrationTests(AspireAppHostFixture fixture)
{
    [Fact]
    public async Task CounterPageLoadsInitialValueFromDatabase()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var webClient = app.CreateHttpClient("webfrontend");
        var response = await webClient.GetAsync("/counter", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        // Verify the page contains counter elements
        Assert.Contains("Counter", content);
        Assert.Contains("Current count:", content);
        Assert.Contains("Click me", content);
    }

    [Fact]
    public async Task CounterIncrementFlowWorksEndToEnd()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var apiClient = app.CreateHttpClient("apiservice");
        var counterName = $"test-e2e-{Guid.NewGuid():N}";

        // Act - Get initial counter value from API
        var initialResponse = await apiClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, initialResponse.StatusCode);
        var initialValue = await initialResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

        // Increment the counter via API
        var incrementResponse = await apiClient.PostAsync($"/api/counter?name={counterName}", null, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, incrementResponse.StatusCode);
        var newValue = await incrementResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

        // Verify the counter was incremented
        Assert.Equal(initialValue + 1, newValue);

        // Verify the new value persists
        var verifyResponse = await apiClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);
        var persistedValue = await verifyResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(newValue, persistedValue);
    }

    [Fact]
    public async Task CounterStatusEndpointReturnsCorrectData()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var apiClient = app.CreateHttpClient("apiservice");
        var counterName = $"test-status-{Guid.NewGuid():N}";

        // Create counter first
        await apiClient.PostAsync($"/api/counter?name={counterName}", null, cancellationToken);

        // Act
        var response = await apiClient.GetAsync($"/api/counter/status?name={counterName}", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        // Verify the status contains expected fields
        Assert.Contains("\"exists\":", content);
        Assert.Contains("\"id\":", content);
        Assert.Contains("\"name\":", content);
        Assert.Contains("\"value\":", content);
    }

    [Fact]
    public async Task SingleIncrementWorks()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var apiClient = app.CreateHttpClient("apiservice");
        var counterName = $"test-single-{Guid.NewGuid():N}";

        // Act - Get current value (should be 0 for new counter)
        var getCurrentResponse = await apiClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, getCurrentResponse.StatusCode);
        var currentValue = await getCurrentResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(0, currentValue);

        // Increment once
        var incrementResponse = await apiClient.PostAsync($"/api/counter?name={counterName}", null, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, incrementResponse.StatusCode);
        var newValue = await incrementResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

        // Assert - New value should be exactly current + 1
        Assert.Equal(1, newValue);

        // Verify it persisted
        var verifyResponse = await apiClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);
        var verifiedValue = await verifyResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(newValue, verifiedValue);
    }

    [Fact]
    public async Task MultipleConsecutiveIncrementsWork()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var apiClient = app.CreateHttpClient("apiservice");
        var counterName = $"test-consecutive-{Guid.NewGuid():N}";

        // Act - Increment multiple times sequentially and verify each step
        var incrementCount = 10;

        for (int i = 1; i <= incrementCount; i++)
        {
            // Increment
            var incrementResponse = await apiClient.PostAsync($"/api/counter?name={counterName}", null, cancellationToken);
            Assert.Equal(HttpStatusCode.OK, incrementResponse.StatusCode);
            var afterValue = await incrementResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

            // Verify the increment worked - should equal the iteration number
            Assert.Equal(i, afterValue);
        }

        // Verify final value is persisted correctly
        var finalResponse = await apiClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);
        var finalValue = await finalResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(incrementCount, finalValue);
    }
}
