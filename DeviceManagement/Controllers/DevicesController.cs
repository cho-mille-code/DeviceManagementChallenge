using DeviceManagement.Models;
using DeviceManagement.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManagement.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceRepository _repository;
    private readonly IValidator<Device> _validator;
    private readonly IValidator<UpdateDeviceRequest> _updateValidator;

    public DevicesController(
        IDeviceRepository repository,
        IValidator<Device> validator,
        IValidator<UpdateDeviceRequest> updateValidator)
    {
        _repository = repository;
        _validator = validator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{serialNumber:guid}")]
    public async Task<IActionResult> Get(Guid serialNumber)
    {
        var device = await _repository.GetBySerialNumberAsync(serialNumber);
        return device is null ? NotFound() : Ok(device);
    }

    [HttpGet]
    public async Task<IActionResult> GetByPrimaryUser([FromQuery] string? primaryUser)
    {
        if (string.IsNullOrWhiteSpace(primaryUser))
            return BadRequest(new { error = "primaryUser query parameter is required." });

        var devices = await _repository.GetByPrimaryUserAsync(primaryUser);
        return Ok(devices);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Device device)
    {
        var validation = await _validator.ValidateAsync(device);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var created = await _repository.CreateAsync(device);
        return CreatedAtAction(nameof(Get), new { serialNumber = created.SerialNumber }, created);
    }

    [HttpPut("{serialNumber:guid}")]
    public async Task<IActionResult> Update(Guid serialNumber, [FromBody] UpdateDeviceRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var updated = await _repository.UpdateAsync(serialNumber, request);
        return updated is null ? NotFound() : Ok(updated);
    }
}
