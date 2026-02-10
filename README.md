# 🚀 Zalloc - Zero-Allocation String Parsing in C#

> Educational project demonstrating **13x faster** string parsing with **zero heap allocations** using `ReadOnlySpan<char>` and `ref struct`

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## 📊 Performance at a Glance

Parsing `"key1=value1;key2=value2;key3=value3;key4=value4;key5=value5"`:

| Method                       | Mean         | Allocated | Speed            | Memory Saved |
| ---------------------------- | ------------ | --------- | ---------------- | ------------ |
| **ZeroAllocationParsing** ⭐ | **21.23 ns** | **0 B**   | **13.0x faster** | **100%**     |
| SpanBasedParsing             | 47.79 ns     | 160 B     | 5.8x faster      | 88%          |
| SpanWithDictionary           | 141.80 ns    | 688 B     | 2.0x faster      | 48%          |
| TraditionalParsing           | 277.28 ns    | 1,328 B   | Baseline         | -            |

**TL;DR**: By using `ReadOnlySpan<char>`, we parse strings **13x faster** while allocating **zero bytes** on the heap! 🎯

## 🎯 What is Zero-Allocation Parsing?

Traditional string parsing in C# creates multiple heap allocations:

```csharp
// ❌ BAD - Creates array objects on the heap
var pairs = input.Split(';');        // Allocates string[] (HEAP)
var parts = pair.Split('=');         // Allocates more string[] (HEAP)
```

Zero-allocation parsing uses **stack-only** memory:

```csharp
// ✅ GOOD - All operations on the stack
var parser = new ConfigLineParser(input.AsSpan());  // ReadOnlySpan (STACK)
while (parser.TryGetNextEntry(out var key, out var value))  // STACK only!
{
    // key and value are spans pointing to the original string - no copies!
}
```

**Why It Matters:**

- 🔥 **Faster**: No garbage collection overhead
- 💾 **Memory-efficient**: Perfect for high-throughput scenarios
- ⚡ **CPU cache-friendly**: Data stays in cache longer
- 🎓 **Educational**: Learn modern C# performance patterns

## 🚀 Quick Start

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/) or later

### Run the Benchmarks

```bash
# Clone the repository
git clone https://github.com/yourusername/Zalloc.git
cd Zalloc

# Run benchmarks (MUST use Release configuration!)
dotnet run -c Release --project Zalloc.App
```

**⚠️ Critical**: Always use `-c Release` for accurate results. Debug mode disables optimizations!

## 💡 Usage Examples

### Basic Parsing (Zero Allocations)

```csharp
using Zalloc.App;

string config = "key1=value1;key2=value2;key3=value3";

// Create parser - note the .AsSpan()!
var parser = new ConfigLineParser(config.AsSpan());

// Iterate without allocations
while (parser.TryGetNextEntry(out var key, out var value))
{
    Console.WriteLine($"{key.ToString()} = {value.ToString()}");
    // .ToString() here only for demonstration - try to avoid it!
}
```

### Validating Without Allocation

```csharp
// Count entries without creating strings
var parser = new ConfigLineParser(config.AsSpan());
int count = 0;
while (parser.TryGetNextEntry(out _, out _))
{
    count++;
}
Console.WriteLine($"Found {count} entries");  // Zero allocations!
```

### Selective Materialization

```csharp
// Only convert to string when needed
var parser = new ConfigLineParser(config.AsSpan());
while (parser.TryGetNextEntry(out var key, out var value))
{
    // Compare on spans - no allocation
    if (key.SequenceEqual("important_key".AsSpan()))
    {
        // Allocate string only for this specific value
        string importantValue = value.ToString();
        ProcessImportantValue(importantValue);
    }
}
```

### Storing in Dictionary

```csharp
// Pre-size dictionary to avoid resizing allocations
var dict = new Dictionary<string, string>(capacity: 10);
var parser = new ConfigLineParser(config.AsSpan());

while (parser.TryGetNextEntry(out var key, out var value))
{
    // Convert to strings for storage
    dict[key.ToString()] = value.ToString();
}
```

## 🔬 How It Works

### The `ref struct` Magic

