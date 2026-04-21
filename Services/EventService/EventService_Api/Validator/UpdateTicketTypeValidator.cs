using EventService_Application.DTOs;
using FluentValidation;

namespace EventService_Api.Validator;

public class UpdateTicketTypeValidator : AbstractValidator<UpdateTicketTypeRequest>
{
    public UpdateTicketTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ticket type name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0.");

        RuleFor(x => x.QuantityAvailable)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity available must be greater than or equal to 0.");

        RuleFor(x => x.MinPerOrder)
            .GreaterThanOrEqualTo(0).WithMessage("MinPerOrder must be greater than or equal to 0.");

        RuleFor(x => x.MaxPerOrder)
            .GreaterThanOrEqualTo(0).WithMessage("MaxPerOrder must be greater than or equal to 0.")
            .GreaterThanOrEqualTo(x => x.MinPerOrder)
                .WithMessage("MaxPerOrder must be greater than or equal to MinPerOrder.");

        RuleFor(x => x.SaleEndTime)
            .GreaterThanOrEqualTo(x => x.SaleStartDate)
                .WithMessage("Sale end time must be after sale start date.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status value is invalid.");

        // Streaming benefits
        RuleFor(x => x.MaxQaQuestions)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MaxQaQuestions must be greater than or equal to 0.")
            .When(x => x.MaxQaQuestions.HasValue);

        RuleFor(x => x.AllowedTrackIds)
            .Must(ids => ids == null || ids.Distinct().Count() == ids.Count)
            .WithMessage("AllowedTrackIds must not contain duplicate GUIDs.");
    }
}
