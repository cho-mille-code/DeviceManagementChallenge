using DeviceManagement.Data;
using DeviceManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceManagement.Repositories;

public class SqlDeviceRepository(DeviceDbContext context) : IDeviceRepository
{
    public async Task<Device?> GetBySerialNumberAsync(Guid serialNumber)
        => await context.Devices.FindAsync(serialNumber);

    public async Task<IReadOnlyList<Device>> GetByPrimaryUserAsync(string primaryUser)
        => await context.Devices
            .Where(d => d.PrimaryUser == primaryUser)
            .ToListAsync();

    public async Task<Device> CreateAsync(Device device)
    {
        context.Devices.Add(device);
        await context.SaveChangesAsync();
        return device;
    }

    public async Task<Device?> UpdateAsync(Guid serialNumber, UpdateDeviceRequest request)
    {
        // AsNoTracking so we can freely attach the new record produced by `with`
        var existing = await context.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.SerialNumber == serialNumber);

        if (existing is null) return null;

        var updated = existing with
        {
            PrimaryUser = request.PrimaryUser ?? existing.PrimaryUser,
            OperatingSystem = request.OperatingSystem ?? existing.OperatingSystem,
            DeviceType = request.DeviceType ?? existing.DeviceType,
            Status = request.Status ?? existing.Status
        };

        context.Devices.Update(updated);
        await context.SaveChangesAsync();
        return updated;
    }
}
