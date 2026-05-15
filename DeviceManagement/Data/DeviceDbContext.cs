using DeviceManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceManagement.Data;

public class DeviceDbContext : DbContext
{
    public DeviceDbContext(DbContextOptions<DeviceDbContext> options) : base(options) { }

    public DbSet<Device> Devices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(d => d.SerialNumber);
            entity.Property(d => d.SerialNumber).ValueGeneratedNever();
            entity.Property(d => d.ModelId).IsUnicode(false);
            entity.Property(d => d.ModelName).IsUnicode(false);
            entity.Property(d => d.Manufacturer).IsUnicode(false);
            entity.Property(d => d.PrimaryUser).IsUnicode(false);
            entity.Property(d => d.OperatingSystem).IsUnicode(false);
            entity.Property(d => d.DeviceType).HasConversion<string>().IsUnicode(false);
            entity.Property(d => d.Status).HasConversion<string>().IsUnicode(false);
        });
    }
}
