# RestMiddleware  
*A lightweight, generic HTTP client wrapper for building .NET repository layers.*

RestMiddleware is a simple but powerful helper library that eliminates repetitive `HttpClient` boilerplate. It provides a `RestClient` helper class that uses `IHttpClientFactory` to perform consistent, strongly-typed HTTP network calls with minimal code.

**Compatible with:** .NET Standard 2.0 (.NET Framework 4.6.1+, .NET Core 2.0+, .NET 5/6/7+)

---

## ✨ Features

- 🚀 Async generic HTTP helpers  
- 📦 Strongly typed responses (`T`, `List<T>`)  
- 🔄 Unified response structure  
- 🧽 Centralized error handling  
- 🧱 Uses `IHttpClientFactory` for efficient connection pooling  
- ♻️ Highly reusable across repositories  
- 💡 Clean, readable architecture (Composition over Inheritance)
- 🌍 Global configuration support (Startup.cs or Global.asax)

---

## 📚 Installation

Install the NuGet package:

```bash
dotnet add package RestMiddleware
```

---

## 🚀 Initialization

### Option 1: .NET Core / .NET 5+ (Dependency Injection)

In `Startup.cs` or `Program.cs`:

```csharp
using RestMiddleware.Extensions;

services.AddRestMiddleware(options => 
{
    options.MethodToGetBaseUrl = () => "https://api.myapp.com";
    options.MethodToGetToken = () => GetUserToken(); // Delegate to get token
    options.FetchRefreshTokenIfUnauthorised = true;
});
```

### Option 2: Legacy .NET Framework (Global.asax)

In `Global.asax.cs`:

```csharp
using RestMiddleware;

protected void Application_Start()
{
    GlobalConfiguration.Options.MethodToGetBaseUrl = () => "https://api.myapp.com";
    GlobalConfiguration.Options.MethodToGetToken = () => HttpContext.Current.Session["Token"]?.ToString();
}
```

---

## 🛠 Usage in Repository

Inject `RestClient` into your repository:

```csharp
public class LeaveRepository : ILeaveRepository
{
    private readonly RestClient _client;

    public LeaveRepository(RestClient client)
    {
        _client = client;
    }

    public async Task<LeaveListResponseDto> GetLeaves()
    {
        // Global options (Base URL, Token) are automatically applied!
        // You only need to specify the endpoint/data.
        var result = await _client.GetSingleItem<LeaveListResponseDto>(new HttpRequestDto()
        {
            endpoint = "/api/leaves" 
        });

        if (result.response.IsSuccess)
            return result.data;
        
        throw new Exception("Error fetching leaves");
    }
}
```

---

## 📡 Available Methods

| Method | Description |
|--------|-------------|
| `GetSingleItem<T>(dto)` | GET a single object of type `T` |
| `GetList<T>(dto)` | GET a list of `T` |
| `Post(dto)` | POST JSON body (no return data) |
| `PostAndGetObject<T>(dto)` | POST and return `T` |
| `DeleteItem(dto)` | DELETE request |

---

## 📄 License

MIT License. Free for commercial and personal use.
