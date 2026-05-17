using System.Collections.Concurrent;
using DeviceManagement.Models;
using DeviceManagement.Repositories;

namespace DeviceManagement.Tests;

internal class InMemoryDeviceRepository : IDeviceRepository
{
    private readonly ConcurrentDictionary<Guid, Device> _store = new();

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
