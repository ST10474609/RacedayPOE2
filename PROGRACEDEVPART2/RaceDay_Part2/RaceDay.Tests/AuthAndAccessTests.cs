using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;
namespace RaceDay.Tests;
public class AuthAndAccessTests : IClassFixture<ApiFactory>
{
    readonly ApiFactory factory; public AuthAndAccessTests(ApiFactory factory)=>this.factory=factory;
    [Fact] public async Task ProtectedProfileRejectsUnauthenticatedRequest(){var client=factory.CreateClient();var response=await client.GetAsync("/api/users/me");Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode);}
    [Fact] public async Task RegistrationAndLoginCreateAuthenticatedSession(){var client=factory.CreateClient(new WebApplicationFactoryClientOptions{HandleCookies=true});var email=$"user{Guid.NewGuid():N}@example.com";var register=await client.PostAsJsonAsync("/api/auth/register",new RegisterRequest("Test","User",email,"Password123!","Participant","0710000000"));Assert.Equal(HttpStatusCode.Created,register.StatusCode);var login=await client.PostAsJsonAsync("/api/auth/login",new LoginRequest(email,"Password123!"));Assert.Equal(HttpStatusCode.OK,login.StatusCode);var profile=await client.GetAsync("/api/users/me");Assert.Equal(HttpStatusCode.OK,profile.StatusCode);}
    [Fact] public async Task InvalidPasswordIsRejected(){var client=factory.CreateClient();var email=$"user{Guid.NewGuid():N}@example.com";await client.PostAsJsonAsync("/api/auth/register",new RegisterRequest("Test","User",email,"Password123!","Participant",null));var login=await client.PostAsJsonAsync("/api/auth/login",new LoginRequest(email,"WrongPassword!"));Assert.Equal(HttpStatusCode.Unauthorized,login.StatusCode);}
}
