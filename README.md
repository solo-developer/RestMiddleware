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
| `MethodToGetBaseUrl` | `Func<string>` | **Required**. Returns the Base URL for the client. |
| `MethodToGetToken` | `Func<string>` | Optional. Returns the Bearer token for authorization. |
| `FetchRefreshTokenIfUnauthorised` | `bool` | If true, attempts to refresh token on 401. |
| `MethodToGetRefreshTokenEndpoint` | `Func<string>` | Endpoint to call for refreshing tokens. |
| `MethodToSetTokenLocally` | `Action<object>` | Callback to save the new token received. |
| `ParseSuccess` | `Func<string, Type, object>` | The function responsible for parsing success JSON. |
| `ParseErrors` | `Func<string, string[]>` | The function responsible for parsing error JSON. |

---

## 🛠 Usage

### Standard Calls
The library returns a tuple: `(T data, HttpResponseDto response)`.

```csharp
// POST request with body
var (user, response) = await _client.PostAndGetObject<UserDto>(new HttpRequestDto()
{
    endpoint = "/users",
    data = new { name = "John" },
    Headers = new Dictionary<string, string> { { "X-Priority", "High" } }
});

if (response.IsSuccess) {
    Console.WriteLine(user.Id);
}
```

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
