using BookingService_Application.DTOs;
using FluentValidation;

namespace BookingService_Application.Validators;

public sealed class AdminTransactionQueryRequestValidator : AbstractValidator<AdminTransactionQueryRequest>
{
    private static readonly string[] AllowedSortBy =
    [
        "CreatedAt",
        "Amount",
        "NetAmount",
        "PlatformFee",
        "BalanceAfter",
        "Status",
        "Type"
    ];

    public AdminTransactionQueryRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.SortBy)
            .NotEmpty()
            .Must(sortBy => AllowedSortBy.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortBy)}.");

        RuleFor(x => x)
            .Must(x => !x.FromDateUtc.HasValue || !x.ToDateUtc.HasValue || x.FromDateUtc <= x.ToDateUtc)
            .WithMessage("FromDateUtc must be less than or equal to ToDateUtc.");

        RuleFor(x => x)
            .Must(x => !x.MinAmount.HasValue || !x.MaxAmount.HasValue || x.MinAmount <= x.MaxAmount)
            .WithMessage("MinAmount must be less than or equal to MaxAmount.");
    }
}
