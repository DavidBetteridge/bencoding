using System.Collections;
using System.Globalization;
using System.Text;

namespace Bencoding;

public class Encoder
{
    public string Encode<T>(T input) where T : notnull
    {
        var sb = new StringBuilder();
        EncodeObject(input, sb);
        return sb.ToString();
    }

    private void EncodeObject<T>(T instance, StringBuilder sb) where T : notnull
    {
        var properties = instance.GetType().GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(instance);
            if (value is not null)
                EncodeProperty(value, property.PropertyType, sb);
        }
    }

    private void EncodeProperty<T>(T input, Type type, StringBuilder sb) where T : notnull
    {
        switch (type.Name)
        {
            case "String":
                EncodeString((input as string)!, sb);
                break;
            case "Int32":
                EncodeInt32((int)(object)input, sb);
                break;
            case "List`1":
                EncodeList(type, input, sb);
                break;
            case "Dictionary`2":
                EncodeDictionary(type, input, sb);
                break;
            default:
                EncodeObject(input, sb);
                break;
        }
    }

    private void EncodeDictionary<T>(Type type, T input, StringBuilder sb) where T : notnull
    {
        var childType = type.GenericTypeArguments[1];
        var asDictionary = input as IDictionary;
        
        sb.Append('d');
        foreach (var key in asDictionary!.Keys)
        {
            EncodeString((string)key, sb);
            EncodeProperty(asDictionary[key]!, childType, sb);
        }
        sb.Append('e');
    }

    private void EncodeList<T>(Type type, T input, StringBuilder sb) where T : notnull
    {
        var childType = type.GenericTypeArguments[0];
        var asList = input as IEnumerable;
        
        sb.Append('l');
        foreach (var listEntry in asList!)
        {
            EncodeProperty(listEntry, childType, sb);
        }
        sb.Append('e');
    }

    private static void EncodeInt32(int input, StringBuilder sb)
    {
        sb.Append('i');
        sb.Append(input.ToString(CultureInfo.InvariantCulture));
        sb.Append('e');
    }

    private static void EncodeString(string input, StringBuilder sb)
    {
        sb.Append(input.Length);
        sb.Append(':');
        sb.Append(input);
    }
}