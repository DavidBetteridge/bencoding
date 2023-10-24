using System.Reflection;

namespace Bencoding;

public class Decoder
{
    public T Decode<T>(string input) where T : notnull
    {
        var type = typeof(T);
        var instance = (T)Activator.CreateInstance(type)!;
        
        return PopulateObject(new Input(input), instance);
    }

    private class Input
    {
        private readonly string _value;
        private int _head = 0;

        public Input(string input)
        {
            _value = input;
        }

        public int IndexOf(char c)
        {
            return _value.IndexOf(c, _head)-_head;
        }

        public string Slice(int start, int length)
        {
            var r = _value[(_head + start)..(_head+start+length)];
            _head += start + length;
            return r;
        }

        public void Eat(char c)
        {
            if (_value[_head] != c)
                throw new Exception($"Expected {c} but found {_value[_head]}");
            _head += 1;
        }

        public char Peek()
        {
            return _value[_head];
        }
    }
    
    private T PopulateObject<T>(Input input, T instance) where T : notnull
    {
        var properties = instance.GetType().GetProperties();

        foreach (var property in properties)
        {
            var propertyValue = ParseProperty(input, property);
            property.SetValue(instance, propertyValue);
        }

        return instance;
    }

    private object ParseProperty(Input input, PropertyInfo property)
    {
        return property.PropertyType.Name switch
        {
            "String" => ParseString(input),
            "Int32" => ParseInt(input),
            "List`1" => ParseList(input, property),
            "Dictionary`2" => ParseDictionary(input, property),
            _ => ParseObject(input, property.PropertyType)
        };
    }
    
    private object ParseType(Input input, PropertyInfo property, Type type)
    {
        return type.Name switch
        {
            "String" => ParseString(input),
            "Int32" => ParseInt(input),
            "List`1" => ParseList(input, property),
            "Dictionary`2" => ParseDictionary(input, property),
            _ => ParseObject(input, type)
        };
    }

    private object ParseList(Input input, PropertyInfo property)
    {
        var list = Activator.CreateInstance(property.PropertyType);
        var childType = property.PropertyType.GenericTypeArguments[0];
        input.Eat('l');
        while (input.Peek() != 'e')
        {
            var propertyValue = ParseType(input, property, childType);
            property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { propertyValue });
        }
        input.Eat('e');
        return list!;
    }

    private object ParseDictionary(Input input, PropertyInfo property)
    {
        var dictionary = Activator.CreateInstance(property.PropertyType);
        var childType = property.PropertyType.GenericTypeArguments[1];
        input.Eat('d');
        while (input.Peek() != 'e')
        {
            var key = ParseString(input);
            var propertyValue = ParseType(input, property, childType);
            property.PropertyType.GetMethod("Add")!.Invoke(dictionary, new[] { key, propertyValue });
        }
        input.Eat('e');
        return dictionary!;
    }

    private object ParseObject(Input input, Type childType)
    {
        var childInstance = Activator.CreateInstance(childType)!;
        PopulateObject(input, childInstance);
        return childInstance;
    }

    private string ParseString(Input input)
    {
        var i = input.IndexOf(':'); // Error if -1
        var length = int.Parse(input.Slice(0, i)); // Error If not a number
        var value = input.Slice(i, length).ToString(); // Error if length > rest
        return value;
    }

    private int ParseInt(Input input)
    {
        var i = input.IndexOf('e'); // Error if 0,1 or -1
        var value = int.Parse(input.Slice(1, i - 1).ToString());
        input.Eat('e');
        return value;
    }
}