# Zalloc - Zero-Allocation String Parsing

## Project Overview

Educational C# project demonstrating **zero-allocation string parsing** using `ReadOnlySpan<char>` and `ref struct`. The goal is to parse key=value pairs **13x faster** with **0 bytes allocated** compared to traditional `string.Split()` approaches.

**Target Framework**: .NET 10.0  
**Key Dependencies**: BenchmarkDotNet 0.14.0

## Architecture

### Core Component: ConfigLineParser (ref struct)

- **Location**: [Zalloc.App/ConfigLineParser.cs](Zalloc.App/ConfigLineParser.cs)
- **Pattern**: Iterator-style parser using `ReadOnlySpan<char>` to avoid heap allocations
- **Critical**: Always use `ref struct` when working with spans to enforce stack-only allocation
- **API**: `TryGetNextEntry(out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)` - returns false when no more pairs

**Example Usage**:

```csharp
var parser = new ConfigLineParser(inputString.AsSpan()); // .AsSpan() is critical!
while (parser.TryGetNextEntry(out var key, out var value))
{
    // key and value are stack-allocated spans - zero heap allocations
    // Convert to string ONLY when storage is needed: key.ToString()
}
```

### Benchmark Suite

- **Location**: [Zalloc.App/ParsingBenchmarks.cs](Zalloc.App/ParsingBenchmarks.cs)
- **Attributes Required**: `[MemoryDiagnoser]` to track allocations, `[Orderer(SummaryOrderPolicy.FastestToSlowest)]` for readability
- **Baseline**: TraditionalParsing using `string.Split()` - the "bad" approach we're improving upon
- **Test Data**: Fixed string `"key1=value1;key2=value2;..."` - keeping it consistent for fair comparison

## Development Workflows

### Running Benchmarks

```bash
# From project root
dotnet run -c Release --project Zalloc.App
```

**Critical**: Always use `-c Release` for accurate benchmark results (Debug mode has optimization disabled)

### Build Commands

```bash
dotnet build Zalloc.App/Zalloc.App.csproj  # Debug build
dotnet build -c Release                     # Release build (for benchmarking)
```

### Benchmark Artifacts

- **Location**: `Zalloc.App/BenchmarkDotNet.Artifacts/results/`
- **Auto-generated**: GitHub markdown, CSV, and HTML reports
- **Report Documentation**: [docs/BENCHMARK_REPORT.md](docs/BENCHMARK_REPORT.md) - comprehensive analysis with explanations

## Project-Specific Conventions

### Zero-Allocation Patterns

1. **Always prefer `ReadOnlySpan<char>` over `string` for parsing**:

   ```csharp
   // ❌ BAD - allocates array
   var parts = input.Split(';');

   // ✅ GOOD - zero allocations
   var parser = new ConfigLineParser(input.AsSpan());
   ```

2. **Delay string materialization** - only call `.ToString()` when absolutely necessary (e.g., storing in Dictionary)

3. **Use `ref struct` for span-based types** - enforces stack-only, prevents heap escape

### Benchmark Naming Convention

- Prefix method with approach name: `TraditionalParsing`, `SpanBasedParsing`, `ZeroAllocationParsing`
- Mark baseline with `[Benchmark(Baseline = true)]`
- Use descriptive comments showing allocations: `// HEAP allocations` vs `// STACK only!`

### Code Comments Style

- Emphasize allocation behavior in comments (see [ParsingBenchmarks.cs](Zalloc.App/ParsingBenchmarks.cs#L17-L25))
- Use `// HEAP` and `// STACK` markers to highlight allocation points
- Include "why it's fastest" explanations in benchmark documentation

## Key Files Reference

- **Core Parser**: [ConfigLineParser.cs](Zalloc.App/ConfigLineParser.cs) - the zero-allocation ref struct
- **Benchmarks**: [ParsingBenchmarks.cs](Zalloc.App/ParsingBenchmarks.cs) - 4 comparison methods
- **Entry Point**: [Program.cs](Zalloc.App/Program.cs) - runs benchmarks and prints summary
- **Performance Report**: [docs/BENCHMARK_REPORT.md](docs/BENCHMARK_REPORT.md) - detailed analysis

## Common Pitfalls

- **Forgetting `.AsSpan()`**: Must convert string to span before passing to ConfigLineParser
- **Running in Debug**: Benchmarks must use Release configuration or results are meaningless
- **Calling `.ToString()` unnecessarily**: Defeats the zero-allocation purpose - only convert when storing
- **Removing `ref struct`**: Required to prevent spans from escaping to heap
