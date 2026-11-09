using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services;

public sealed class RaceDayApiClient
{
    private readonly IHttpClientFactory _clients;
    private readonly IHttpContextAccessor _context;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public RaceDayApiClient(IHttpClientFactory clients, IHttpContextAccessor context)
    {
        _clients = clients;
        _context = context;
    }

    public async Task<(UserResponse? User, string? Error)> LoginAsync(LoginRequest model)
    {
        using var response = await _clients.CreateClient("RaceDayApi").PostAsJsonAsync("api/auth/login", model);
        if (!response.IsSuccessStatusCode)
            return (null, await ReadErrorAsync(response));
        SaveApiCookie(response);
        return (await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions), null);
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest model)
    {
        using var response = await _clients.CreateClient("RaceDayApi").PostAsJsonAsync("api/auth/register", model);
        return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? null : await ReadErrorAsync(response));
    }

    public async Task LogoutAsync()
    {
        try { using var response = await SendAsync(HttpMethod.Post, "api/auth/logout"); }
        finally { _context.HttpContext?.Session.Clear(); }
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

    private async Task<HttpResponseMessage> SendAsync<T>(HttpMethod method, string path, T? body = default)
    {
        var request = new HttpRequestMessage(method, path);
        var cookie = _context.HttpContext?.Session.GetString("ApiCookie");
        if (!string.IsNullOrWhiteSpace(cookie)) request.Headers.TryAddWithoutValidation("Cookie", cookie);
        if (body is not null) request.Content = JsonContent.Create(body);
        return await _clients.CreateClient("RaceDayApi").SendAsync(request);
    }

    private void SaveApiCookie(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            var authCookie = values.FirstOrDefault(x => x.StartsWith("RaceDay.Auth=", StringComparison.OrdinalIgnoreCase));
            if (authCookie is not null)
                _context.HttpContext?.Session.SetString("ApiCookie", authCookie.Split(';', 2)[0]);
        }
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
