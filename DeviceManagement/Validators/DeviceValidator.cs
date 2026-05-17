using DeviceManagement.Models;
using FluentValidation;

namespace DeviceManagement.Validators;

public class DeviceValidator : AbstractValidator<Device>
{
    public DeviceValidator()
    {
        RuleFor(x => x.SerialNumber).NotEmpty();
        RuleFor(x => x.ModelId).NotEmpty();
        RuleFor(x => x.ModelName).NotEmpty();
        RuleFor(x => x.Manufacturer).NotEmpty();
        RuleFor(x => x.PrimaryUser).NotEmpty().EmailAddress();
        RuleFor(x => x.OperatingSystem).NotEmpty();
        RuleFor(x => x.DeviceType).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
    }
}
