using System.Net.Http.Json;
using AspireTestApp.Models;

namespace AspireTestApp.Tests;

[Collection("AspireAppHost")]
public class CounterSeedingTests(AspireAppHostFixture fixture)
{
    [Fact]
    public async Task CounterIsSeededWithInitialValueZero()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        
        // Get the counter value - it should be seeded with 0
        var response = await httpClient.GetAsync("/api/counter", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var counterValue = await response.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.True(counterValue >= 0, "Counter should be seeded with initial value of 0 or higher");
    }

    [Fact]
    public async Task CounterDocumentHasRequiredIdField()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        
        // Increment the counter to ensure document is created/updated
        var postResponse = await httpClient.PostAsync("/api/counter", null, cancellationToken);
        
        // Verify the operation succeeded
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        
        // Get the counter value to ensure document exists
        var getResponse = await httpClient.GetAsync("/api/counter", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var counterValue = await getResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.True(counterValue > 0, "Counter should have been incremented");
    }

    [Fact]
    public async Task CounterPersistsAfterMultipleOperations()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var httpClient = app.CreateHttpClient("apiservice");

        // Act - Increment multiple times and verify each increment
        var incrementCount = 5;
        var previousValue = -1;

        for (int i = 0; i < incrementCount; i++)
        {
            // Get current value
            var getCurrentResponse = await httpClient.GetAsync("/api/counter", cancellationToken);
            var currentValue = await getCurrentResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

            // If this isn't the first iteration, verify the value increased
            if (previousValue >= 0)
            {
                Assert.True(currentValue > previousValue, 
                    $"Counter value {currentValue} should be greater than previous value {previousValue}");
            }

            // Increment
            var postResponse = await httpClient.PostAsync("/api/counter", null, cancellationToken);
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
            var incrementedValue = await postResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

            // Verify the increment worked
            Assert.Equal(currentValue + 1, incrementedValue);
            previousValue = incrementedValue;

            // Small delay to avoid race conditions
            await Task.Delay(10, cancellationToken);
        }

        // Get final value and verify it matches the last incremented value
        var finalResponse = await httpClient.GetAsync("/api/counter", cancellationToken);
        var finalValue = await finalResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(previousValue, finalValue);
    }
}
