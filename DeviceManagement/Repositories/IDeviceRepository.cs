using DeviceManagement.Models;

namespace DeviceManagement.Repositories;

public interface IDeviceRepository
{
    Task<Device?> GetBySerialNumberAsync(Guid serialNumber);
    Task<IEnumerable<Device>> GetByPrimaryUserAsync(string primaryUser);
    Task<Device> CreateAsync(Device device);
    Task<Device?> UpdateAsync(Guid serialNumber, UpdateDeviceRequest request);
}
