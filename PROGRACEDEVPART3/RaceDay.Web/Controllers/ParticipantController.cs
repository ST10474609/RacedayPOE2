using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
namespace RaceDay.Web.Controllers;
public class ParticipantController(RaceDayApiClient api):Controller
{
    private bool SignedIn=>HttpContext.Session.GetString("Role")=="Participant";
    public async Task<IActionResult> MyEvents(){if(!SignedIn)return RedirectToAction("Login","Account");return View(await api.GetMyEnrolmentsAsync()??[]);}
    public async Task<IActionResult> MyResults(){if(!SignedIn)return RedirectToAction("Login","Account");return View(await api.GetMyResultsAsync()??[]);}
}