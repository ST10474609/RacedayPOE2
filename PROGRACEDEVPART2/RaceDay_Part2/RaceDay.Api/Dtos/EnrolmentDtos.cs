using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public record EnrolmentRequest(
    [Required] int CategoryId,
    [MaxLength(100)] string? EmergencyContactName,
    [MaxLength(30)] string? EmergencyContactPhone);

public record EnrolmentResponse(
    int EnrolmentId,
    int ParticipantId,
    int CategoryId,
    string CategoryName,
    int EventId,
    string EventName,
    DateTime EnrolmentDate,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string PaymentStatus,
    string EnrolmentStatus);

public record ResultRequest(
    int? FinishPosition,
    int? FinishTimeSeconds,
    string ResultStatus);

public record ResultResponse(
    int ResultId,
    int EnrolmentId,
    int ParticipantId,
    string ParticipantName,
    int EventId,
    string EventName,
    int? FinishPosition,
    int? FinishTimeSeconds,
    string ResultStatus,
    DateTime RecordedAt);
