using System.ComponentModel.DataAnnotations;

public class UpdateProfileViewModel
{
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? Email { get; set; }

    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Phone number must be in the format xxx-xxx-xxxx.")]
    public string PhoneNumber { get; set; } = string.Empty; // Default to empty string to allow empty input
}

