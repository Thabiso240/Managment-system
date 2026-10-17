namespace RaceDay.Api.Models;

/// <summary>
/// An age, distance or entry category that a Participant selects when enrolling
/// (for example "Senior Male 21km" or "Junior 5km").
/// </summary>
public class Category
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Optional age bracket, e.g. "18-39".</summary>
    public string? AgeGroup { get; set; }

    /// <summary>Optional distance for multi-distance events.</summary>
    public decimal? DistanceKm { get; set; }

    public decimal? EntryFee { get; set; }

    /// <summary>Optional cap on the number of entries allowed in this category.</summary>
    public int? MaxEntries { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
