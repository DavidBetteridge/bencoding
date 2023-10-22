
using System.Runtime;

var test = "5:Davidi48e4:Yorkl3:Eve:3:Baz:4Junae";
//var test = "5:Davidi48e4:York";

var decoder = new Bencoding.Decoder();
var result = decoder.Decode<SimpleTest>(test);
Console.WriteLine(result.Name);
Console.WriteLine(result.Town);
Console.WriteLine(result.Age);

foreach (var cat in result.Cats)
{
    Console.WriteLine(cat);
}



class SimpleTest
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Town { get; set; } = string.Empty;

    public List<string> Cats { get; set; }
}
