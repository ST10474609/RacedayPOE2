using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;
namespace RaceDay.Tests;
public class RoleAccessTests : IClassFixture<ApiFactory>
{
    readonly ApiFactory factory; public RoleAccessTests(ApiFactory factory)=>this.factory=factory;
    async Task<HttpClient> Login(string role){var client=factory.CreateClient(new WebApplicationFactoryClientOptions{HandleCookies=true});var email=$"{role.ToLower()}{Guid.NewGuid():N}@example.com";await client.PostAsJsonAsync("/api/auth/register",new RegisterRequest("Test",role,email,"Password123!",role,null));var login=await client.PostAsJsonAsync("/api/auth/login",new LoginRequest(email,"Password123!"));login.EnsureSuccessStatusCode();return client;}
    [Fact] public async Task ParticipantCannotCreateEvent(){var client=await Login("Participant");var response=await client.PostAsJsonAsync("/api/events",new EventRequest("Test Event",null,DateTime.UtcNow.AddDays(10),"Venue","City","Province","Running"));Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);}
    [Fact] public async Task OrganiserCanCreateEvent(){var client=await Login("Organiser");var response=await client.PostAsJsonAsync("/api/events",new EventRequest("Test Event",null,DateTime.UtcNow.AddDays(10),"Venue","City","Province","Running"));Assert.Equal(HttpStatusCode.Created,response.StatusCode);}
    [Fact] public async Task ParticipantCannotAccessOrganiserEnrolments(){var organiser=await Login("Organiser");var create=await organiser.PostAsJsonAsync("/api/events",new EventRequest("Access Test",null,DateTime.UtcNow.AddDays(10),"Venue","City","Province","Running"));var eventData=await create.Content.ReadFromJsonAsync<EventResponse>();var participant=await Login("Participant");var response=await participant.GetAsync($"/api/events/{eventData!.EventId}/enrolments");Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);}
}
