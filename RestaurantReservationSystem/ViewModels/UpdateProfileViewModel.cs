using System.ComponentModel.DataAnnotations;
public class UpdateProfileViewModel
{
    [EmailAddress]
    public string Email { get; set; }

    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }  // Make Password optional

    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }  // Make ConfirmPassword optional

    public string? FirstName { get; set; }  // Make FirstName optional

    public string? LastName { get; set; }  // Make LastName optional

    [Phone]
    public string? PhoneNumber { get; set; }  // Make PhoneNumber optional
}
