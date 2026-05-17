using DeviceManagement.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DeviceManagement.Tests;

public class DeviceApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Swap out the SQL repository for an in-memory one — no database needed
            services.RemoveAll<IDeviceRepository>();
            services.AddSingleton<IDeviceRepository, InMemoryDeviceRepository>();
        });
    }
}
