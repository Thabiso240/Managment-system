namespace RaceDay.Api.Models;

/// <summary>
/// A finish result captured by an Organiser after an event concludes and
/// published so the Participant can view it in "My Results".
/// </summary>
public class RaceResult
{
    public int Id { get; set; }

    public int EnrolmentId { get; set; }
    public Enrolment? Enrolment { get; set; }

    /// <summary>Recorded finishing time.</summary>
    public TimeSpan FinishTime { get; set; }

    /// <summary>Finishing position within the event (e.g. 47).</summary>
    public int Position { get; set; }

    /// <summary>Total number of finishers in the event, for "47th out of 312".</summary>
    public int TotalFinishers { get; set; }

    /// <summary>Only published results are visible to Participants.</summary>
    public bool IsPublished { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
