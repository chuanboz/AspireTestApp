namespace AspireTestApp.Web;

public class CounterApiClient(HttpClient httpClient)
{
    public async Task<int> GetCounterAsync(string name = "default", CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<int>($"/api/counter?name={Uri.EscapeDataString(name)}", cancellationToken);
        return result;
    }

    public async Task<int> IncrementCounterAsync(string name = "default", CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync($"/api/counter?name={Uri.EscapeDataString(name)}", null, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<int>(cancellationToken);
        return result;
    }
}
