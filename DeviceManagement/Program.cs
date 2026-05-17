using Azure.Identity;
using DeviceManagement.Data;
using DeviceManagement.Models;
using DeviceManagement.Repositories;
using DeviceManagement.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            // Errors keyed with "$." are specific deserialization failures (e.g. invalid enum value).
            // Prefer those over the generic top-level "field is required" error that appears
            // when the whole body fails to bind.
            var specific = context.ModelState
                .Where(e => e.Key.StartsWith("$.") && e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key[2..], // strip "$." prefix -> "deviceType" instead of "$.deviceType"
                    e => e.Value!.Errors
                        .Select(x =>
                        {
                            // System.Text.Json appends " Path: ... | LineNumber: ..." — trim it
                            var msg = x.ErrorMessage;
                            var cut = msg.IndexOf(" Path: ");
                            return cut > 0 ? msg[..cut] + "." : msg;
                        })
                        .ToArray()
                );

            var errors = specific.Count > 0
                ? specific
                : context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

            return new BadRequestObjectResult(new { status = 400, errors });
        };
    });

builder.Services.AddDbContext<DeviceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDeviceRepository, SqlDeviceRepository>();
builder.Services.AddSingleton<IValidator<Device>, DeviceValidator>();
builder.Services.AddSingleton<IValidator<UpdateDeviceRequest>, UpdateDeviceRequestValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }
