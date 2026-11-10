using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
namespace RaceDay.Web.Controllers;
public class AccountController(RaceDayApiClient api) : Controller
{
    [HttpGet] public IActionResult Login()=>View();
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Login(string email,string password)
    {
        var result=await api.LoginAsync(new LoginRequest(email,password));
        if(result.User is null){ViewBag.Error=result.Error??"Unable to sign in.";return View();}
        HttpContext.Session.SetInt32("UserId",result.User.UserId);
        HttpContext.Session.SetString("Role",result.User.Role);
        HttpContext.Session.SetString("DisplayName",$"{result.User.FirstName} {result.User.LastName}");
        return RedirectToAction("Index",result.User.Role=="Organiser"?"Organiser":"Events");
    }
    [HttpGet] public IActionResult Register()=>View();
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Register(string firstName,string lastName,string email,string password,string? phoneNumber,string role)
    {
        var result=await api.RegisterAsync(new RegisterRequest(firstName,lastName,email,password,role,phoneNumber));
        if(!result.Success){ViewBag.Error=result.Error??"Registration failed.";return View();}
        TempData["Message"]="Account created. Sign in to continue.";return RedirectToAction(nameof(Login));
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Logout(){await api.LogoutAsync();return RedirectToAction(nameof(Login));}
}