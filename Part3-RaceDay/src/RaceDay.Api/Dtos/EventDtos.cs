namespace RaceDay.Api.Dtos;

// ---- Events ----

public record EventDto(
    int Id,
    string Name,
    string Description,
    DateTime Date,
    string Location,
    decimal DistanceKm,
    string EventType,
    string? BannerImageUrl,
    int OrganiserId,
    string OrganiserName,
    int CategoryCount,
    int EnrolmentCount);

public record CategoryDto(
    int Id,
    int EventId,
    string Name,
    string? AgeGroup,
    decimal? DistanceKm,
    decimal? EntryFee,
    int? MaxEntries,
    int EnrolmentCount);

public record EventDetailDto(
    int Id,
    string Name,
    string Description,
    DateTime Date,
    string Location,
    decimal DistanceKm,
    string EventType,
    string? BannerImageUrl,
    int OrganiserId,
    string OrganiserName,
    IReadOnlyList<CategoryDto> Categories);

public record CreateEventRequest(
    string Name,
    string Description,
    DateTime Date,
    string Location,
    decimal DistanceKm,
    string EventType);

public record UpdateEventRequest(
    string Name,
    string Description,
    DateTime Date,
    string Location,
    decimal DistanceKm,
    string EventType);

public record CreateCategoryRequest(
    string Name,
    string? AgeGroup,
    decimal? DistanceKm,
    decimal? EntryFee,
    int? MaxEntries);

public record UpdateCategoryRequest(
    string Name,
    string? AgeGroup,
    decimal? DistanceKm,
    decimal? EntryFee,
    int? MaxEntries);
