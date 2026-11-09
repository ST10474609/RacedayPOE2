namespace RaceDay.Web.Models;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string FirstName, string LastName, string Email, string Password, string Role, string? PhoneNumber);
public record UserResponse(int UserId, string FirstName, string LastName, string Email, string Role, string? PhoneNumber, DateTime CreatedAt);
public record EventResponse(int EventId, int OrganiserId, string EventName, string? Description, DateTime EventDate, string Venue, string City, string Province, string EventType, string Status, DateTime CreatedAt);
public record CategoryResponse(int CategoryId, int EventId, string CategoryName, decimal DistanceKm, decimal EntryFee, int? MaximumParticipants);
public record EventDetailsResponse(EventResponse Event, List<CategoryResponse> Categories, List<RouteResponse> Routes);
public record RouteResponse(int RouteId, int EventId, string RouteName, string StartLocation, string FinishLocation, decimal RouteDistanceKm, string? RouteDescription);
public record EnrolmentRequest(int CategoryId, string? EmergencyContactName, string? EmergencyContactPhone);
public record EnrolmentResponse(int EnrolmentId, int ParticipantId, int CategoryId, string CategoryName, int EventId, string EventName, DateTime EnrolmentDate, string? EmergencyContactName, string? EmergencyContactPhone, string PaymentStatus, string EnrolmentStatus);
public record ResultResponse(int ResultId, int EnrolmentId, int ParticipantId, string ParticipantName, int EventId, string EventName, int? FinishPosition, int? FinishTimeSeconds, string ResultStatus, DateTime RecordedAt);
public record EventRequest(string EventName, string? Description, DateTime EventDate, string Venue, string City, string Province, string EventType, string Status);
