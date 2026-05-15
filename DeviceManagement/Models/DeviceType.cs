using System.Text.Json.Serialization;

namespace DeviceManagement.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeviceType { Laptop, Desktop }
