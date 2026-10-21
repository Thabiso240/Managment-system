namespace RaceDay.Api.Dtos;

// ---- Authentication / profile ----

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);

public record UserDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string? Phone,
    DateTime? DateOfBirth,
    string? Gender,
    string? City,
    string? Bio,
    string? ProfilePictureUrl,
    DateTime CreatedAt);

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? Phone,
    DateTime? DateOfBirth,
    string? Gender,
    string? City,
    string? Bio);
