﻿
using System.Runtime;

var test = "5:Davidi48e4:Yorkl3:Eve3:Baz4:Junaeli7ei11ee";
//var test = "5:Davidi48e4:York";

var decoder = new Bencoding.Decoder();
var result = decoder.Decode<SimpleTest>(test);
Console.WriteLine(result.Name);
Console.WriteLine(result.Town);
Console.WriteLine(result.Age);

foreach (var cat in result.Cats)
    Console.WriteLine(cat.Name);

foreach (var pn in result.PrimeNumbers)
    Console.WriteLine(pn);


class SimpleTest
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Town { get; set; } = string.Empty;
    public List<Cat> Cats { get; set; }
    public List<int> PrimeNumbers { get; set; }
}

class Cat
{
    public string Name { get; set; }
}