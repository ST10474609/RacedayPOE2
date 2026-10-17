using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public record RegisterRequest(
    [Required, MinLength(2), MaxLength(50)] string FirstName,
    [Required, MinLength(2), MaxLength(50)] string LastName,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(8)] string Password,
    [Required, MaxLength(20)] string Role,
    [MaxLength(30)] string? PhoneNumber);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record UserResponse(
    int UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string? PhoneNumber,
    DateTime CreatedAt);

public record UpdateProfileRequest(
    [Required, MinLength(2), MaxLength(50)] string FirstName,
    [Required, MinLength(2), MaxLength(50)] string LastName,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [MaxLength(30)] string? PhoneNumber);
