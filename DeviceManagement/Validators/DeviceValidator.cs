using DeviceManagement.Models;
using FluentValidation;

namespace DeviceManagement.Validators;

public class DeviceValidator : AbstractValidator<Device>
{
    public DeviceValidator()
    {
        RuleFor(x => x.SerialNumber).NotEmpty();
        RuleFor(x => x.PrimaryUser).EmailAddress();
        RuleFor(x => x.DeviceType).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
    }
}
