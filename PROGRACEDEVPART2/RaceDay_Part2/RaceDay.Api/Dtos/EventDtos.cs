using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Dtos;
public record EventRequest([Required,MaxLength(150)] string EventName,[MaxLength(1000)] string? Description,DateTime EventDate,[Required,MaxLength(200)] string Venue,[Required,MaxLength(100)] string City,[Required,MaxLength(100)] string Province,[Required] string EventType,string Status="Upcoming");
public record EventResponse(int EventId,int OrganiserId,string EventName,string? Description,DateTime EventDate,string Venue,string City,string Province,string EventType,string Status,DateTime CreatedAt);
public record EventDetailsResponse(EventResponse Event,IEnumerable<CategoryResponse> Categories,IEnumerable<RouteResponse> Routes);
public record RouteRequest([Required,MaxLength(150)] string RouteName,[Required,MaxLength(200)] string StartLocation,[Required,MaxLength(200)] string FinishLocation,decimal RouteDistanceKm,[MaxLength(1000)] string? RouteDescription);
public record RouteResponse(int RouteId,int EventId,string RouteName,string StartLocation,string FinishLocation,decimal RouteDistanceKm,string? RouteDescription);
public record CategoryRequest([Required,MaxLength(100)] string CategoryName,decimal DistanceKm,decimal EntryFee,int? MaximumParticipants);
public record CategoryResponse(int CategoryId,int EventId,string CategoryName,decimal DistanceKm,decimal EntryFee,int? MaximumParticipants);
