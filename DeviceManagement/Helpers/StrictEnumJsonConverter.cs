using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeviceManagement.Helpers;

public class StrictEnumJsonConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (value is not null && Enum.TryParse<TEnum>(value, ignoreCase: false, out var result) && Enum.IsDefined(result))
            return result;

        var accepted = string.Join(", ", Enum.GetNames<TEnum>());
        throw new JsonException($"'{value}' is not a valid {typeof(TEnum).Name}. Accepted values are: {accepted}.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
