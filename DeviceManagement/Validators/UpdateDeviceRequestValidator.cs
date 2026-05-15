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

        RuleFor(x => x.DeviceType)
            .Must(v => Enum.IsDefined(typeof(DeviceType), v!.Value))
            .When(x => x.DeviceType.HasValue);

        RuleFor(x => x.Status)
            .Must(v => Enum.IsDefined(typeof(Status), v!.Value))
            .When(x => x.Status.HasValue);
    }
}
