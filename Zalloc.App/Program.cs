using BenchmarkDotNet.Running;
using Zalloc.App;

// Run the benchmarks
var summary = BenchmarkRunner.Run<ParsingBenchmarks>();

Console.WriteLine("\nBenchmark completed! Check the results above.");
Console.WriteLine("""Key takeaways:""");
Console.WriteLine(
    """- TraditionalParsing: Multiple string.Split() calls create many heap allocations"""
);
Console.WriteLine("""- SpanBasedParsing: Uses ReadOnlySpan for parsing, minimal allocations""");
Console.WriteLine("""- ZeroAllocationParsing: Completely zero allocation - spans only""");
Console.WriteLine("""- SpanWithDictionary: Zero allocation parsing + controlled storage""");
