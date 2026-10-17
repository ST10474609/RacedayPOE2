namespace RaceDay.Api.Models;
public class EventRoute { public int RouteId { get; set; } public int EventId { get; set; } public string RouteName { get; set; } = ""; public string StartLocation { get; set; } = ""; public string FinishLocation { get; set; } = ""; public decimal RouteDistanceKm { get; set; } public string? RouteDescription { get; set; } public Event? Event { get; set; } }
