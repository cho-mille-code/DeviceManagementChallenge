using Azure.Identity;
using DeviceManagement.Validations;
using DeviceManagement.Models;
using DeviceManagement.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// In production, secrets (e.g. the connection string) are stored in Azure Key Vault.
// The vault name is the only non-secret config needed — safe to put in an env variable.
// DefaultAzureCredential picks up the app's Managed Identity automatically; no credentials in code.
if (!builder.Environment.IsDevelopment())
{
    var kvName = builder.Configuration["Azure:KeyVaultName"];
    if (!string.IsNullOrEmpty(kvName))
        builder.Configuration.AddAzureKeyVault(
            new Uri($"https://{kvName}.vault.azure.net/"),
            new DefaultAzureCredential());
}

builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
        options.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Build);

builder.Services.AddDbContext<DeviceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDeviceRepository, SqlDeviceRepository>();
builder.Services.AddSingleton<IValidator<Device>, DeviceValidator>();
builder.Services.AddSingleton<IValidator<UpdateDeviceRequest>, UpdateDeviceRequestValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }
