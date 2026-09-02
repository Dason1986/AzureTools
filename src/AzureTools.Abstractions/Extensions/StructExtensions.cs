using System;
using System.Collections.Generic;

namespace AzureTools.Extensions
{
    public static class StructExtensions
    {
        private static readonly HashSet<string> TrueValues = new(StringComparer.OrdinalIgnoreCase) { "Y", "Yes", "OK", "T", "True" };

        private static readonly HashSet<string> FalseValues = new(StringComparer.OrdinalIgnoreCase) { "N", "No", "F", "False" };
        public static bool IsTrue(string str)
        {
            return TrueValues.Contains(str);
        }
        public static bool IsFalse(string str)
        {
            return FalseValues.Contains(str);
        }
        public static bool TryGetValue<T>(this IDictionary<string, T> dic, string[] keys, out T t)
        {
            if (dic == null || dic.Count == 0)
            {
                t = default;
                return false;
            }

            foreach (string item in keys)
            {
                bool flag = dic.TryGetValue(item, out t);
                if (flag) return flag;
            }
            t = default;
            return false;
        }
        public static DateTime? ToLocalDateTime(this DateTimeOffset? dateTimeOffset)
        {
            if (dateTimeOffset == null) return null;
            DateTime time = dateTimeOffset.Value.ToLocalTime().DateTime;
            if (time == System.DateTime.MinValue || time == System.DateTime.MaxValue) return null;
            return time;
        }
        public static DateTime ToLocalDateTime(this DateTimeOffset dateTimeOffset)
        {

            return dateTimeOffset.ToLocalTime().DateTime;

        }
        public static T StringCase<T>(string str)
        {
            return TryStringCase<T>(str, out T t) ? t : default;
        }
        public static bool TryStringCase<T>(string str, out T result)
        {
            Type targetType = typeof(T);
            object obj = null;
            if (targetType == typeof(bool) || targetType == typeof(bool?)) obj = IsTrue(str);
            if ((targetType == typeof(DateTime) || targetType == typeof(DateTime?)) && DateTime.TryParse(str, out DateTime resultDateTime)) obj = resultDateTime;
            if ((targetType == typeof(TimeSpan) || targetType == typeof(TimeSpan?)) && TimeSpan.TryParse(str, out TimeSpan resultTimeSpan)) obj = resultTimeSpan;
            if ((targetType == typeof(int) || targetType == typeof(int?)) && int.TryParse(str, out int resultInt)) obj = resultInt;
            if ((targetType == typeof(double) || targetType == typeof(double?)) && DateTime.TryParse(str, out DateTime resultDouble)) obj = resultDouble;
            if ((targetType == typeof(decimal) || targetType == typeof(decimal?)) && DateTime.TryParse(str, out DateTime resultDecimal)) obj = resultDecimal;
            if ((targetType == typeof(long) || targetType == typeof(long?)) && long.TryParse(str, out long resultLong)) obj = resultLong;
            if (obj is T t)
            {
                result = t;
                return true;
            }
            result = default;
            return false;
        }
        public static T Case<T>(object obj)
        {
            return TryCase<T>(obj, out T t) ? t : default;
        }
        public static bool TryCase<T>(object obj, out T result)
        {

            if (obj == null || System.Convert.IsDBNull(obj))
            {
                result = default;
                return false;
            }
            if (obj is T t)
            {
                result = t;
                return true;
            }
            if (obj is string str && TryStringCase<T>(str, out result)) return true;
            try
            {
                Type targetType = typeof(T);
                if (targetType.IsGenericType) targetType = targetType.GetGenericArguments()[0];
                object tobj = System.Convert.ChangeType(obj, targetType);
                result = (T)tobj;
                return true;
            }
            catch (Exception)
            {


            }
            result = default;
            return false;
        }

        public static object ToDisylay(double size, int decimalPlaces = 2)
        {
            if (size <= 0) return string.Empty;
            string[] units = { "B", "KB", "MB", "GB", "TB" };

            int unitIndex = 0;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{size.ToString($"F{decimalPlaces}")} {units[unitIndex]}";
        }

    }
}
