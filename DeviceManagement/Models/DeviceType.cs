using System.Text.Json.Serialization;
using DeviceManagement.Infrastructure;

namespace DeviceManagement.Models;

[JsonConverter(typeof(StrictEnumJsonConverter<DeviceType>))]
public enum DeviceType { Laptop, Desktop }
