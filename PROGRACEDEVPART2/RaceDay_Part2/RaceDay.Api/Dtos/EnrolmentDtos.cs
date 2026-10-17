using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Dtos;
public record EnrolmentRequest(int CategoryId,[MaxLength(100)] string? EmergencyContactName,[MaxLength(30)] string? EmergencyContactPhone);
public record EnrolmentResponse(int EnrolmentId,int ParticipantId,int CategoryId,string CategoryName,int EventId,string EventName,DateTime EnrolmentDate,string? EmergencyContactName,string? EmergencyContactPhone,string PaymentStatus,string EnrolmentStatus);
public record ResultRequest(int EnrolmentId,int? FinishPosition,int? FinishTimeSeconds,string ResultStatus="Finished");
public record ResultResponse(int ResultId,int EnrolmentId,int ParticipantId,string ParticipantName,int EventId,string EventName,int? FinishPosition,int? FinishTimeSeconds,string ResultStatus,DateTime RecordedAt);
