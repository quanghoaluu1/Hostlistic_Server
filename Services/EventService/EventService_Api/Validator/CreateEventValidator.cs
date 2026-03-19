using EventService_Application.DTOs;
using EventService_Domain.Enums;
using FluentValidation;

namespace EventService_Api.Validator;

public class CreateEventValidator : AbstractValidator<EventRequestDto>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.EventTypeId)
            .NotNull().WithMessage("Event type is required");

        RuleFor(x => x.EventMode)
            .NotNull().WithMessage("Event mode is required")
            .IsInEnum().WithMessage("Event mode is invalid");

        RuleFor(x => x.StartDate)
            .NotNull().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotNull().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.TotalCapacity)
            .NotNull().WithMessage("Total capacity is required")
            .GreaterThan(0).WithMessage("Total capacity must be greater than 0");

        // Location required only for offline/hybrid
        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required for in-person events")
            .When(x => x.EventMode is EventMode.Offline or EventMode.Hybrid);
    }
}