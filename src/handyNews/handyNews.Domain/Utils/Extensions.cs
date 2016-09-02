using System;
using System.Collections.Generic;
using Windows.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace handyNews.Domain.Utils
{
    public static class Extensions
    {
        public static T GetValue<T>(this ApplicationDataContainer container, string key, T defaultValue)
        {
            object obj;
            if (container.Values.TryGetValue(key, out obj))
                return (T)obj;

            return defaultValue;
        }

        public static T GetValue<T>(this IDictionary<string, object> dictionary, string key, T defaultValue = default(T))
        {
            object o;
            if (!dictionary.TryGetValue(key, out o))
                return defaultValue;

            return (T)o;
        }

        public static DateTime GetBeginWeekDate(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            var diff = date.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return date.AddDays(-1 * diff).Date;
        }

        public static string ToJson(this object obj)
        {
            return JObject.FromObject(obj).ToString(Formatting.None);
        }

        public static T FromJson<T>(this string jsonString, bool supressErrors = false)
        {
            try
            {
                return JObject.Parse(jsonString).ToObject<T>();
            }
            catch (Exception)
            {
                if (supressErrors)
                    return default(T);

                throw;
            }
        }

        public static string ConvertHtmlToText(this string html)
        {
            if (html == null)
            {
                return null;
            }

            html = html.Replace("&amp;", "&")
                .Replace("&gt;", ">")
                .Replace("&lt;", "<")
                .Replace("\n", string.Empty)
                .Trim();

            return html;
        }

        public static bool StartsWithOrdinalIgnoreCase(this string text, string value)
        {
            return text.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EndsWithOrdinalIgnoreCase(this string text, string value)
        {
            return text.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EqualsOrdinalIgnoreCase(this string text, string value)
        {
            return text.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}