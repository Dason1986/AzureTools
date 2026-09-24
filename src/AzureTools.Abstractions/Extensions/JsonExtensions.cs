using System;
using System.Linq;
using System.Text.Json;

namespace AzureTools.Extensions
{
    public static class JsonExtensions
    {
        public static string GetString(this JsonElement element, string key)
        {
            if (element.ValueKind == JsonValueKind.Undefined) return string.Empty;
            if (element.TryGetProperty(key, out JsonElement item) && item.ValueKind != JsonValueKind.Undefined) return item.GetString();
            return string.Empty;
        }
        public static string[] GetStrings(this JsonElement element, string key)
        {
            if (element.ValueKind == JsonValueKind.Undefined) return Array.Empty<string>();
            if (element.TryGetProperty(key, out JsonElement tagsElements))
                return tagsElements.EnumerateArray().Select(n => n.GetString()).ToArray();
            return Array.Empty<string>();
        }
        public static bool GetBoolean(this JsonElement element, string key)
        {
            if (element.ValueKind == JsonValueKind.Undefined) return false;
            if (element.TryGetProperty(key, out JsonElement item) && item.ValueKind != JsonValueKind.Undefined)
            {
                if (item.ValueKind == JsonValueKind.True) return true;
                if (item.ValueKind == JsonValueKind.False) return false;
                if (item.ValueKind == JsonValueKind.String)
                {
                    string str = item.GetString();
                    if (bool.TryParse(str, out bool falg)) return falg;
                    return StructExtensions.IsTrue(str);
                }
            }
            return false;
        }

        public static double? GetDouble(this JsonElement element, string key)
        {
            if (element.ValueKind == JsonValueKind.Undefined) return null;
            if (element.TryGetProperty(key, out JsonElement item) && item.ValueKind != JsonValueKind.Undefined)
            {
                if (item.ValueKind == JsonValueKind.Number) return item.GetDouble();
                if (item.ValueKind == JsonValueKind.String)
                {
                    string str = item.GetString();
                    if (Double.TryParse(str, out double d)) return d;
                }
            }
            return null;
        }
    }
}
