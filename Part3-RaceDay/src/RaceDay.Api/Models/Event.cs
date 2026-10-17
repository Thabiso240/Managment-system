namespace RaceDay.Api.Models;

/// <summary>A running event created and managed by an Organiser.</summary>
public class Event
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Date on which the event takes place.</summary>
    public DateTime Date { get; set; }

    public string Location { get; set; } = string.Empty;

    /// <summary>Primary distance of the event in kilometres.</summary>
    public decimal DistanceKm { get; set; }

    public EventType EventType { get; set; } = EventType.Road;

    /// <summary>Absolute URL of the banner image stored in Azure Blob Storage.</summary>
    public string? BannerImageUrl { get; set; }

    public int OrganiserId { get; set; }
    public User? Organiser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
