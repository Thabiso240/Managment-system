namespace RaceDay.Api.Services;

/// <summary>Strongly typed binding for the "Jwt" configuration section.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "RaceDay.Api";
    public string Audience { get; set; } = "RaceDay.Mvc";
    public int ExpiryMinutes { get; set; } = 480;
}

/// <summary>Strongly typed binding for the "BlobStorage" configuration section.</summary>
public class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    /// <summary>Azure Storage connection string. When empty the API falls back to local disk storage.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    public string ContainerName { get; set; } = "raceday-images";
}
