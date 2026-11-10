using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
namespace RaceDay.Web.Controllers;
public class EventsController(RaceDayApiClient api):Controller
{
    public async Task<IActionResult> Index(){ViewBag.Role=HttpContext.Session.GetString("Role");return View(await api.GetEventsAsync()??[]);}
    public async Task<IActionResult> Details(int id){var item=await api.GetEventAsync(id);return item is null?NotFound():View(item);}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Enrol(int categoryId,string? emergencyContactName,string? emergencyContactPhone,int eventId)
    {
        if(HttpContext.Session.GetString("Role")!="Participant")return RedirectToAction("Login","Account");
        var result=await api.EnrolAsync(new EnrolmentRequest(categoryId,emergencyContactName,emergencyContactPhone));
        TempData[result.Success?"Message":"Error"]=result.Success?"Your entry has been recorded.":result.Error;
        return RedirectToAction(nameof(Details),new{id=eventId});
    }
}