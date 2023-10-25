using BenchmarkDotNet.Attributes;

namespace Bencoding;

[MemoryDiagnoser(true)]
public class BenchmarkTests
{
    [Benchmark]
    public void StringVersion()
    {
        var test = "5:David4:Yorki48el3:Eve3:Baz4:Junaeli7ei11eed5:David3:Red4:Paul4:Bluee";
        var decoder = new Bencoding.Decoder();
        decoder.Decode<SimpleTest>(test);
    }
    
    [Benchmark]
    public void SpanVersion()
    {
        var test = "5:David4:Yorki48el3:Eve3:Baz4:Junaeli7ei11eed5:David3:Red4:Paul4:Bluee";
        var decoder = new Bencoding.Decoder2();
        decoder.Decode<SimpleTest>(test);
    }
}

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