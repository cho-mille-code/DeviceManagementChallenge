namespace DeviceManagement.Models;

public record Device
{
    public required Guid SerialNumber { get; init; }
    public required string ModelId { get; init; }
    public required string ModelName { get; init; }
    public required string Manufacturer { get; init; }
    public required string PrimaryUser { get; init; }
    public required string OperatingSystem { get; init; }
    public required DeviceType DeviceType { get; init; }
    public required Status Status { get; init; }
}
