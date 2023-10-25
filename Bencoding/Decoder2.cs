namespace Bencoding;

public class Decoder2
{
    public T Decode<T>(string input) where T : notnull
    {
        var type = typeof(T);
        var instance = (T)Activator.CreateInstance(type)!;
        
        PopulateObject( input.AsSpan() , instance);
        return instance;
    }
    
    private ReadOnlySpan<char> PopulateObject<T>(ReadOnlySpan<char> input, T instance) where T : notnull
    {
        var properties = instance.GetType().GetProperties();

        foreach (var property in properties)
        {
            input = ParseProperty(input, property.PropertyType, out var propertyValue);
            property.SetValue(instance, propertyValue);
        }

        return input;
    }

    private ReadOnlySpan<char> ParseProperty(ReadOnlySpan<char> input, Type type, out object propertyValue)
    {
        return type.Name switch
        {
            "String" => ParseString(input, out propertyValue),
            "Int32" => ParseInt(input, out propertyValue),
            "List`1" => ParseList(input, type, out propertyValue),
            "Dictionary`2" => ParseDictionary(input, type, out propertyValue),
            _ => ParseObject(input, type, out propertyValue)
        };
    }
    
    private ReadOnlySpan<char> ParseString(ReadOnlySpan<char> input, out object propertyValue)
    {
        var i = input.IndexOf(':'); // Error if -1
        var length = int.Parse(input.Slice(0, i)); // Error If not a number
        propertyValue = input.Slice(i+1, length).ToString(); // Error if length > rest
        return input.Slice(i + length + 1);
    }

    private ReadOnlySpan<char> ParseInt(ReadOnlySpan<char> input, out object propertyValue)
    {
        var i = input.IndexOf('e'); // Error if 0,1 or -1
        propertyValue = int.Parse(input.Slice(1, i - 1));
        return input.Slice(i + 1);
    }
    
    private ReadOnlySpan<char> ParseList(ReadOnlySpan<char> input, Type type, out object list)
    {
        list = Activator.CreateInstance(type)!;
        var childType = type.GenericTypeArguments[0];
        input = input.Slice(1);
        while (input[0] != 'e')
        {
            input = ParseProperty(input, childType,out var propertyValue);
            type.GetMethod("Add")!.Invoke(list, new[] { propertyValue });
        }
        return input.Slice(1);
    }

    private ReadOnlySpan<char> ParseDictionary(ReadOnlySpan<char> input, Type type, out object dictionary)
    {
        dictionary = Activator.CreateInstance(type)!;
        var childType = type.GenericTypeArguments[1];
        input = input.Slice(1);
        while (input[0] != 'e')
        {
            input = ParseString(input, out var key);
            input = ParseProperty(input, childType, out var propertyValue);
            type.GetMethod("Add")!.Invoke(dictionary, new[] { key, propertyValue });
        }
        return input.Slice(1);
    }

    private ReadOnlySpan<char> ParseObject(ReadOnlySpan<char> input, Type childType, out object childInstance)
    {
        childInstance = Activator.CreateInstance(childType)!;
        return PopulateObject(input, childInstance);
    }
}