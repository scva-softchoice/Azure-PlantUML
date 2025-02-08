using System.Drawing;

/// <summary>
/// Provides utility functions.
/// </summary>
public static class Util
{
    /// <summary>
    /// Converts a Color to its hexadecimal string representation.
    /// </summary>
    /// <param name="c">The Color to convert.</param>
    /// <returns>The hexadecimal string representation of the color.</returns>
    public static string ToHexString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    /// <summary>
    /// Converts a Color to its RGB string representation.
    /// </summary>
    /// <param name="c">The Color to convert.</param>
    /// <returns>The RGB string representation of the color.</returns>
    public static string ToRgbString(this Color c) => $"RGB({c.R}, {c.G}, {c.B})";

    /// <summary>
    /// Replaces a portion of a string at the specified index with the provided replacement string.
    /// </summary>
    /// <param name="str">The original string.</param>
    /// <param name="index">The starting index of the portion to replace.</param>
    /// <param name="length">The length of the portion to replace.</param>
    /// <param name="replace">The replacement string.</param>
    /// <returns>The modified string with the specified portion replaced.</returns>
    public static string ReplaceAt(this string str, int index, int length, string replace)
    {
        return str.Remove(index, Math.Min(length, str.Length - index)).Insert(index, replace);
    }
}
