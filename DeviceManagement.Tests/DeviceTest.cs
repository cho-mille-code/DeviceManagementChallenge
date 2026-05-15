using DeviceManagement.Models;
using DeviceManagement.Validators;
using FluentValidation.TestHelper;

namespace DeviceManagement.Tests;

public class DeviceTest
{
    private readonly DeviceValidator _validator = new();

    private static Device ValidRequest() => new()
    {
        SerialNumber = Guid.NewGuid(),
        ModelId = "MODEL-001",
        ModelName = "Test Device",
        Manufacturer = "ACME Corp",
        PrimaryUser = "user@example.com",
        OperatingSystem = "Windows 11",
        DeviceType = DeviceType.Laptop,
        Status = Status.Active
    };

    [Fact]
    public void ValidDevice_PassesValidation()
    {
        var result = _validator.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- SerialNumber ---

    [Fact]
    public void SerialNumber_Empty_FailsValidation()
    {
        var request = ValidRequest() with { SerialNumber = Guid.Empty };
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.SerialNumber);
    }

    [Fact]
    public void SerialNumber_Valid_PassesValidation()
    {
        var request = ValidRequest() with { SerialNumber = Guid.NewGuid() };
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.SerialNumber);
    }

    // --- PrimaryUser ---

    [Fact]
    public void PrimaryUser_MalformedEmail_FailsValidation()
    {
        var request = ValidRequest() with { PrimaryUser = "bob@" };
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.PrimaryUser);
    }

    [Fact]
    public void PrimaryUser_ValidEmail_PassesValidation()
    {
        var request = ValidRequest() with { PrimaryUser = "bob@example.com" };
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.PrimaryUser);
    }

    // --- DeviceType ---

    [Theory]
    [InlineData(DeviceType.Laptop)]
    [InlineData(DeviceType.Desktop)]
    public void DeviceType_ValidValue_PassesValidation(DeviceType value)
    {
        var request = ValidRequest() with { DeviceType = value };
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.DeviceType);
    }

    [Fact]
    public void DeviceType_UndefinedEnumValue_FailsValidation()
    {
        var request = ValidRequest() with { DeviceType = (DeviceType)99 };
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.DeviceType);
    }

    // --- Status ---

    [Theory]
    [InlineData(Status.Active)]
    [InlineData(Status.Inactive)]
    [InlineData(Status.Retired)]
    public void Status_ValidValue_PassesValidation(Status value)
    {
        var request = ValidRequest() with { Status = value };
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Status_UndefinedEnumValue_FailsValidation()
    {
        var request = ValidRequest() with { Status = (Status)99 };
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Status);
    }
}
