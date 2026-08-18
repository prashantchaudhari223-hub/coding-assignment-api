using System;
using FluentValidation;
using PharmacyInventory.Application.DTOs;

namespace PharmacyInventory.Application.Validators
{
    /// <summary>
    /// Replaces Data Annotations for CreateMedicineDto. Living in the Application layer keeps
    /// business validation rules alongside the use-case logic they protect, rather than
    /// scattering them across the API's request models.
    /// </summary>
    public class CreateMedicineDtoValidator : AbstractValidator<CreateMedicineDto>
    {
        public CreateMedicineDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters.");

            RuleFor(x => x.Brand)
                .NotEmpty().WithMessage("Brand is required.")
                .MaximumLength(200).WithMessage("Brand cannot exceed 200 characters.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");

            RuleFor(x => x.ExpiryDate)
                .NotEmpty().WithMessage("Expiry date is required.")
                .GreaterThan(DateTime.Today.AddYears(-50))
                    .WithMessage("Expiry date is not a plausible date.");            

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.")
                .PrecisionScale(18, 2, ignoreTrailingZeros: false)
                    .WithMessage("Price can have at most 2 decimal places.");
        }
    }
}
