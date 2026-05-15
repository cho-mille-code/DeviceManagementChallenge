using DeviceManagement.Models;
using FluentValidation;

namespace DeviceManagement.Validators;

public class UpdateDeviceRequestValidator : AbstractValidator<UpdateDeviceRequest>
{
    public UpdateDeviceRequestValidator()
    {
        RuleFor(x => x.PrimaryUser)
            .EmailAddress()
            .When(x => x.PrimaryUser is not null);
    }
}
