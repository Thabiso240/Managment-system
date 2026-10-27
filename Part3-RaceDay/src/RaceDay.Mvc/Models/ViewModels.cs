using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "Participant";

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "First name is required.")]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Participant";
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "First name is required.")]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone number")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public string? Phone { get; set; }

    [Display(Name = "Date of birth")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? City { get; set; }

    [Display(Name = "About me")]
    [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
    public string? Bio { get; set; }

    public string Role { get; set; } = "Participant";

    public string? ProfilePictureUrl { get; set; }
}

public class EventFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Event name is required.")]
    [Display(Name = "Event name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "A description is required.")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Event date")]
    public DateTime Date { get; set; } = DateTime.Today.AddDays(30);

    [Required(ErrorMessage = "Location is required.")]
    public string Location { get; set; } = string.Empty;

    [Range(0.1, 500, ErrorMessage = "Distance must be greater than zero.")]
    [Display(Name = "Distance (km)")]
    public decimal DistanceKm { get; set; } = 10;

    [Required]
    [Display(Name = "Event type")]
    public string EventType { get; set; } = "Road";

    public string? BannerImageUrl { get; set; }

    public bool IsEdit => Id > 0;
}

public class CategoryFormViewModel
{
    public int Id { get; set; }
    public int EventId { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [Display(Name = "Category name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Age group")]
    public string? AgeGroup { get; set; }

    [Display(Name = "Distance (km)")]
    public decimal? DistanceKm { get; set; }

    [Display(Name = "Entry fee (R)")]
    public decimal? EntryFee { get; set; }

    [Display(Name = "Maximum entries")]
    public int? MaxEntries { get; set; }
}

public class EventFilterViewModel
{
    public string? Search { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public bool UpcomingOnly { get; set; }
}
