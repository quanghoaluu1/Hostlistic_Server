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
            .NotNull().WithMessage("Start date is required")
            .Must(start => start!.Value >= DateTime.UtcNow)
            .WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .NotNull().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.TotalCapacity)
            .NotNull().WithMessage("Total capacity is required")
            .GreaterThan(0).WithMessage("Total capacity must be greater than 0");

        // Location required only for offline/hybrid
        RuleFor(x => x.LocationAddress)
            .NotEmpty().WithMessage("Location Address is required for in-person events")
            .MaximumLength(500).WithMessage("Location Address must not exceed 500 characters")
            .When(x => x.EventMode is EventMode.Offline or EventMode.Hybrid);

        RuleFor(x => x.Latitude)
            .NotNull().WithMessage("Latitude is required")
            .InclusiveBetween(-90.0, 90.0).WithMessage("Latitude must be between -90 and 90")
            .When(x => x.EventMode is EventMode.Offline or EventMode.Hybrid);

        RuleFor(x => x.Longitude)
            .NotNull().WithMessage("Longitude is required")
            .InclusiveBetween(-180.0, 180.0).WithMessage("Longitude must be between -180 and 180")
            .When(x => x.EventMode is EventMode.Offline or EventMode.Hybrid);
    }
}