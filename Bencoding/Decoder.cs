using System.Reflection;

namespace Bencoding;

public class Decoder
{
    public T Decode<T>(string input)
    {
        var type = typeof(T);
        var instance = (T)Activator.CreateInstance(type)!;
        
        return InternalDecode<T>(new Input(input), instance);
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
    
    private T InternalDecode<T>(Input input, T instance) where T : notnull
    {
        var properties =  instance.GetType().GetProperties();

        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(string))
            {
                var propertyValue = ParseString(input);
                property.SetValue(instance, propertyValue);
            }

            if (property.PropertyType == typeof(int))
            {
                var propertyValue = ParseInt(input);
                property.SetValue(instance, propertyValue);
            }

            if (property.PropertyType.Name == "Dictionary`2")
            {
                // Create the list
                var list = Activator.CreateInstance(property.PropertyType);
                var childType = property.PropertyType.GenericTypeArguments[1];
                input.Eat('d');
                while (input.Peek() != 'e')
                {
                    var key = ParseString(input);
                    
                    if (childType == typeof(string))
                    {
                        var propertyValue = ParseString(input);
                        property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { key, propertyValue });
                    }
                    else if (childType == typeof(int))
                    {
                        var propertyValue = ParseInt(input);
                        property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { key, (object)propertyValue });
                    }
                    else
                    {
                        var childInstance = Activator.CreateInstance(childType)!;
                        InternalDecode(input, childInstance);
                        property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { key, childInstance });                        
                    }

                }
                input.Eat('e');
                property.SetValue(instance, list);
            }

            if (property.PropertyType.Name == "List`1")
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
                    else
                    {
                        var childInstance = Activator.CreateInstance(childType)!;
                        InternalDecode(input, childInstance);
                        property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { childInstance });                        
                    }

                }
                input.Eat('e');

                property.SetValue(instance, list);
            }
        }

        return (T)instance;
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