using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services;

public sealed class RaceDayApiClient(IHttpClientFactory clients, IHttpContextAccessor context)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<(UserResponse? User, string? Error)> LoginAsync(LoginRequest model)
    {
        using var response = await clients.CreateClient("RaceDayApi").PostAsJsonAsync("api/auth/login", model);
        if (!response.IsSuccessStatusCode) return (null, await ReadErrorAsync(response));
        if (response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            var cookie = values.FirstOrDefault(x => x.StartsWith("RaceDay.Auth=", StringComparison.OrdinalIgnoreCase));
            if (cookie is not null) context.HttpContext?.Session.SetString("ApiCookie", cookie.Split(';', 2)[0]);
        }
        return (await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions), null);
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest model)
    {
        using var response = await clients.CreateClient("RaceDayApi").PostAsJsonAsync("api/auth/register", model);
        return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? null : await ReadErrorAsync(response));
    }

    public async Task LogoutAsync()
    {
        try { using var response = await SendAsync(HttpMethod.Post, "api/auth/logout"); }
        finally { context.HttpContext?.Session.Clear(); }
    }

    public Task<List<EventResponse>?> GetEventsAsync() => GetAsync<List<EventResponse>>("api/events");
    public Task<EventDetailsResponse?> GetEventAsync(int id) => GetAsync<EventDetailsResponse>($"api/events/{id}");
    public Task<List<EnrolmentResponse>?> GetMyEnrolmentsAsync() => GetAsync<List<EnrolmentResponse>>("api/enrolments/me");
    public Task<List<ResultResponse>?> GetMyResultsAsync() => GetAsync<List<ResultResponse>>("api/results/me");

    public async Task<(bool Success, string? Error)> EnrolAsync(EnrolmentRequest model)
    {
        using var response = await SendAsync(HttpMethod.Post, "api/enrolments", model);
        return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? null : await ReadErrorAsync(response));
    }

    public async Task<(bool Success, string? Error)> CreateEventAsync(EventRequest model)
    {
        using var response = await SendAsync(HttpMethod.Post, "api/events", model);
        return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? null : await ReadErrorAsync(response));
    }

    private async Task<T?> GetAsync<T>(string path)
    {
        using var response = await SendAsync(HttpMethod.Get, path);
        if (response.StatusCode == HttpStatusCode.Unauthorized) return default;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private Task<HttpResponseMessage> SendAsync(HttpMethod method, string path) => SendAsync<object?>(method, path, null);

    private async Task<HttpResponseMessage> SendAsync<T>(HttpMethod method, string path, T? body)
    {
        var request = new HttpRequestMessage(method, path);
        var cookie = context.HttpContext?.Session.GetString("ApiCookie");
        if (!string.IsNullOrWhiteSpace(cookie)) request.Headers.TryAddWithoutValidation("Cookie", cookie);
        if (body is not null) request.Content = JsonContent.Create(body);
        return await clients.CreateClient("RaceDayApi").SendAsync(request);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        try
        {
            using var json = JsonDocument.Parse(body);
            if (json.RootElement.TryGetProperty("message", out var message)) return message.GetString() ?? "The API request failed.";
            if (json.RootElement.TryGetProperty("title", out var title)) return title.GetString() ?? "The API request failed.";
        }
        catch { }
        return string.IsNullOrWhiteSpace(body) ? $"API request failed ({(int)response.StatusCode})." : body;
    }
}