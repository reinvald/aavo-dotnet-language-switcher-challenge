# External Service API

Lets look at how we can call external services to retrieve data. To do this we'll use .NET's `HttpClient`. For this tutorial we will make a new endpoint, that makes an HttpRequest to retrieve this project's Swagger json definition.

## Creating an HttpClient

First we need to add an `HttpClient` to our available services. Add the following line to the `Program.cs` Main function (before the call to `builder.Build()`).  It will allow for injecting
an `HttpClient` instance into your services (with the appropriate lifecycle).

```csharp
builder.Services.AddHttpClient();
```

> ---
> **Note**: `HttpClient` is Complicated
>
> ---
> This exercise uses the `HttpClient` class directly, using _dependency injection_ to inject
> it into services.  The HttpClient class is quirky (for example, it implements `IDisposable`,
> but should not normally be Disposed).  In many cases, it makes more sense to use 
> `IHttpClientFactory` (with dependency injection) as a factory for your HttpClients.  You can 
> read up more about this in
> [IHttpClientFactory with .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory).
> What you definitely **should not** be doing is [creating and disposing a new `HttpClient` for 
> each request.](https://learn.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client#create-and-initialize-httpclient).
> Using dependency injection as shown here works well.
>
> ---

## Create an External Service

We want our external service to be available via Dependency Injection. We will associate an
injected interface with a concrete type that represents this service.  To do this, create a 
directory in the project called `Services`.  Add two new files; one is the interface 
describing the service, the other the associated concrete type.

```
├── DotNetWebApi
│   ├── Controllers
│   │   ├── ...
│   ├── Models
│   │   ├── ...
│   ├── Services
│   │   ├── (+)ExternalApiService.cs
│   │   ├── (+)IExternalApiService.cs
```

The service will run asynchronously and return a string (with the JSON returned from the
call to the external web api service).  As a result, it returns a `Task<string>`.  Note that
in the interface definition, neither of the `await` nor `async` keywords show up.

The service will be calling back into this web site.  We need the host's URL to get access
to this.  The controller can fish out the URL, so this call will accept the host as a string
to use as the host in the call.

`IExternalApiService.cs`:
```csharp
namespace DotNetWebApi.Services;

public interface IExternalApiService
{
    Task<string> GetSwaggerJson(string host);
}
```

The concrete service type implements the interface (denoted by 
`class MyClass : IMyInterface` - in .NET, 
[interfaces traditionally start with a capital `I`](./1-ScaffoldingYourProgram.md#net-naming---namespaces-types-members-and-variables)). 
The service accepts the dependency injected HttpClient and then provides the implementation of
the `GetSwaggerJson` method defined by the interface:

`ExternalApiService.cs`:
```csharp
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
}
```
Note that this code uses an _Interpolated String_.  The `url` variable is initialized with
a literal string prefaced with a `$`.  The `$` indicates that any expressions within `{`curly 
braces`}` should be evaluated as a string and incorporated into the string.


## Inject the Service

Now that we have our object and interface, we can inject an instance of `ExternalApiService` into our services. Add the following line to the `Program.cs` file (before the call to 
`builder.Build()`). This creates a singleton instance of our `ExternalApiService` class,
ready to inject it into any controller or service that requests an `IExternalApiService`.

```csharp
builder.Services.AddSingleton<IExternalApiService, ExternalApiService>();
```

You will also need to add a `using` statement to the top of that file:

```C#
using DotNetWebApi.Services;
```

For more information on the available methods to inject dependencies, read Microsoft's [ServiceCollectionServiceExtensions Class Methods](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.servicecollectionserviceextensions?view=dotnet-plat-ext-7.0#methods)

## Create new Endpoint

Like we've done previously, we will create a new controller for this, a `ServicesController`.
To do this, create a new file in the `Controllers` folder named `ServicesController.cs`.
Add this boilerplate code:

```C#
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
}
```

Note that this is a bit different from previous controllers:

1. We are incorporating the `DotNetWebApi.Services` namespace with a `using` statement, not
   the `Models` namespace
2. We are injecting the `IExternalApiService` that we defined (and not the database context).
   As described above, this will inject an `ExternalApiService` instance into the controller
   (accessible via an `IExternalApiService`-typed member field).

Now add the endpoint to the controller, fetching the data from the service and returning
it to the caller.  Add the following to the `ServicesController` class:
```C#
[HttpGet("SwaggerDefinition")]
public async Task<string> GetSwaggerDefinition()
{
    var host = Request.Host.Value;
    return await _apiService.GetSwaggerJson(host);
}
```

Note that `Request` is a property of `ControllerBase` (the base class of `ServicesController`).
It describes the request.  `Request.Host` will return the domain name and the port number here,
in the appropriate format (something like `"localhost:1234"`).

## Validate

Run the program by pressing `F5` in VS Code.  Run the `SwaggerDefinition` (GET) endpoint and 
see the OpenAPI (/Swagger) definition of our application in the response.

## Next Steps

[Unit Tests](/docs/6-Unit-Testing.md)