using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };
    private const long MaxImageBytes = 5 * 1024 * 1024;

    private readonly RaceDayDbContext _db;
    private readonly IFileStorageService _storage;

    public UsersController(RaceDayDbContext db, IFileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    /// <summary>Current user's profile.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var user = await _db.Users.FindAsync(User.GetUserId());
        return user is null ? NotFound() : Ok(user.ToDto());
    }

    /// <summary>Updates the current user's personal information.</summary>
    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var user = await _db.Users.FindAsync(User.GetUserId());
        if (user is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(new { message = "First name and last name are required." });
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Phone = request.Phone;
        user.DateOfBirth = request.DateOfBirth;
        user.Gender = request.Gender;
        user.City = request.City;
        user.Bio = request.Bio;

        await _db.SaveChangesAsync();
        return Ok(user.ToDto());
    }

    /// <summary>Uploads a profile picture. The file is stored through the API's blob service.</summary>
    [HttpPost("me/photo")]
    public async Task<ActionResult<UserDto>> UploadPhoto(IFormFile file)
    {
        var error = ValidateImage(file);
        if (error is not null) return BadRequest(new { message = error });

        var user = await _db.Users.FindAsync(User.GetUserId());
        if (user is null) return NotFound();

        await using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(stream, file.FileName, file.ContentType, "profiles");

        user.ProfilePictureUrl = url;
        await _db.SaveChangesAsync();

        return Ok(user.ToDto());
    }

    private static string? ValidateImage(IFormFile? file)
    {
        if (file is null || file.Length == 0) return "Please choose an image file to upload.";
        if (file.Length > MaxImageBytes) return "The image must be 5 MB or smaller.";
        if (!AllowedImageTypes.Contains(file.ContentType.ToLowerInvariant())) return "Only JPG, PNG, WEBP or GIF images are supported.";
        return null;
    }
}
