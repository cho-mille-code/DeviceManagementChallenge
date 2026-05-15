using System.Text.Json.Serialization;
using DeviceManagement.Infrastructure;

namespace DeviceManagement.Models;

[JsonConverter(typeof(StrictEnumJsonConverter<Status>))]
public enum Status { Active, Inactive, Retired }