```csharp
public ref struct ConfigLineParser
{
    private ReadOnlySpan<char> _remaining;  // Stack-only reference

    public ConfigLineParser(ReadOnlySpan<char> input)
    {
        _remaining = input;
    }

    public bool TryGetNextEntry(out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)
    {
        // Parse by slicing the span - no heap allocations
        // Returns false when no more entries
    }
}
```

**Key Concepts:**

- `ref struct`: Enforces stack-only allocation, cannot escape to heap
- `ReadOnlySpan<char>`: Represents a contiguous region of memory without copying
- **Slicing**: `span.Slice(start, length)` creates a new view, not a copy
- **Iterator pattern**: Returns spans pointing to the original string

## 📁 Project Structure

```
Zalloc/
├── Zalloc.App/
│   ├── ConfigLineParser.cs      # 🎯 Core: Zero-allocation parser
│   ├── ParsingBenchmarks.cs     # 📊 BenchmarkDotNet suite
│   ├── Program.cs               # 🚀 Entry point
│   └── BenchmarkDotNet.Artifacts/
│       └── results/             # 📈 Auto-generated reports
├── docs/
│   └── BENCHMARK_REPORT.md      # 📖 Detailed performance analysis
└── README.md                     # 📄 You are here
```

## 🎓 Learning Objectives

This project teaches you:

1. **`ReadOnlySpan<T>` fundamentals**: Stack-based memory views
2. **`ref struct` constraints**: Why they can't be boxed or stored in fields
3. **Zero-allocation patterns**: When and how to avoid heap allocations
4. **String slicing**: Parsing without creating substrings
5. **BenchmarkDotNet**: Professional .NET performance measurement
6. **Memory profiling**: Understanding allocation impact on performance
7. **Iterator patterns**: Implementing stateful parsers efficiently

## 🔧 Building the Project

```bash
# Debug build (for development)
dotnet build Zalloc.App/Zalloc.App.csproj

# Release build (for benchmarking)
dotnet build -c Release

# Run without benchmarks (just see the output)
dotnet run --project Zalloc.App
```

## 📚 Additional Resources

- **[Detailed Benchmark Report](docs/BENCHMARK_REPORT.md)** - Full analysis with charts and explanations
- **[BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)** - Learn about performance benchmarking
- **[Span<T> Guide](https://learn.microsoft.com/en-us/archive/msdn-magazine/2018/january/csharp-all-about-span-exploring-a-new-net-mainstay)** - Deep dive into spans
- **[Memory Management in .NET](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/)** - Understanding the heap and GC

## 🚨 Common Pitfalls

1. **Forgetting `.AsSpan()`**: Always convert strings to spans before parsing

   ```csharp
   var parser = new ConfigLineParser(input.AsSpan());  // ✅
   var parser = new ConfigLineParser(input);           // ❌ Won't compile
   ```

2. **Running in Debug mode**: Benchmarks are meaningless without optimizations

   ```bash
   dotnet run -c Release  # ✅ Correct
   dotnet run            # ❌ Wrong - Debug mode
   ```

3. **Calling `.ToString()` unnecessarily**: Defeats the zero-allocation purpose

   ```csharp
   // ❌ BAD - unnecessary allocation
   string k = key.ToString();
   if (k == "mykey") { }

   // ✅ GOOD - compare on spans
   if (key.SequenceEqual("mykey")) { }
   ```

4. **Trying to store spans in class fields**: `ref struct` can't escape the stack
   ```csharp
   class MyClass {
       private ReadOnlySpan<char> _data;  // ❌ Won't compile
   }
   ```

## 🤝 Contributing

This is an educational project! Contributions are welcome:

- 🐛 **Bug reports**: Found an issue? Open an issue!
- 💡 **Feature ideas**: More parsing scenarios? Educational examples?
- 📖 **Documentation**: Clarifications and improvements
- ⚡ **Performance**: Even faster implementations?

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details

## 🙏 Acknowledgments

Built with:

- [BenchmarkDotNet](https://benchmarkdotnet.org/) - Professional .NET benchmarking library
- [.NET](https://dotnet.microsoft.com/) - Modern, high-performance framework

---

**⭐ Star this repo if you learned something new about C# performance!**
