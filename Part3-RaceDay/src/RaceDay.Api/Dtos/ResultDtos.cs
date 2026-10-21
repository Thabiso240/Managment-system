namespace RaceDay.Api.Dtos;

// ---- Results ----

/// <summary>Finish time is submitted as text, e.g. "3:58:12" or "58:12".</summary>
public record RecordResultRequest(int EnrolmentId, string FinishTime, int Position);

public record PublishResultsRequest(int TotalFinishers);

/// <summary>A published result as displayed on the Participant's "My Results" page.</summary>
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
    /// <summary>Human readable position label, e.g. "47th out of 312 finishers".</summary>
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
