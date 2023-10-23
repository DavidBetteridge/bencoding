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
            ParseProperty(input, instance, property);
        }

        return instance;
    }

    private void ParseProperty<T>(Input input, T instance, PropertyInfo property) where T : notnull
    {
        if (property.PropertyType == typeof(string))
        {
            var propertyValue = ParseString(input);
            property.SetValue(instance, propertyValue);
        }

        else if (property.PropertyType == typeof(int))
        {
            var propertyValue = ParseInt(input);
            property.SetValue(instance, propertyValue);
        }

        else if (property.PropertyType.Name == "Dictionary`2")
        {
            var list = ParseDictionary<T>(input, property);
            property.SetValue(instance, list);
        }

        else if (property.PropertyType.Name == "List`1")
        {
            var list = ParseList<T>(input, property);
            property.SetValue(instance, list);
        }

        else
        {
            var childInstance = Activator.CreateInstance(property.PropertyType)!;
            PopulateObject(input, childInstance);
            property.SetValue(instance, childInstance);
        }
    }

    private object ParseList<T>(Input input, PropertyInfo property) where T : notnull
    {
        // Create the list
        var list = Activator.CreateInstance(property.PropertyType);
        var childType = property.PropertyType.GenericTypeArguments[0];

        input.Eat('l');

        while (input.Peek() != 'e')
        {
            if (childType == typeof(string))
            {
                var propertyValue = ParseString(input);
                property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { propertyValue });
            }
            else if (childType == typeof(int))
            {
                var propertyValue = ParseInt(input);
                property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { (object)propertyValue });
            }
            else if (childType.Name == "List`1")
            {
                var propertyValue = ParseList<T>(input, property);
                property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { propertyValue });
            }
            else if (childType.Name == "Dictionary`2")
            {
                var propertyValue = ParseDictionary<T>(input, property);
                property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { propertyValue });
            }
            else
            {
                var childInstance = Activator.CreateInstance(childType)!;
                PopulateObject(input, childInstance);
                property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { childInstance });
            }
        }

        input.Eat('e');
        return list!;
    }

    private object ParseDictionary<T>(Input input, PropertyInfo property) where T : notnull
    {
        // Create the dictionary
        var list = Activator.CreateInstance(property.PropertyType);
        var childType = property.PropertyType.GenericTypeArguments[1];
        input.Eat('d');
        while (input.Peek() != 'e')
        {
            var key = ParseString(input);

            switch (childType.Name)
            {
                case "String":
                {
                    var propertyValue = ParseString(input);
                    property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { key, propertyValue });
                    break;
                }
                case "Int32":
                {
                    var propertyValue = ParseInt(input);
                    property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { key, (object)propertyValue });
                    break;
                }
                case "List`1":
                {
                    var propertyValue = ParseList<T>(input, property);
                    property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { key, propertyValue });
                    break;
                }
                case "Dictionary`2":
                {
                    var propertyValue = ParseDictionary<T>(input, property);
                    property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { key, propertyValue });
                    break;
                }
                default:
                {
                    var childInstance = Activator.CreateInstance(childType)!;
                    PopulateObject(input, childInstance);
                    property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { key, childInstance });
                    break;
                }
            }
        }

        input.Eat('e');
        return list;
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