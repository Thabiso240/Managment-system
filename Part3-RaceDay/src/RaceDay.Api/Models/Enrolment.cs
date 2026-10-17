namespace RaceDay.Api.Models;

/// <summary>Links a Participant to an Event (and optionally a Category).</summary>
public class Enrolment
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int ParticipantId { get; set; }
    public User? Participant { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public EnrolmentStatus Status { get; set; } = EnrolmentStatus.Pending;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    /// <summary>The result recorded by the Organiser after the event concludes.</summary>
    public RaceResult? Result { get; set; }
}
