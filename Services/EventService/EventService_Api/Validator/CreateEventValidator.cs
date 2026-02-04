using EventService_Application.DTOs;
using FluentValidation;

namespace EventService_Api.Validator;

public class CreateEventValidator : AbstractValidator<EventRequestDto>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required");

        RuleFor(x => x.CoverImageUrl)
            .NotEmpty().WithMessage("Cover Image is required");

        RuleFor(x => x.TotalCapacity)
            .GreaterThan(0).WithMessage("Total capacity must be greater than 0");

        RuleFor(x => x.EventTypeId)
            .NotNull().WithMessage("Event type is required");
        
        RuleFor(x => x.EventMode)
            .IsInEnum().WithMessage("Event mode is invalid");


        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be after start date");
    }
}