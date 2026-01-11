# RestMiddleware  
*A lightweight, generic HTTP client wrapper for building .NET repository layers.*

RestMiddleware is a simple but powerful helper library that eliminates repetitive `HttpClient` boilerplate. It provides a `RestClient` helper class that uses `IHttpClientFactory` to perform consistent, strongly-typed HTTP network calls with minimal code.

**Compatible with:** .NET Standard 2.0 (.NET Framework 4.6.1+, .NET Core 2.0+, .NET 5/6/7/8/9+)

---

## ✨ Features

- 🚀 **Async Generic Helpers**: Strongly typed responses (`(data, response)`)
- 🏷️ **Named Instances**: Use multiple API configurations in one project
- 🧭 **Nested Layouts**: Navigate deep JSON responses using `>` (e.g., `data>payload>items`)
- 🥊 **Simple Response**: Direct body parsing for standard REST APIs
- 🛡️ **Auto-Refresh**: Automatic JWT token refresh handling
- 🧱 **Efficient**: Uses `IHttpClientFactory` for connection pooling
- 🌍 **Global & Per-Request Headers**: Easy custom header management

---

## 📚 Installation

Install the NuGet package:

```bash
dotnet add package RestMiddleware
```

---

## 🚀 Initialization

### Basic Setup (.NET Core / .NET 5+)

In `Program.cs` or `Startup.cs`:

```csharp
using RestMiddleware.Extensions;

services.AddRestMiddleware(options => 
{
    options.MethodToGetBaseUrl = () => "https://api.myapp.com";
    options.MethodToGetToken = () => GetUserToken(); // Delegate to get token
    options.FetchRefreshTokenIfUnauthorised = true;
});
```

### Named Instances (Multiple APIs)

If you need to connect to multiple different APIs:

```csharp
// Default Client
services.AddRestMiddleware(options => {
    options.MethodToGetBaseUrl = () => "https://api.one.com";
});

// Second API Client
services.AddRestMiddleware("InventoryService", options => {
    options.MethodToGetBaseUrl = () => "https://inventory.api.com";
    options.UseSimpleResponse(); // Parse direct body
});
```

### Usage with Named Instances

```csharp
public class MyService 
{
    private readonly RestClient _inventoryClient;

    public MyService(IRestClientFactory factory)
    {
        _inventoryClient = factory.CreateClient("InventoryService");
    }
}
```

### 🧩 Response Parsing Strategies

There are two main ways to tell the library how to extract data and errors from your API responses:

#### Option A: Schema-Based (Standard Envelopes)
Best when your API wraps results in a standard structure (e.g., `{ "status": "ok", "result": { ... }, "errors": [] }`).

You provide a **sample JSON file** and the **keys** to navigate.
- **Support for nested keys**: Use `>` to go deep (e.g., `data>items`).
- **Automatic Deduction**: The library analyzes the sample file to learn how to parse error lists.

```csharp
options.UseResponseLayout(
    sampleJsonPath: "api_sample.json", 
    dataKey: "result>payload", 
    errorKey: "errors"
);
```

#### Option B: Function-Based (Custom Logic)
Best for maximum flexibility or when you want to handle parsing manually without a schema file.

```csharp
options.ParseSuccess = (json, type) => {
    // Custom logic to extract and deserialize data (e.g. using JObject)
    return myCustomDeserializer(json, type);
};

options.ParseErrors = (json) => {
    // Custom logic to extract error messages as string[]
    return new string[] { "Something failed" };
};
```

#### Quick Start: Simple Response
If your API returns the object/array directly (no wrapper), use:
```csharp
options.UseSimpleResponse();
```

---

## 🧪 Configuration Options

### `HttpRequestOptions`

