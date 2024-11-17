using DigitalSeal.Data.Models;
using FluentValidation;

namespace DigitalSeal.Core.Validators
{
    public class OrganizationValidator : AbstractValidator<Organization>
    {
        public OrganizationValidator()
        {
            //RuleFor(x => x.Name).NotNull().NotEmpty();
            Transform(org => org.Name, name => name?.Trim()).NotEmpty();
        }
    }
}
