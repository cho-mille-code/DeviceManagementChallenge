namespace DeviceManagement.Models;

public record UpdateDeviceRequest
{
    public string? PrimaryUser { get; init; }
    public string? OperatingSystem { get; init; }
    public DeviceType? DeviceType { get; init; }
    public Status? Status { get; init; }
}
