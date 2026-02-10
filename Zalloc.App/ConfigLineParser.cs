namespace Zalloc.App;

/// <summary>
/// Zero-allocation parser for key=value pairs separated by semicolons
/// Uses ReadOnlySpan to avoid heap allocations
/// </summary>
public ref struct ConfigLineParser
{
    private ReadOnlySpan<char> _remaining;

    public ConfigLineParser(ReadOnlySpan<char> input)
    {
        _remaining = input;
    }

    public bool TryGetNextEntry(out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)
    {
        key = default;
        value = default;

        // No more data to parse
        if (_remaining.Length == 0)
            return false;

        // Find the next semicolon (pair separator)
        int semicolonIndex = _remaining.IndexOf(';');
        ReadOnlySpan<char> currentPair;

        if (semicolonIndex >= 0)
        {
            // Extract current pair and advance remaining
            currentPair = _remaining.Slice(0, semicolonIndex);
            _remaining = _remaining.Slice(semicolonIndex + 1);
        }
        else
        {
            // Last pair (no semicolon at the end)
            currentPair = _remaining;
            _remaining = ReadOnlySpan<char>.Empty;
        }

        // Find the equals sign (key-value separator)
        int equalsIndex = currentPair.IndexOf('=');
        if (equalsIndex < 0)
            return false; // Invalid format

        // Extract key and value (all on stack, zero allocations!)
        key = currentPair.Slice(0, equalsIndex);
        value = currentPair.Slice(equalsIndex + 1);

        return true;
    }
}
