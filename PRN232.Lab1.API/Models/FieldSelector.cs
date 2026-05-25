using System.Dynamic;
using System.Reflection;
using System.Text.Json;

namespace PRN232.Lab1.API.Models
{
    public static class FieldSelector
    {
        public static object Apply<T>(IEnumerable<T> items, string? fields)
        {
            var selectedFields = ParseFields(fields);
            if (selectedFields.Count == 0)
            {
                return items;
            }

            return items.Select(item => Shape(item!, selectedFields)).ToList();
        }

        public static object Apply<T>(T item, string? fields)
        {
            var selectedFields = ParseFields(fields);
            return selectedFields.Count == 0 ? item! : Shape(item!, selectedFields);
        }

        private static IReadOnlySet<string> ParseFields(string? fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static ExpandoObject Shape(object item, IReadOnlySet<string> fields)
        {
            IDictionary<string, object?> shaped = new ExpandoObject();
            var properties = item.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in properties)
            {
                var jsonName = JsonNamingPolicy.CamelCase.ConvertName(property.Name);
                if (fields.Contains(property.Name) || fields.Contains(jsonName))
                {
                    var value = property.GetValue(item);
                    if (value != null)
                    {
                        shaped[jsonName] = value;
                    }
                }
            }

            return (ExpandoObject)shaped;
        }
    }
}
