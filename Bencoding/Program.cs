
var test = "5:David4:Yorki48el3:Eve3:Baz4:Junaeli7ei11eed5:David3:Red4:Paul4:Bluee";
//var test = "5:Davidi48e4:York";

var decoder = new Bencoding.Decoder2();
var result = decoder.Decode<SimpleTest>(test);
Console.WriteLine(result.Name);
Console.WriteLine(result.Town);
Console.WriteLine(result.Age);

foreach (var cat in result.Cats)
    Console.WriteLine(cat.Name);

foreach (var pn in result.PrimeNumbers)
    Console.WriteLine(pn);

foreach (var colour in result.Colours)
    Console.WriteLine($"{colour.Key} is {colour.Value}");

class SimpleTest
{
    public string Name { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public int Age { get; set; }
   
    public List<Cat> Cats { get; set; }
    public List<int> PrimeNumbers { get; set; }
    
    public Dictionary<string, string> Colours { get; set; }
}

class Cat
{
    public string Name { get; set; }
}