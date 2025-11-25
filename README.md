# RestMiddleware  
*A lightweight, generic HTTP abstraction layer for building .NET repository layers.*

RestMiddleware is a simple but powerful helper library that eliminates repetitive `HttpClient` boilerplate. It provides a generic `HttpMiddleware<T>` base class that your repositories can inherit from to perform consistent, strongly-typed HTTP network calls with minimal code.

---

## ✨ Features

- 🚀 Async generic HTTP helpers  
- 📦 Strongly typed responses (`T`, `List<T>`)  
- 🔄 Unified response structure  
- 🧽 Centralized error handling  
- 🧱 Zero dependency, lightweight  
- ♻️ Highly reusable across repositories  
- 💡 Clean, readable architecture  

---

## 📚 How It Works

Inherit from:

```csharp
public class LeaveRepository : HttpMiddleware<LeaveListResponseDto>
```

Your repository will automatically have these methods available:

- `GetSingleItem()`
- `GetList()`
- `Post()`
- `Put()`
- `DeleteItem()`

Each request is defined using:

```csharp
public class HttpRequestDto
{
    public string endpoint { get; set; }
    public object? data { get; set; }
    public string? query { get; set; }
    public Dictionary<string, string>? headers { get; set; }
}
```

---

## 🚀 Usage Example

### Example Repository

```csharp
public class LeaveRepository 
    : HttpMiddleware<LeaveListResponseDto>, ILeaveRepository
{
    public async Task Delete(int id)
    {
        await DeleteItem(new HttpRequestDto()
        {
            endpoint = Constants.DeleteLeaveUrl,
            query = $"id={id}"
        });
    }

    public async Task<LeaveListResponseDto> GetLeaveOfLoggedInUser()
    {
        var result = await GetSingleItem(new HttpRequestDto()
        {
            endpoint = Constants.MyLeaveUrl
        });

        if (result.response.IsSuccess)
            return result.data;

        if (result.response.IsError)
            throw new Exception("Failed to get vacation detail.");

        var info = result.response.InfoDetail.ToObject<InfoResponseDto>();
        throw new CustomException(info.description);
    }

    public async Task<List<LeaveTransferOptionDto>> GetTransferOptions()
    {
        var result = await HttpMiddleware<LeaveTransferOptionDto>.GetList(
            new HttpRequestDto { endpoint = Constants.LeaveTransferOptionsUrl });

        if (result.response.IsSuccess)
            return result.datas;

        if (result.response.IsError)
            throw new Exception("Failed to get transfer options.");

        var info = result.response.InfoDetail.ToObject<InfoResponseDto>();
        throw new CustomException(info.description);
    }

    public async Task Save(AddLeaveDto dto)
    {
        var result = await Post(new HttpRequestDto()
        {
            data = dto,
            endpoint = Constants.SaveLeaveUrl
        });

        if (result.IsSuccess)
            return;

        if (result.IsError)
            throw new Exception("Failed to save vacation detail.");

        var info = result.InfoDetail.ToObject<InfoResponseDto>();
        throw new CustomException(info.description);
    }
}
```

---

## 📡 Available Methods

| Method | Description |
|--------|-------------|
| `GetSingleItem(dto)` | GET a single object of type `T` |
| `GetList(dto)` | GET a list of `T` |
| `Post(dto)` | POST JSON body |
| `Put(dto)` | PUT JSON body |
| `DeleteItem(dto)` | DELETE request |

---

## 🧱 Why Use RestMiddleware?

- ✨ Cleaner repository code  
- 🔄 No repeated HttpClient logic  
- 🔗 Strongly typed everywhere  
- 🛠️ Ideal for Clean Architecture, MVVM, DDD  
- 💡 Encapsulates networking concerns  

---

## 📄 License

MIT License. Free for commercial and personal use.
