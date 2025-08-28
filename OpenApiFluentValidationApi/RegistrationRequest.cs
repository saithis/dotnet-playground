using FluentValidation;

namespace OpenApiFluentValidationApi;


/// <summary>
/// A DTO representing a user registration request.
/// </summary>
public class RegistrationRequest
{
    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }
}

/// <summary>
/// The FluentValidation validator for the RegistrationRequest DTO.
/// </summary>
public class RegistrationRequestValidator : AbstractValidator<RegistrationRequest>
{
    public RegistrationRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(20)
            .WithMessage("Username must be between 5 and 20 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d]).{8,}$")
            .WithMessage("Password must be at least 8 characters long and include an uppercase letter, lowercase letter, number, and special character.");
    }
}
