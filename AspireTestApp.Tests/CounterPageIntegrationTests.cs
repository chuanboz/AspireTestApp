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

        // Act - Get initial counter value from API
        var initialResponse = await apiClient.GetAsync("/api/counter", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, initialResponse.StatusCode);
        var initialValue = await initialResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

        // Increment the counter via API
        var incrementResponse = await apiClient.PostAsync("/api/counter", null, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, incrementResponse.StatusCode);
        var newValue = await incrementResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

        // Verify the counter was incremented
        Assert.Equal(initialValue + 1, newValue);

        // Verify the new value persists
        var verifyResponse = await apiClient.GetAsync("/api/counter", cancellationToken);
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

        // Act
        var response = await apiClient.GetAsync("/api/counter/status", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        // Verify the status contains expected fields
        Assert.Contains("\"exists\":", content);
        Assert.Contains("\"id\":", content);
        Assert.Contains("\"value\":", content);
    }

    [Fact]
    public async Task SingleIncrementWorks()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var apiClient = app.CreateHttpClient("apiservice");

        // Act - Get current value
        var getCurrentResponse = await apiClient.GetAsync("/api/counter", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, getCurrentResponse.StatusCode);
        var currentValue = await getCurrentResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

        // Increment once
        var incrementResponse = await apiClient.PostAsync("/api/counter", null, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, incrementResponse.StatusCode);
        var newValue = await incrementResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

        // Assert - New value should be exactly current + 1
        Assert.Equal(currentValue + 1, newValue);

        // Verify it persisted
        var verifyResponse = await apiClient.GetAsync("/api/counter", cancellationToken);
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

        // Act - Increment multiple times sequentially and verify each step
        var incrementCount = 10;
        var values = new List<int>();

        for (int i = 0; i < incrementCount; i++)
        {
            // Get current value before increment
            var beforeResponse = await apiClient.GetAsync("/api/counter", cancellationToken);
            Assert.Equal(HttpStatusCode.OK, beforeResponse.StatusCode);
            var beforeValue = await beforeResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

            // Increment
            var incrementResponse = await apiClient.PostAsync("/api/counter", null, cancellationToken);
            Assert.Equal(HttpStatusCode.OK, incrementResponse.StatusCode);
            var afterValue = await incrementResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

            // Verify the increment worked
            Assert.Equal(beforeValue + 1, afterValue);
            
            values.Add(afterValue);

            // Small delay to avoid race conditions
            await Task.Delay(10, cancellationToken);
        }

        // Assert - Verify we got incrementing values
        for (int i = 1; i < values.Count; i++)
        {
            Assert.True(values[i] > values[i - 1], 
                $"Value at index {i} ({values[i]}) should be greater than value at index {i-1} ({values[i - 1]})");
        }

        // Verify final value is persisted
        var finalResponse = await apiClient.GetAsync("/api/counter", cancellationToken);
        var finalValue = await finalResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(values[^1], finalValue);
    }
}
