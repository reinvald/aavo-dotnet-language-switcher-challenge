using Microsoft.AspNetCore.Mvc;
using DotNetWebApi.Services;

namespace DotNetWebApi.Controllers;

[Route("api/")]
[ApiController]
public class ServicesController : ControllerBase
{
    private readonly IExternalApiService _apiService;

    public ServicesController(IExternalApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet("SwaggerDefinition")]
    public async Task<string> GetSwaggerDefinition()
    {
        var host = Request.Host.Value;
        return await _apiService.GetSwaggerJson(host);
    }
}
