using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public record EventRequest(
    [Required, MinLength(2), MaxLength(150)] string EventName,
    [Required, MaxLength(1000)] string? Description,
    [Required] DateTime EventDate,
    [Required, MaxLength(200)] string Venue,
    [Required, MaxLength(100)] string City,
    [Required, MaxLength(100)] string Province,
    [Required, MaxLength(30)] string EventType,
    [Required, MaxLength(20)] string Status);

public record EventResponse(
    int EventId,
    int OrganiserId,
    string EventName,
    string? Description,
    DateTime EventDate,
    string Venue,
    string City,
    string Province,
    string EventType,
    string Status,
    DateTime CreatedAt);

public record RouteRequest(
    [Required, MinLength(2), MaxLength(150)] string RouteName,
    [Required, MaxLength(200)] string StartLocation,
    [Required, MaxLength(200)] string FinishLocation,
    [Required] decimal RouteDistanceKm,
    [MaxLength(1000)] string? RouteDescription);

public record RouteResponse(
    int RouteId,
    int EventId,
    string RouteName,
    string StartLocation,
    string FinishLocation,
    decimal RouteDistanceKm,
    string? RouteDescription);

public record CategoryRequest(
    [Required, MinLength(2), MaxLength(100)] string CategoryName,
    [Required] decimal DistanceKm,
    [Required] decimal EntryFee,
    int? MaximumParticipants);

public record CategoryResponse(
    int CategoryId,
    int EventId,
    string CategoryName,
    decimal DistanceKm,
    decimal EntryFee,
    int? MaximumParticipants);
