using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

/// <summary>
/// Creates the SQLite schema (if required) and seeds realistic demo data so the
/// MVC front end has something meaningful to display on first run.
/// </summary>
public static class DbSeeder
{
    /// <summary>Password shared by every seeded demo account.</summary>
    public const string DemoPassword = "Password123!";

    public static async Task SeedAsync(RaceDayDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync())
        {
            return;
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(DemoPassword);

        var organiser = new User
        {
            FirstName = "Thandi",
            LastName = "Mokoena",
            Email = "organiser@raceday.co.za",
            PasswordHash = hash,
            Role = UserRole.Organiser,
            Phone = "082 555 1200",
            City = "Johannesburg",
            Bio = "Race director for the Gauteng running series."
        };

        var organiser2 = new User
        {
            FirstName = "Sipho",
            LastName = "Ndlovu",
            Email = "sipho@raceday.co.za",
            PasswordHash = hash,
            Role = UserRole.Organiser,
            Phone = "083 555 8842",
            City = "Cape Town",
            Bio = "Trail event coordinator."
        };

        var participant = new User
        {
            FirstName = "Lerato",
            LastName = "Dlamini",
            Email = "participant@raceday.co.za",
            PasswordHash = hash,
            Role = UserRole.Participant,
            Phone = "071 555 3311",
            DateOfBirth = new DateTime(1996, 4, 12),
            Gender = "Female",
            City = "Pretoria",
            Bio = "Chasing a sub-4 marathon.",
            ProfilePictureUrl = "https://coresg-normal.trae.ai/api/ide/v1/text_to_image?prompt=portrait%20of%20a%20smiling%20female%20runner%20in%20athletic%20wear%2C%20natural%20light%20headshot&image_size=square"
        };

        var participant2 = new User
        {
            FirstName = "James",
            LastName = "van Wyk",
            Email = "james@raceday.co.za",
            PasswordHash = hash,
            Role = UserRole.Participant,
            DateOfBirth = new DateTime(1988, 9, 3),
            Gender = "Male",
            City = "Durban"
        };

        var participant3 = new User
        {
            FirstName = "Aisha",
            LastName = "Patel",
            Email = "aisha@raceday.co.za",
            PasswordHash = hash,
            Role = UserRole.Participant,
            DateOfBirth = new DateTime(2001, 1, 25),
            Gender = "Female",
            City = "Cape Town"
        };

        db.Users.AddRange(organiser, organiser2, participant, participant2, participant3);
        await db.SaveChangesAsync();

        // ----- Events -----
        // Demo banner images. In production these come from Azure Blob Storage
        // after an Organiser uploads them through the API.
        const string bannerStyle = "&image_size=landscape_4_3";
        const string imageBase = "https://coresg-normal.trae.ai/api/ide/v1/text_to_image?prompt=";
        var cityMarathonBanner = imageBase + "road%20marathon%20runners%20through%20city%20streets%20at%20sunrise%2C%20energetic%20sports%20photography" + bannerStyle;
        var springHalfBanner = imageBase + "half%20marathon%20runners%20on%20an%20open%20highveld%20road%2C%20spring%20morning%2C%20sports%20photography" + bannerStyle;
        var trailRunBanner = imageBase + "trail%20runner%20on%20Table%20Mountain%2C%20dramatic%20mountain%20landscape%2C%20sports%20photography" + bannerStyle;
        var funRunBanner = imageBase + "families%20running%20a%205km%20fun%20run%20in%20a%20park%2C%20cheerful%20and%20colourful%2C%20sports%20photography" + bannerStyle;

        var cityMarathon = new Event
        {
            Name = "Johannesburg City Marathon",
            Description = "A fast, flat city marathon through the heart of Johannesburg. Ideal for a personal best.",
            Date = DateTime.UtcNow.Date.AddDays(-21),
            Location = "Johannesburg, Gauteng",
            DistanceKm = 42.2m,
            EventType = EventType.Road,
            OrganiserId = organiser.Id,
            BannerImageUrl = cityMarathonBanner
        };

        var springHalf = new Event
        {
            Name = "Highveld Spring Half Marathon",
            Description = "21.1km of rolling Highveld hills with water points every 3km.",
            Date = DateTime.UtcNow.Date.AddDays(30),
            Location = "Centurion, Gauteng",
            DistanceKm = 21.1m,
            EventType = EventType.Road,
            OrganiserId = organiser.Id,
            BannerImageUrl = springHalfBanner
        };

        var trailRun = new Event
        {
            Name = "Table Mountain Trail Challenge",
            Description = "A technical 15km trail run with 900m of climbing and breathtaking views.",
            Date = DateTime.UtcNow.Date.AddDays(45),
            Location = "Cape Town, Western Cape",
            DistanceKm = 15m,
            EventType = EventType.Trail,
            OrganiserId = organiser2.Id,
            BannerImageUrl = trailRunBanner
        };

        var funRun = new Event
        {
            Name = "Family Fun Run 5km",
            Description = "A relaxed, family-friendly 5km walk or run. All ages welcome.",
            Date = DateTime.UtcNow.Date.AddDays(14),
            Location = "Durban, KwaZulu-Natal",
            DistanceKm = 5m,
            EventType = EventType.FunRun,
            OrganiserId = organiser2.Id,
            BannerImageUrl = funRunBanner
        };

        db.Events.AddRange(cityMarathon, springHalf, trailRun, funRun);
        await db.SaveChangesAsync();

        // ----- Categories -----
        var catSeniorMale = new Category { EventId = cityMarathon.Id, Name = "Senior Male 42km", AgeGroup = "18-39", DistanceKm = 42.2m, EntryFee = 450m, MaxEntries = 500 };
        var catSeniorFemale = new Category { EventId = cityMarathon.Id, Name = "Senior Female 42km", AgeGroup = "18-39", DistanceKm = 42.2m, EntryFee = 450m, MaxEntries = 500 };
        var catMasters = new Category { EventId = cityMarathon.Id, Name = "Masters 42km", AgeGroup = "40+", DistanceKm = 42.2m, EntryFee = 450m, MaxEntries = 300 };

        var catHalf = new Category { EventId = springHalf.Id, Name = "Open 21km", DistanceKm = 21.1m, EntryFee = 300m, MaxEntries = 800 };
        var catHalfJunior = new Category { EventId = springHalf.Id, Name = "Junior 21km", AgeGroup = "16-17", DistanceKm = 21.1m, EntryFee = 150m, MaxEntries = 100 };

        var catTrail = new Category { EventId = trailRun.Id, Name = "Open 15km Trail", DistanceKm = 15m, EntryFee = 350m, MaxEntries = 400 };

        var catFun = new Category { EventId = funRun.Id, Name = "Open 5km", DistanceKm = 5m, EntryFee = 80m, MaxEntries = 1000 };

        db.Categories.AddRange(catSeniorMale, catSeniorFemale, catMasters, catHalf, catHalfJunior, catTrail, catFun);
        await db.SaveChangesAsync();

        // ----- Enrolments -----
        var e1 = new Enrolment { EventId = cityMarathon.Id, ParticipantId = participant.Id, CategoryId = catSeniorFemale.Id, Status = EnrolmentStatus.Confirmed, EnrolledAt = DateTime.UtcNow.AddDays(-60) };
        var e2 = new Enrolment { EventId = cityMarathon.Id, ParticipantId = participant2.Id, CategoryId = catSeniorMale.Id, Status = EnrolmentStatus.Confirmed, EnrolledAt = DateTime.UtcNow.AddDays(-55) };
        var e3 = new Enrolment { EventId = cityMarathon.Id, ParticipantId = participant3.Id, CategoryId = catSeniorFemale.Id, Status = EnrolmentStatus.Confirmed, EnrolledAt = DateTime.UtcNow.AddDays(-50) };

        var e4 = new Enrolment { EventId = springHalf.Id, ParticipantId = participant.Id, CategoryId = catHalf.Id, Status = EnrolmentStatus.Confirmed, EnrolledAt = DateTime.UtcNow.AddDays(-10) };
        var e5 = new Enrolment { EventId = springHalf.Id, ParticipantId = participant2.Id, CategoryId = catHalf.Id, Status = EnrolmentStatus.Pending, EnrolledAt = DateTime.UtcNow.AddDays(-3) };

        var e6 = new Enrolment { EventId = trailRun.Id, ParticipantId = participant3.Id, CategoryId = catTrail.Id, Status = EnrolmentStatus.Confirmed, EnrolledAt = DateTime.UtcNow.AddDays(-5) };

        var e7 = new Enrolment { EventId = funRun.Id, ParticipantId = participant.Id, CategoryId = catFun.Id, Status = EnrolmentStatus.Pending, EnrolledAt = DateTime.UtcNow.AddDays(-1) };

        db.Enrolments.AddRange(e1, e2, e3, e4, e5, e6, e7);
        await db.SaveChangesAsync();

        // ----- Published results for the completed marathon -----
        db.Results.AddRange(
            new RaceResult { EnrolmentId = e1.Id, FinishTime = new TimeSpan(3, 58, 12), Position = 47, TotalFinishers = 312, IsPublished = true, RecordedAt = DateTime.UtcNow.AddDays(-20) },
            new RaceResult { EnrolmentId = e2.Id, FinishTime = new TimeSpan(3, 12, 44), Position = 8, TotalFinishers = 312, IsPublished = true, RecordedAt = DateTime.UtcNow.AddDays(-20) },
            new RaceResult { EnrolmentId = e3.Id, FinishTime = new TimeSpan(4, 22, 5), Position = 155, TotalFinishers = 312, IsPublished = true, RecordedAt = DateTime.UtcNow.AddDays(-20) });

        await db.SaveChangesAsync();
    }
}
