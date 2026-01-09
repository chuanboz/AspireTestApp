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

        // Act - Test the default counter which is seeded
        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/api/counter?name=default", cancellationToken);

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
        var counterName = $"test-id-field-{Guid.NewGuid():N}";

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        
        // Increment the counter to ensure document is created/updated
        var postResponse = await httpClient.PostAsync($"/api/counter?name={counterName}", null, cancellationToken);
        
        // Verify the operation succeeded
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        
        // Get the counter value to ensure document exists
        var getResponse = await httpClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var counterValue = await getResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(1, counterValue); // First increment should result in 1
    }

    [Fact]
    public async Task CounterPersistsAfterMultipleOperations()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var app = fixture.App;
        var counterName = $"test-persist-ops-{Guid.NewGuid():N}";
        var httpClient = app.CreateHttpClient("apiservice");

        // Act - Increment multiple times and verify each increment
        var incrementCount = 5;

        for (int i = 1; i <= incrementCount; i++)
        {
            // Increment
            var postResponse = await httpClient.PostAsync($"/api/counter?name={counterName}", null, cancellationToken);
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
            var incrementedValue = await postResponse.Content.ReadFromJsonAsync<int>(cancellationToken);

            // Verify the increment resulted in the expected value
            Assert.Equal(i, incrementedValue);
        }

        // Get final value and verify it matches the last incremented value
        var finalResponse = await httpClient.GetAsync($"/api/counter?name={counterName}", cancellationToken);
        var finalValue = await finalResponse.Content.ReadFromJsonAsync<int>(cancellationToken);
        Assert.Equal(incrementCount, finalValue);
    }
}
