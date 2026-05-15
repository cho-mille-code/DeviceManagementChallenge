using DeviceManagement.Data;
using DeviceManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceManagement.Repositories;

public class SqlDeviceRepository : IDeviceRepository
{
    private readonly DeviceDbContext _context;

    public SqlDeviceRepository(DeviceDbContext context)
    {
        _context = context;
    }

    public async Task<Device?> GetBySerialNumberAsync(Guid serialNumber)
        => await _context.Devices.FindAsync(serialNumber);

    public async Task<IEnumerable<Device>> GetByPrimaryUserAsync(string primaryUser)
        => await _context.Devices
            .Where(d => d.PrimaryUser == primaryUser)
            .ToListAsync();

    public async Task<Device> CreateAsync(Device device)
    {
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
        return device;
    }

    public async Task<Device?> UpdateAsync(Guid serialNumber, UpdateDeviceRequest request)
    {
        // AsNoTracking so we can freely attach the new record produced by `with`
        var existing = await _context.Devices
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

        _context.Devices.Update(updated);
        await _context.SaveChangesAsync();
        return updated;
    }
}
