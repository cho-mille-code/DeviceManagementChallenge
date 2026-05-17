using DeviceManagement.Validations;
using DeviceManagement.Models;
using FluentValidation.TestHelper;

namespace DeviceManagement.Tests;

public class DeviceTests
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
        _validator.TestValidate(ValidRequest() with { SerialNumber = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.SerialNumber);
    }

    // --- Required string fields ---

    [Fact]
    public void ModelId_Empty_FailsValidation()
    {
        _validator.TestValidate(ValidRequest() with { ModelId = "" })
            .ShouldHaveValidationErrorFor(x => x.ModelId);
    }

    [Fact]
    public void ModelName_Empty_FailsValidation()
    {
        _validator.TestValidate(ValidRequest() with { ModelName = "" })
            .ShouldHaveValidationErrorFor(x => x.ModelName);
    }

    [Fact]
    public void Manufacturer_Empty_FailsValidation()
    {
        _validator.TestValidate(ValidRequest() with { Manufacturer = "" })
            .ShouldHaveValidationErrorFor(x => x.Manufacturer);
    }

    [Fact]
    public void OperatingSystem_Empty_FailsValidation()
    {
        _validator.TestValidate(ValidRequest() with { OperatingSystem = "" })
            .ShouldHaveValidationErrorFor(x => x.OperatingSystem);
    }

    // --- PrimaryUser ---

    [Fact]
    public void PrimaryUser_MalformedEmail_FailsValidation()
    {
        _validator.TestValidate(ValidRequest() with { PrimaryUser = "bob@" })
            .ShouldHaveValidationErrorFor(x => x.PrimaryUser);
    }
}
