using DeviceManagement.Models;

namespace DeviceManagement.Repositories;

public interface IDeviceRepository
{
    Task<Device?> GetBySerialNumberAsync(Guid serialNumber);
    Task<Device> CreateAsync(Device device);
    Task<Device?> UpdateAsync(Guid serialNumber, UpdateDeviceRequest request);
}