| Option | Type | Description |
|--------|------|-------------|
| `MethodToGetBaseUrl` | `Func<string>` | Returns the Base URL for the client (sync). |
| `MethodToGetBaseUrlAsync` | `Func<Task<string>>` | **Preferred**. Async version to retrieve Base URL. |
| `MethodToGetToken` | `Func<string>` | Returns the Bearer token for authorization (sync). |
| `MethodToGetTokenAsync` | `Func<Task<string>>` | **Preferred**. Async version to retrieve Bearer token. |
| `FetchRefreshTokenIfUnauthorised` | `bool` | If true, attempts to refresh token on 401. |
| `MethodToGetRefreshTokenEndpoint` | `Func<string>` | Endpoint to call for refreshing tokens. |
| `MethodToGetRefreshToken` | `Func<string>` | Optional. Returns the refresh token (if distinctive). |
| `AccessTokenParameterName` | `string` | JSON key for access token (default: "jwt_token"). |
| `RefreshTokenParameterName` | `string` | JSON key for refresh token (default: "refresh_token"). |
| `CreateCustomRefreshRequestBody` | `Func<object>` | **Advanced**. Return any object to fully customize the refresh body. |
| `MethodToSetTokenLocally` | `Action<object>` | Callback to save the new token (sync). |
| `MethodToSetTokenLocallyAsync` | `Func<object, Task>` | **Preferred**. Async callback to save new token. |
| `ParseSuccess` | `Func<string, Type, object>` | The function responsible for parsing success JSON. |
| `ParseErrors` | `Func<string, string[]>` | The function responsible for parsing error JSON. |

---

## 🛠 Usage

### Standard Calls
The library returns a tuple: `(T data, HttpResponseDto response)`.

```csharp
// POST request with body and CancellationToken
var (user, response) = await _client.PostAndGetObject<UserDto>(new HttpRequestDto()
{
    endpoint = "/users",
    data = new { name = "John" },
    CancellationToken = myCancellationToken
});
```

### Async Token Retrieval Example
```csharp
options.MethodToGetTokenAsync = async () => {
    var token = await _secureStorage.GetAsync("token");
    return token;
};
```

---

## 🔐 Automated Auth Workflow

One of the most powerful features of `RestMiddleware` is the automated token refresh. When a request fails with a `401 Unauthorized`, the library can automatically refresh the token and retry the original request.

### Example Implementation

Here is a mock implementation showing how to configure a fully automated auth loop:

```csharp
services.AddRestMiddleware(options => 
{
    options.MethodToGetBaseUrl = () => "https://api.myapp.com";

    // 1. Tell the library how to fetch the CURRENT token asynchronously
    options.MethodToGetTokenAsync = async () => {
        return await MySecureStorage.GetTokenAsync();
    };

    // 2. Enable auto-refresh on 401
    options.FetchRefreshTokenIfUnauthorised = true;
    options.MethodToGetRefreshTokenEndpoint = () => "/api/auth/refresh";
    
    // Optional: Customize payload keys (Defaults: "jwt_token", "refresh_token")
    options.AccessTokenParameterName = "expiredToken"; 
    options.RefreshTokenParameterName = "refreshToken";
    
    // Optional: Provide the separate refresh token just for this call
    options.MethodToGetRefreshToken = () => MySecureStorage.GetRefreshToken();

    // 3. Tell the library how to SAVE the new token after refresh
    options.MethodToSetTokenLocallyAsync = async (tokenResponse) => {
        // 'tokenResponse' is the object returned by your refresh endpoint
        var newToken = ((dynamic)tokenResponse).access_token;
        await MySecureStorage.SaveTokenAsync(newToken);
    };
});
```

With this configuration, your repositories don't need to worry about expiration. `RestClient` will handle the "Expire -> Refresh -> Retry" cycle silently in the background.

---

### Accessing Response Headers
```csharp
var contentType = response.Headers["Content-Type"].FirstOrDefault();
```

---

## 📡 Available Methods

All methods return `(T data, HttpResponseDto response)`.

| Method | Description |
|--------|-------------|
| `GetSingleItem<T>(dto)` | GET a single object of type `T`. |
| `GetList<T>(dto)` | GET a list of `T`. |
| `GetPrimitiveTypeObject(dto)` | GET a primitive value (string, int, etc). |
| `Post(dto)` | POST JSON body (returns `HttpResponseDto`). |
| `PostAndGetObject<T>(dto)` | POST data and parse response as `T`. |
| `PostAndGetList<T>(dto)` | POST data and parse response as `List<T>`. |
| `PostMultipartAndGetObject<T>(dto)` | Form-data / Multipart POST. |
| `DeleteItem(dto)` | DELETE request. |

---

## 📄 License

MIT License. Free for commercial and personal use.
