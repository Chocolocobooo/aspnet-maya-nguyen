using System.ComponentModel.DataAnnotations;

namespace CoreFitnessClub.Application.DTOs;

// ── Auth ─────────────────────────────────────────────────────────────────────

public class RegisterDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

// ── Profile ───────────────────────────────────────────────────────────────────

public class UpdateProfileDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Enter a valid phone number")]
    public string? PhoneNumber { get; set; }
}

// ── Membership ────────────────────────────────────────────────────────────────

public class MembershipPlanDto
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string  Description { get; set; } = string.Empty;
    public decimal Price       { get; set; }
    public int     MaxClasses  { get; set; }
    public List<string> Features { get; set; } = [];
}

// ── Booking ───────────────────────────────────────────────────────────────────

public class BookingDto
{
    public int      Id            { get; set; }
    public string   ClassName     { get; set; } = string.Empty;
    public string   Category      { get; set; } = string.Empty;
    public DateTime ClassDateTime { get; set; }
    public string   Status        { get; set; } = string.Empty;
}

public class CreateBookingDto
{
    [Required]
    public int      FitnessClassId { get; set; }

    [Required]
    public DateTime ClassDateTime  { get; set; }
}

// ── Contact ───────────────────────────────────────────────────────────────────

public class ContactDto
{
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Message is required")]
    [MinLength(10, ErrorMessage = "Message must be at least 10 characters")]
    public string Message { get; set; } = string.Empty;
}
