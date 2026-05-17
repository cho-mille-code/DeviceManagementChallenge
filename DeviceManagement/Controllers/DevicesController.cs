using DeviceManagement.Models;
using DeviceManagement.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManagement.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController(
    IDeviceRepository repository,
    IValidator<Device> validator,
    IValidator<UpdateDeviceRequest> updateValidator) : ControllerBase
{
    [HttpGet("{serialNumber:guid}")]
    public async Task<IActionResult> Get(Guid serialNumber)
    {
        var device = await repository.GetBySerialNumberAsync(serialNumber);
        return device is null ? NotFound() : Ok(device);
    }

    [HttpGet]
    public async Task<IActionResult> GetByPrimaryUser([FromQuery] string? primaryUser)
    {
        if (string.IsNullOrWhiteSpace(primaryUser))
            return BadRequest(new { error = "primaryUser query parameter is required." });

        var devices = await repository.GetByPrimaryUserAsync(primaryUser);
        return Ok(devices);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Device device)
    {
        var validation = await validator.ValidateAsync(device);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var created = await repository.CreateAsync(device);
        return CreatedAtAction(nameof(Get), new { serialNumber = created.SerialNumber }, created);
    }

    [HttpPut("{serialNumber:guid}")]
    public async Task<IActionResult> Update(Guid serialNumber, [FromBody] UpdateDeviceRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var updated = await repository.UpdateAsync(serialNumber, request);
        return updated is null ? NotFound() : Ok(updated);
    }
}
