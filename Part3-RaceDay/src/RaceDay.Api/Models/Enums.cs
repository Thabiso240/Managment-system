namespace RaceDay.Api.Models;

/// <summary>
/// The two roles supported by the RaceDay platform.
/// Organisers create and manage events; Participants enter events and record results.
/// </summary>
public enum UserRole
{
    Participant = 0,
    Organiser = 1
}

/// <summary>Types of running event that can be created on the platform.</summary>
public enum EventType
{
    Road = 0,
    Trail = 1,
    CrossCountry = 2,
    FunRun = 3
}

/// <summary>Lifecycle of a Participant's enrolment in an event.</summary>
public enum EnrolmentStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2
}
