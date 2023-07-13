namespace DotNetWebApi.Services;

public interface IExternalApiService
{
    Task<string> GetSwaggerJson(string host);
}
