using EventService_Application.DTOs;
using FluentValidation;

namespace EventService_Api.Validator;

public class CreateFeedbackValidator : AbstractValidator<CreateFeedbackDto>
{
    public CreateFeedbackValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("EventId is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters.");
    }
}

public class UpdateFeedbackValidator : AbstractValidator<UpdateFeedbackDto>
{
    public UpdateFeedbackValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters.");
    }
}
