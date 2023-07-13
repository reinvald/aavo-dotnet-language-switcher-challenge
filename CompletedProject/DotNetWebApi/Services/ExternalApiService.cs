namespace DotNetWebApi.Services;

public class ExternalApiService : IExternalApiService
{
    private readonly HttpClient _httpClient;

    public ExternalApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetSwaggerJson(string host)
    {
        var url = $"http://{host}/swagger/v1/swagger.json";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public bool IsEven(int value)
    {
        return (value % 2) == 0;
    }
}