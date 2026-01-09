namespace AspireTestApp.Web;

public class CounterApiClient(HttpClient httpClient)
{
    public async Task<int> GetCounterAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<int>("/api/counter", cancellationToken);
        return result;
    }

    public async Task<int> IncrementCounterAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync("/api/counter", null, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<int>(cancellationToken);
        return result;
    }
}
