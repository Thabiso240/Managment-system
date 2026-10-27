namespace RaceDay.Mvc.Models;

// ---------------------------------------------------------------------------
// Data transfer objects mirrored from the RaceDay API.
// The MVC application owns no database access - it only speaks to the API.
// ---------------------------------------------------------------------------

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string FirstName, string LastName, string Email, string Password, string Role);

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
    DateTime CreatedAt)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? Phone,
    DateTime? DateOfBirth,
    string? Gender,
    string? City,
    string? Bio);

public record EventDto(
    int Id,
    string Name,
    string Description,
    DateTime Date,
    string Location,
    decimal DistanceKm,
    string EventType,
    string? BannerImageUrl,
    int OrganiserId,
    string OrganiserName,
    int CategoryCount,
    int EnrolmentCount);

public record CategoryDto(
    int Id,
    int EventId,
    string Name,
    string? AgeGroup,
    decimal? DistanceKm,
    decimal? EntryFee,
    int? MaxEntries,
    int EnrolmentCount);

public record EventDetailDto(
    int Id,
    string Name,
    string Description,
    DateTime Date,
    string Location,
    decimal DistanceKm,
    string EventType,
    string? BannerImageUrl,
    int OrganiserId,
    string OrganiserName,
    IReadOnlyList<CategoryDto> Categories);

public record CreateEventRequest(
    string Name,
    string Description,
    DateTime Date,
    string Location,
    decimal DistanceKm,
    string EventType);

public record UpdateEventRequest(
    string Name,
    string Description,
    DateTime Date,
    string Location,
    decimal DistanceKm,
    string EventType);

public record CreateCategoryRequest(
    string Name,
    string? AgeGroup,
    decimal? DistanceKm,
    decimal? EntryFee,
    int? MaxEntries);

public record UpdateCategoryRequest(
    string Name,
    string? AgeGroup,
    decimal? DistanceKm,
    decimal? EntryFee,
    int? MaxEntries);

public record CreateEnrolmentRequest(int? CategoryId);

public record EnrolmentDto(
    int Id,
    int EventId,
    string EventName,
    DateTime EventDate,
    string Location,
    string EventType,
    string? BannerImageUrl,
    int? CategoryId,
    string? CategoryName,
    string Status,
    DateTime EnrolledAt);

public record EventEnrolmentDto(
    int EnrolmentId,
    int ParticipantId,
    string ParticipantName,
    string Email,
    string? CategoryName,
    string Status,
    DateTime EnrolledAt,
    string? FinishTime,
    int? Position,
    bool ResultPublished);

public record RecordResultRequest(int EnrolmentId, string FinishTime, int Position);

public record PublishResultsRequest(int TotalFinishers);

public record ParticipantResultDto(
    int EventId,
    string EventName,
    DateTime EventDate,
    string Location,
    string EventType,
    string? CategoryName,
    string FinishTime,
    int Position,
    int TotalFinishers)
{
    public string PositionLabel => $"{Position}{Ordinal(Position)} out of {TotalFinishers} finishers";

    private static string Ordinal(int n)
    {
        if (n % 100 is 11 or 12 or 13) return "th";
        return (n % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }
}

public record OrganiserDashboardDto(
    int TotalEvents,
    int TotalEnrolments,
    int UpcomingEvents,
    IReadOnlyList<EventSummaryDto> Events);

public record EventSummaryDto(
    int EventId,
    string Name,
    DateTime Date,
    string Location,
    string EventType,
    int EnrolmentCount,
    int ConfirmedCount,
    int PendingCount,
    int CategoryCount,
    bool IsPast);

/// <summary>Supported event types, used to populate drop-down lists.</summary>
public static class EventTypes
{
    public static readonly string[] All = { "Road", "Trail", "CrossCountry", "FunRun" };

    public static string Label(string? value) => value switch
    {
        "Road" => "Road",
        "Trail" => "Trail",
        "CrossCountry" => "Cross Country",
        "FunRun" => "Fun Run",
        _ => value ?? string.Empty
    };
}
