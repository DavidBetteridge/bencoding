using System.Reflection;

namespace Bencoding;

public class Decoder
{
    public T Decode<T>(string input)
    {
        return InternalDecode<T>(new Input(input));
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
    
    private T InternalDecode<T>(Input input)
    {
        var type = typeof(T);
        var instance = Activator.CreateInstance(type)!;
        var properties = type.GetProperties();

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

            if (property.PropertyType.Name == "List`1")
            {
                // Create the list
                var list = Activator.CreateInstance(property.PropertyType);
                var decoder = this.GetType()
                    .GetMethod("InternalDecode", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(property.PropertyType.GenericTypeArguments[0]);

                input.Eat('l');

                while (input.Peek() != 'e')
                {
                    var parsedValue = decoder.Invoke(this, new[] { input });
                    property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { parsedValue });
                }
                
                property.SetValue(instance, list);
            }
//                     // Invoke the decoder
//              //       var r = decoder.Invoke(this, rest) as InternalResult<object>!;
//                     
//                     //  var 
//                     //var r = decoder.Invoke(this, new[] { rest }) as InternalResult<char>!;
//
// // e                var item = Activator.CreateInstance(property.PropertyType.GenericTypeArguments[0]);
// //                 rest = Decode<>(rest, out var propertyValue);
// //                 property.SetValue(item, propertyValue);
// //                 property.PropertyType.GetMethod("Add")!.Invoke(list, new[] { item });
//              //   }
//
//                 property.SetValue(instance, list);
//             }
        }

        return (T)instance;
        //return new InternalResult<T>(){Rest = rest, Instance = (T) instance};
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