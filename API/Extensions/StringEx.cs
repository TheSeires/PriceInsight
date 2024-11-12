using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace API.Extensions;

public static class StringEx
{
    public static string ToBase64(this string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    public static string FromBase64(this string value)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(value));
    }

    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static string IfNullOrEmpty(this string? value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    public static string Uncapitilize(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        ReadOnlySpan<char> span = input.AsSpan();
        char[] result = new char[span.Length];
        result[0] = char.ToLower(span[0]);
        span[1..].CopyTo(result.AsSpan(1));

        return new string(result);
    }
}
