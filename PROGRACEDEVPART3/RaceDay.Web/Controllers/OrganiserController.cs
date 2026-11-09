using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
namespace RaceDay.Web.Controllers;
public class OrganiserController(RaceDayApiClient api):Controller
{
    private bool IsOrganiser=>HttpContext.Session.GetString("Role")=="Organiser";
    public async Task<IActionResult> Index(){if(!IsOrganiser)return RedirectToAction("Login","Account");var events=await api.GetEventsAsync()??[];ViewBag.Upcoming=events.Count(x=>x.EventDate>=DateTime.Today&&x.Status=="Upcoming");return View(events);}
    [HttpGet] public IActionResult CreateEvent(){if(!IsOrganiser)return RedirectToAction("Login","Account");return View();}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> CreateEvent(EventRequest model)
    {
        if(!IsOrganiser)return Forbid();var result=await api.CreateEventAsync(model);
        if(!result.Success){ViewBag.Error=result.Error;return View(model);}
        TempData["Message"]="Event created successfully.";return RedirectToAction(nameof(Index));
    }
}