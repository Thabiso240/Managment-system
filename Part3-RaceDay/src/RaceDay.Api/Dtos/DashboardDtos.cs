namespace RaceDay.Api.Dtos;

// ---- Organiser dashboard ----

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
