using System.Numerics;

namespace Abgerechnet.Core.Model;

/// <summary>
/// Checks and formats IBANs (ISO 13616): country code, two check digits, account number; the check digits are
/// verified with modulo 97.
/// </summary>
public static class Iban
{
    /// <summary>"de89 3704-0044 0532 0130 00" → "DE89370400440532013000".</summary>
    public static string Compact(string? iban) =>
        new((iban ?? string.Empty).Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    /// <summary>"DE89370400440532013000" → "DE89 3704 0044 0532 0130 00".</summary>
    public static string Format(string? iban)
    {
        var compact = Compact(iban);
        return string.Join(' ', compact.Chunk(4).Select(chunk => new string(chunk)));
    }

    /// <summary>True when the IBAN has a valid structure and check digits. Spaces and lower case are allowed.</summary>
    public static bool IsValid(string? iban)
    {
        var compact = Compact(iban);
        if (compact.Length is < 15 or > 34)
            return false;
        if (!char.IsAsciiLetterUpper(compact[0]) || !char.IsAsciiLetterUpper(compact[1])
            || !char.IsAsciiDigit(compact[2]) || !char.IsAsciiDigit(compact[3]))
            return false;
        if (!compact.All(char.IsAsciiLetterOrDigit))
            return false;
        // German IBANs always have 22 characters; a missing digit is the most common typo.
        if (compact.StartsWith("DE", StringComparison.Ordinal) && compact.Length != 22)
            return false;

        // Move the first four characters to the end, replace letters by 10..35, the number mod 97 must be 1.
        var rearranged = compact[4..] + compact[..4];
        var digits = string.Concat(rearranged.Select(c => char.IsAsciiDigit(c) ? c.ToString() : (c - 'A' + 10).ToString()));
        return BigInteger.Parse(digits) % 97 == 1;
    }
}
