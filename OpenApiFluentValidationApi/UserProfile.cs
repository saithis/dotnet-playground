using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace OpenApiFluentValidationApi;

/// <summary>
/// Example user profile class demonstrating Obsolete attributes at different levels.
/// </summary>
public class UserProfile
{
    public required string FirstName { get; set; }
    
    public required string LastName { get; set; }
    
    public required string Email { get; set; }
    
    [Obsolete("Use Email property instead. This property will be removed in v2.0")]
    public string? EmailAddress { get; set; }
    
    [Obsolete("This field is no longer supported", true)]
    public string? LegacyField { get; set; }
    
    [Obsolete]
    public DateTime? LastLoginDate { get; set; }
    
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// FluentValidation validator for UserProfile.
/// </summary>
public class UserProfileValidator : AbstractValidator<UserProfile>
{
    public UserProfileValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .Length(2, 50).WithMessage("First name must be between 2 and 50 characters");
            
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Please provide a valid email address");
            
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Please provide a valid phone number")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}

/// <summary>
/// Example of an entirely obsolete class.
/// </summary>
[Obsolete("This class has been replaced by UserProfile. Please use UserProfile instead.")]
public class OldUserModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
} 