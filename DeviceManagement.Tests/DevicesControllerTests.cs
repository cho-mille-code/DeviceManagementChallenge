using DeviceManagement.Controllers;
using DeviceManagement.Models;
using DeviceManagement.Repositories;
using DeviceManagement.Validators;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManagement.Tests;

public class DevicesControllerTests
{
    private readonly FakeDeviceRepository _repository = new();
    private readonly DevicesController _controller;

    public DevicesControllerTests()
    {
        _controller = new DevicesController(
            _repository,
            new DeviceValidator(),
            new UpdateDeviceRequestValidator());
    }

    private static Device ValidDevice() => new()
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

    // --- GET by primary user ---

    [Fact]
    public async Task GetByPrimaryUser_ReturnsOnlyDevicesForThatUser()
    {
        var alice1 = ValidDevice() with { PrimaryUser = "alice@lego.com" };
        var alice2 = ValidDevice() with { PrimaryUser = "alice@lego.com" };
        var bob   = ValidDevice() with { PrimaryUser = "bob@lego.com" };
        await _repository.CreateAsync(alice1);
        await _repository.CreateAsync(alice2);
        await _repository.CreateAsync(bob);

        var result = await _controller.GetByPrimaryUser("alice@lego.com");

        var ok = Assert.IsType<OkObjectResult>(result);
        var devices = Assert.IsAssignableFrom<IEnumerable<Device>>(ok.Value).ToList();
        Assert.Equal(2, devices.Count);
        Assert.All(devices, d => Assert.Equal("alice@lego.com", d.PrimaryUser));
    }

    [Fact]
    public async Task GetByPrimaryUser_NoDevices_ReturnsEmptyList()
    {
        var result = await _controller.GetByPrimaryUser("nobody@lego.com");

        var ok = Assert.IsType<OkObjectResult>(result);
        var devices = Assert.IsAssignableFrom<IEnumerable<Device>>(ok.Value);
        Assert.Empty(devices);
    }

    [Fact]
    public async Task GetByPrimaryUser_MissingParameter_Returns400()
    {
        var result = await _controller.GetByPrimaryUser("");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // --- GET by serial number ---

    [Fact]
    public async Task Get_ExistingDevice_Returns200WithDevice()
    {
        var device = ValidDevice();
        await _repository.CreateAsync(device);

        var result = await _controller.Get(device.SerialNumber);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(device, ok.Value);
    }

    [Fact]
    public async Task Get_UnknownSerialNumber_Returns404()
    {
        var result = await _controller.Get(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    // --- POST ---

    [Fact]
    public async Task Post_ValidDevice_Returns201WithLocation()
    {
        var device = ValidDevice();

        var result = await _controller.Create(device);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(device, created.Value);
    }

    [Fact]
    public async Task Post_InvalidEmail_Returns400()
    {
        var device = ValidDevice() with { PrimaryUser = "not-an-email" };

        var result = await _controller.Create(device);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Post_EmptySerialNumber_Returns400()
    {
        var device = ValidDevice() with { SerialNumber = Guid.Empty };

        var result = await _controller.Create(device);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // --- PUT ---

    [Fact]
    public async Task Put_ExistingDevice_Returns200WithUpdatedFields()
    {
        var device = ValidDevice();
        await _repository.CreateAsync(device);

        var update = new UpdateDeviceRequest
        {
            PrimaryUser = "new@example.com",
            OperatingSystem = "macOS",
            DeviceType = DeviceType.Desktop,
            Status = Status.Inactive
        };

        var result = await _controller.Update(device.SerialNumber, update);

        var ok = Assert.IsType<OkObjectResult>(result);
        var updated = Assert.IsType<Device>(ok.Value);
        Assert.Equal("new@example.com", updated.PrimaryUser);
        Assert.Equal("macOS", updated.OperatingSystem);
        Assert.Equal(DeviceType.Desktop, updated.DeviceType);
        Assert.Equal(Status.Inactive, updated.Status);
    }

    [Fact]
    public async Task Put_ExistingDevice_ImmutableFieldsArePreserved()
    {
        var device = ValidDevice();
        await _repository.CreateAsync(device);

        var update = new UpdateDeviceRequest { PrimaryUser = "new@example.com" };
        var result = await _controller.Update(device.SerialNumber, update);

        var ok = Assert.IsType<OkObjectResult>(result);
        var updated = Assert.IsType<Device>(ok.Value);

        Assert.Equal(device.SerialNumber, updated.SerialNumber);
        Assert.Equal(device.ModelId, updated.ModelId);
        Assert.Equal(device.ModelName, updated.ModelName);
        Assert.Equal(device.Manufacturer, updated.Manufacturer);
    }

    [Fact]
    public async Task Put_PartialUpdate_OnlyChangesProvidedFields()
    {
        var device = ValidDevice();
        await _repository.CreateAsync(device);

        var update = new UpdateDeviceRequest { Status = Status.Retired };
        var result = await _controller.Update(device.SerialNumber, update);

        var ok = Assert.IsType<OkObjectResult>(result);
        var updated = Assert.IsType<Device>(ok.Value);
        Assert.Equal(Status.Retired, updated.Status);
        Assert.Equal(device.PrimaryUser, updated.PrimaryUser);
        Assert.Equal(device.OperatingSystem, updated.OperatingSystem);
        Assert.Equal(device.DeviceType, updated.DeviceType);
    }

    [Fact]
    public async Task Put_UnknownSerialNumber_Returns404()
    {
        var result = await _controller.Update(Guid.NewGuid(), new UpdateDeviceRequest());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Put_InvalidEmail_Returns400()
    {
        var device = ValidDevice();
        await _repository.CreateAsync(device);

        var update = new UpdateDeviceRequest { PrimaryUser = "not-an-email" };
        var result = await _controller.Update(device.SerialNumber, update);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}

internal class FakeDeviceRepository : IDeviceRepository
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, Device> _store = new();

    public Task<Device?> GetBySerialNumberAsync(Guid serialNumber)
    {
        _store.TryGetValue(serialNumber, out var device);
        return Task.FromResult(device);
    }

    public Task<IReadOnlyList<Device>> GetByPrimaryUserAsync(string primaryUser)
        => Task.FromResult<IReadOnlyList<Device>>(_store.Values.Where(d => d.PrimaryUser == primaryUser).ToList());

    public Task<Device> CreateAsync(Device device)
    {
        _store[device.SerialNumber] = device;
        return Task.FromResult(device);
    }

    public Task<Device?> UpdateAsync(Guid serialNumber, UpdateDeviceRequest request)
    {
        if (!_store.TryGetValue(serialNumber, out var existing))
            return Task.FromResult<Device?>(null);

        var updated = existing with
        {
            PrimaryUser = request.PrimaryUser ?? existing.PrimaryUser,
            OperatingSystem = request.OperatingSystem ?? existing.OperatingSystem,
            DeviceType = request.DeviceType ?? existing.DeviceType,
            Status = request.Status ?? existing.Status
        };

        _store[serialNumber] = updated;
        return Task.FromResult<Device?>(updated);
    }
}
