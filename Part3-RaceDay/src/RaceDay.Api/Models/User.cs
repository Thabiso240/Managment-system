namespace RaceDay.Api.Models;

/// <summary>
/// An application user. A single table stores both Organisers and Participants,
/// separated by <see cref="Role"/>.
/// </summary>
public class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>Unique login identifier and contact address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt hash - plain-text passwords are never stored.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Participant;

    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? City { get; set; }
    public string? Bio { get; set; }

    /// <summary>Absolute URL of the profile picture stored in blob storage.</summary>
    public string? ProfilePictureUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
