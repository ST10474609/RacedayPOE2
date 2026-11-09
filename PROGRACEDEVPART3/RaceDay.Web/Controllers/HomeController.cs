using Microsoft.AspNetCore.Mvc;
namespace RaceDay.Web.Controllers;
public class HomeController:Controller
{
    [ResponseCache(Duration=0,Location=ResponseCacheLocation.None,NoStore=true)]
    public IActionResult Error()=>View();
}