// using System.Collections;
using System.Diagnostics;
using Spectre.Console;

namespace SpectreHelpers;

public static class IEnumerableExtensions
{

    public static Table ToTable<T>(this IEnumerable<T> values)
    {

        var header = GetHeaderColumn(typeof(T));
        var rows = GetDataRows(values);

        var table = new Table().AddColumns(header);

        foreach (var row in rows)
        {
            Debug.WriteLine(string.Join(", ", row));
            table.AddRow(row.ToArray());
        }

        return table;
    }

    private static IEnumerable<IEnumerable<string>> GetDataRows<T>(IEnumerable<T> models)
    {
        List<IEnumerable<string>> result = [];
        var tProps = typeof(T).GetProperties();
        foreach (var model in models)
        {
            var strings = GetStrings(model);
            result.Add(GetStrings(model));
        }
        var malformed = result.Where(stringList => stringList.Any(s => s.Contains("Char")));

        return result;
    }

    private static IEnumerable<string> GetStrings<T>(T item)
    {
        var value = "";
        List<string> strings = [];
        if (item?.GetType() is not Type type)
            return strings;

        foreach (var prop in type.GetProperties())
        {
            if (prop.PropertyType.GetInterfaces().Contains(typeof(IEnumerable<>)))
            {
                value = (prop.GetValue(item) is IEnumerable<T> itemValue) ? GetStringFromEnumerable(itemValue) : "";
            }
            else
            {
                value = prop.GetValue(item)?.ToString() ?? "";
            }
            strings.Add(value);
        }
        return strings;
    }

    private static string GetStringFromEnumerable<T>(IEnumerable<T> enumerable)
    {
        var output = "{ ";
        output += CastEnumerable(enumerable);

        return output += " }";

        string CastEnumerable(IEnumerable<T> enumerable) => enumerable switch
        {
            IEnumerable<Char> c => string.Join(", ", c),
            _ => "",
        };

    }

    private static TableColumn[] GetHeaderColumn(Type type)
    {
        var props = type.GetProperties();

        TableColumn[] cols = new TableColumn[props.Length];

        for (int i = 0; i < props.Length; i++)
        {
            cols[i] = new TableColumn(props[i].Name);
        }
        return cols;
    }
}
