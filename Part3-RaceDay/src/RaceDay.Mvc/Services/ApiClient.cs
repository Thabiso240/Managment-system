using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Mvc.Models;

namespace RaceDay.Mvc.Services;

/// <summary>Result of an API call that returns no payload.</summary>
public record ApiResult(bool Success, string? Error, int StatusCode)
{
    public static ApiResult Ok(int statusCode = 200) => new(true, null, statusCode);
    public static ApiResult Fail(string error, int statusCode) => new(false, error, statusCode);
}

/// <summary>Result of an API call that returns a payload.</summary>
public record ApiResult<T>(bool Success, T? Data, string? Error, int StatusCode)
{
    public static ApiResult<T> Ok(T data) => new(true, data, null, 200);
    public static ApiResult<T> Fail(string error, int statusCode) => new(false, default, error, statusCode);
}

/// <summary>
/// Single point of contact between the MVC front end and the RaceDay API.
/// The MVC application contains no database access and no business logic - every
/// operation below is delegated to the API over HTTP.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;

    public ApiClient(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        _accessor = accessor;
    }

    private string? Token => _accessor.HttpContext?.Session.GetString(SessionKeys.Token);

    private HttpRequestMessage Request(HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);

        var token = Token;
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private async Task<ApiResult<T>> SendAsync<T>(HttpRequestMessage request)
    {
        try
        {
            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>();
                return data is null
                    ? ApiResult<T>.Fail("The server returned an empty response.", (int)response.StatusCode)
                    : ApiResult<T>.Ok(data);
            }

            return ApiResult<T>.Fail(await ReadErrorAsync(response), (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Fail("The RaceDay service is unavailable. Please try again shortly.", 503);
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Fail("The request timed out. Please try again.", 408);
        }
    }

    private async Task<ApiResult> SendAsync(HttpRequestMessage request)
    {
        try
        {
            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Ok((int)response.StatusCode);
            }

            return ApiResult.Fail(await ReadErrorAsync(response), (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ApiResult.Fail("The RaceDay service is unavailable. Please try again shortly.", 503);
        }
        catch (TaskCanceledException)
        {
            return ApiResult.Fail("The request timed out. Please try again.", 408);
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var fallback = $"The request could not be completed ({(int)response.StatusCode}).";

        try
        {
            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body)) return fallback;

            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                return message.GetString() ?? fallback;

            if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                return detail.GetString() ?? fallback;

            if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                return title.GetString() ?? fallback;
        }
        catch (JsonException)
        {
            // Fall through to the generic message.
        }

        return fallback;
    }

    // -----------------------------------------------------------------------
    // Authentication
    // -----------------------------------------------------------------------

    public Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request)
        => SendAsync<AuthResponse>(Request(HttpMethod.Post, "api/auth/login", request));

    public Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request)
        => SendAsync<AuthResponse>(Request(HttpMethod.Post, "api/auth/register", request));

    // -----------------------------------------------------------------------
    // Events (shared)
    // -----------------------------------------------------------------------

    public async Task<List<EventDto>> GetEventsAsync(string? search, string? type, string? location, bool upcomingOnly)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(type)) query.Add($"type={Uri.EscapeDataString(type)}");
        if (!string.IsNullOrWhiteSpace(location)) query.Add($"location={Uri.EscapeDataString(location)}");
        if (upcomingOnly) query.Add("upcoming=true");

        var url = "api/events" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
        var result = await SendAsync<List<EventDto>>(Request(HttpMethod.Get, url));
        return result.Data ?? new List<EventDto>();
    }

    public async Task<EventDetailDto?> GetEventAsync(int id)
    {
        var result = await SendAsync<EventDetailDto>(Request(HttpMethod.Get, $"api/events/{id}"));
        return result.Data;
    }

    // -----------------------------------------------------------------------
    // Participant
    // -----------------------------------------------------------------------

    public Task<ApiResult<EnrolmentDto>> EnrolAsync(int eventId, int? categoryId)
        => SendAsync<EnrolmentDto>(Request(HttpMethod.Post, $"api/events/{eventId}/enrolments", new CreateEnrolmentRequest(categoryId)));

    public async Task<List<EnrolmentDto>> GetMyEnrolmentsAsync()
    {
        var result = await SendAsync<List<EnrolmentDto>>(Request(HttpMethod.Get, "api/enrolments/mine"));
        return result.Data ?? new List<EnrolmentDto>();
    }

    public Task<ApiResult> CancelEnrolmentAsync(int enrolmentId)
        => SendAsync(Request(HttpMethod.Delete, $"api/enrolments/{enrolmentId}"));

    public async Task<List<ParticipantResultDto>> GetMyResultsAsync()
    {
        var result = await SendAsync<List<ParticipantResultDto>>(Request(HttpMethod.Get, "api/participants/me/results"));
        return result.Data ?? new List<ParticipantResultDto>();
    }

    public async Task<UserDto?> GetProfileAsync()
    {
        var result = await SendAsync<UserDto>(Request(HttpMethod.Get, "api/users/me"));
        return result.Data;
    }

    public Task<ApiResult<UserDto>> UpdateProfileAsync(UpdateProfileRequest request)
        => SendAsync<UserDto>(Request(HttpMethod.Put, "api/users/me", request));

    public async Task<ApiResult<UserDto>> UploadProfilePhotoAsync(Stream content, string fileName, string contentType)
    {
        using var form = new MultipartFormDataContent();
        var file = new StreamContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "file", fileName);

        var request = new HttpRequestMessage(HttpMethod.Post, "api/users/me/photo") { Content = form };
        AttachToken(request);
        return await SendAsync<UserDto>(request);
    }

    // -----------------------------------------------------------------------
    // Organiser
    // -----------------------------------------------------------------------

    public async Task<OrganiserDashboardDto?> GetDashboardAsync()
    {
        var result = await SendAsync<OrganiserDashboardDto>(Request(HttpMethod.Get, "api/organiser/dashboard"));
        return result.Data;
    }

    public async Task<List<EventDto>> GetMyEventsAsync()
    {
        var result = await SendAsync<List<EventDto>>(Request(HttpMethod.Get, "api/events/mine"));
        return result.Data ?? new List<EventDto>();
    }

    public Task<ApiResult<EventDto>> CreateEventAsync(CreateEventRequest request)
        => SendAsync<EventDto>(Request(HttpMethod.Post, "api/events", request));

    public Task<ApiResult<EventDto>> UpdateEventAsync(int id, UpdateEventRequest request)
        => SendAsync<EventDto>(Request(HttpMethod.Put, $"api/events/{id}", request));

    public Task<ApiResult> DeleteEventAsync(int id)
        => SendAsync(Request(HttpMethod.Delete, $"api/events/{id}"));

    public async Task<ApiResult<EventDto>> UploadBannerAsync(int eventId, Stream content, string fileName, string contentType)
    {
        using var form = new MultipartFormDataContent();
        var file = new StreamContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "file", fileName);

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/events/{eventId}/banner") { Content = form };
        AttachToken(request);
        return await SendAsync<EventDto>(request);
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(int eventId)
    {
        var result = await SendAsync<List<CategoryDto>>(Request(HttpMethod.Get, $"api/events/{eventId}/categories"));
        return result.Data ?? new List<CategoryDto>();
    }

    public Task<ApiResult<CategoryDto>> CreateCategoryAsync(int eventId, CreateCategoryRequest request)
        => SendAsync<CategoryDto>(Request(HttpMethod.Post, $"api/events/{eventId}/categories", request));

    public Task<ApiResult<CategoryDto>> UpdateCategoryAsync(int categoryId, UpdateCategoryRequest request)
        => SendAsync<CategoryDto>(Request(HttpMethod.Put, $"api/categories/{categoryId}", request));

    public Task<ApiResult> DeleteCategoryAsync(int categoryId)
        => SendAsync(Request(HttpMethod.Delete, $"api/categories/{categoryId}"));

    public async Task<List<EventEnrolmentDto>> GetEventEnrolmentsAsync(int eventId)
    {
        var result = await SendAsync<List<EventEnrolmentDto>>(Request(HttpMethod.Get, $"api/events/{eventId}/enrolments"));
        return result.Data ?? new List<EventEnrolmentDto>();
    }

    public Task<ApiResult<EventEnrolmentDto>> RecordResultAsync(int eventId, RecordResultRequest request)
        => SendAsync<EventEnrolmentDto>(Request(HttpMethod.Post, $"api/events/{eventId}/results", request));

    public Task<ApiResult> PublishResultsAsync(int eventId, int totalFinishers)
        => SendAsync(Request(HttpMethod.Post, $"api/events/{eventId}/results/publish", new PublishResultsRequest(totalFinishers)));

    private void AttachToken(HttpRequestMessage request)
    {
        var token = Token;
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
