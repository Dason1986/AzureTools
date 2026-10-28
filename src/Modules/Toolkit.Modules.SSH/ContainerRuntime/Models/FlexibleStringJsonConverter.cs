using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models
{
    /// <summary>
    /// 将 JSON 中的字符串、数字、布尔或数组（取第一个可转换元素）统一读取为字符串，兼容 Docker 与 Podman CLI 对同名字段的输出差异。
    /// </summary>
    public sealed class FlexibleStringJsonConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray) return ReadValue(ref reader);

            string value = null;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (value == null) value = ReadValue(ref reader);
            }
            return value == null ? string.Empty : value;

        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            => writer.WriteStringValue(value);

        private static string ReadValue(ref Utf8JsonReader reader)
        {
            return reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => ReadNumberText(ref reader),
                JsonTokenType.True => bool.TrueString,
                JsonTokenType.False => bool.FalseString,
                _ => null
            };
        }

        private static string ReadNumberText(ref Utf8JsonReader reader)
            => reader.TryGetInt64(out long number)
                ? number.ToString(CultureInfo.InvariantCulture)
                : reader.GetDouble().ToString(CultureInfo.InvariantCulture);
    }
}
