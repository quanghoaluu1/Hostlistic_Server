using EventService_Application.DTOs;
using FluentValidation;

namespace EventService_Api.Validator;

public class PostponeEventValidator : AbstractValidator<PostponeEventRequest>
{
    public PostponeEventValidator()
    {
        RuleFor(x => x.NewStartTime)
            .NotNull().WithMessage("NewStartTime is required.");

        RuleFor(x => x.NewEndTime)
            .NotNull().WithMessage("NewEndTime is required.")
            .GreaterThan(x => x.NewStartTime)
            .WithMessage("NewEndTime must be after NewStartTime.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
