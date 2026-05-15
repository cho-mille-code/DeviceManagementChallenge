using System.ComponentModel.DataAnnotations;

namespace DeviceManagement.Models;

public record Device
{
    [Required]
    public Guid SerialNumber { get; init; }

    [Required]
    public string ModelId { get; init; } = string.Empty;

    [Required]
    public string ModelName { get; init; } = string.Empty;

    [Required]
    public string Manufacturer { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string PrimaryUser { get; init; } = string.Empty;

    [Required]
    public string OperatingSystem { get; init; } = string.Empty;

    [Required]
    public DeviceType DeviceType { get; init; }

    [Required]
    public Status Status { get; init; }
}
