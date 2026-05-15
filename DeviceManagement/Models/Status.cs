using System.Text.Json.Serialization;

namespace DeviceManagement.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status { Active, Inactive, Retired }
