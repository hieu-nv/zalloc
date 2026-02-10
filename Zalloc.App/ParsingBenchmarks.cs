using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Zalloc.App;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ParsingBenchmarks
{
    private string _testData = "key1=value1;key2=value2;key3=value3;key4=value4;key5=value5";

    [Benchmark(Baseline = true)]
    public void TraditionalParsing()
    {
        var dict = new Dictionary<string, string>();
        foreach (var pair in _testData.Split(';')) // HEAP allocations
        {
            var parts = pair.Split('='); // More HEAP
            if (parts.Length == 2)
                dict[parts[0]] = parts[1];
        }
    }

    [Benchmark]
    public void SpanBasedParsing()
    {
        var parser = new ConfigLineParser(_testData.AsSpan()); // STACK only!
        while (parser.TryGetNextEntry(out var key, out var value))
        {
            // Process without allocations
            // Convert to string ONLY if needed
            if (NeedToStore(key))
            {
                string keyStr = key.ToString(); // Controlled HEAP (only when necessary)
            }
        }
    }

    [Benchmark]
    public void ZeroAllocationParsing()
    {
        // Completely zero allocation - just iterate without storing
        var parser = new ConfigLineParser(_testData.AsSpan());
        int count = 0;
        while (parser.TryGetNextEntry(out var key, out var value))
        {
            // Just process the spans without converting to strings
            // Real use case: validation, counting, searching, etc.
            count++;
        }
    }

    [Benchmark]
    public void SpanWithDictionary()
    {
        // Zero allocation during parsing, but controlled allocation for storage
        var dict = new Dictionary<string, string>(5); // Pre-sized to avoid resizing
        var parser = new ConfigLineParser(_testData.AsSpan());

        while (parser.TryGetNextEntry(out var key, out var value))
        {
            // Allocate strings only when storing in dictionary
            dict[key.ToString()] = value.ToString();
        }
    }

    private bool NeedToStore(ReadOnlySpan<char> span) => span.Length > 0;
}
