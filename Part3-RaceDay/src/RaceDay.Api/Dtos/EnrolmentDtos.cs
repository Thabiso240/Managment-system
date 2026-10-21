namespace RaceDay.Api.Dtos;

// ---- Enrolments ----

public record CreateEnrolmentRequest(int? CategoryId);

/// <summary>An enrolment as seen by the Participant who owns it.</summary>
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

/// <summary>An enrolment as seen by the Organiser of the event.</summary>
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
