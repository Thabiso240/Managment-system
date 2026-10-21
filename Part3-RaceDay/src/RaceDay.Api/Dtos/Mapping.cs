using RaceDay.Api.Models;

namespace RaceDay.Api.Dtos;

/// <summary>Converts domain entities into transfer objects returned by the API.</summary>
public static class Mapping
{
    public static UserDto ToDto(this User u) => new(
        u.Id,
        u.FirstName,
        u.LastName,
        u.Email,
        u.Role.ToString(),
        u.Phone,
        u.DateOfBirth,
        u.Gender,
        u.City,
        u.Bio,
        u.ProfilePictureUrl,
        u.CreatedAt);

    public static CategoryDto ToDto(this Category c) => new(
        c.Id,
        c.EventId,
        c.Name,
        c.AgeGroup,
        c.DistanceKm,
        c.EntryFee,
        c.MaxEntries,
        c.Enrolments?.Count ?? 0);

    public static EventDto ToDto(this Event e) => new(
        e.Id,
        e.Name,
        e.Description,
        e.Date,
        e.Location,
        e.DistanceKm,
        e.EventType.ToString(),
        e.BannerImageUrl,
        e.OrganiserId,
        e.Organiser?.FullName ?? string.Empty,
        e.Categories?.Count ?? 0,
        e.Enrolments?.Count ?? 0);

    public static EventDetailDto ToDetailDto(this Event e) => new(
        e.Id,
        e.Name,
        e.Description,
        e.Date,
        e.Location,
        e.DistanceKm,
        e.EventType.ToString(),
        e.BannerImageUrl,
        e.OrganiserId,
        e.Organiser?.FullName ?? string.Empty,
        e.Categories.Select(c => c.ToDto()).ToList());

    public static EnrolmentDto ToDto(this Enrolment en) => new(
        en.Id,
        en.EventId,
        en.Event?.Name ?? string.Empty,
        en.Event?.Date ?? default,
        en.Event?.Location ?? string.Empty,
        en.Event?.EventType.ToString() ?? string.Empty,
        en.Event?.BannerImageUrl,
        en.CategoryId,
        en.Category?.Name,
        en.Status.ToString(),
        en.EnrolledAt);

    public static EventEnrolmentDto ToOrganiserDto(this Enrolment en) => new(
        en.Id,
        en.ParticipantId,
        en.Participant?.FullName ?? string.Empty,
        en.Participant?.Email ?? string.Empty,
        en.Category?.Name,
        en.Status.ToString(),
        en.EnrolledAt,
        en.Result != null ? en.Result.FinishTime.ToString(@"hh\:mm\:ss") : null,
        en.Result?.Position,
        en.Result?.IsPublished ?? false);
}
