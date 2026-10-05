namespace Abgerechnet.Core.Model;

/// <summary>
/// Compares invoice numbers the way people read them: digit groups by value, so "RE-9" comes before "RE-10" and
/// "111409" before "111410". Case is ignored.
/// </summary>
public sealed class Rechnungsnummer : IComparer<string?>
{
    public static readonly Rechnungsnummer Vergleich = new();

    public int Compare(string? x, string? y)
    {
        x ??= string.Empty;
        y ??= string.Empty;
        int i = 0, j = 0;
        while (i < x.Length && j < y.Length)
        {
            if (char.IsAsciiDigit(x[i]) && char.IsAsciiDigit(y[j]))
            {
                var startX = i;
                var startY = j;
                while (i < x.Length && char.IsAsciiDigit(x[i])) i++;
                while (j < y.Length && char.IsAsciiDigit(y[j])) j++;
                var numberX = x[startX..i].TrimStart('0');
                var numberY = y[startY..j].TrimStart('0');
                // Longer number (without leading zeros) is larger; same length compares digit by digit.
                var result = numberX.Length != numberY.Length
                    ? numberX.Length.CompareTo(numberY.Length)
                    : string.CompareOrdinal(numberX, numberY);
                if (result != 0)
                    return result;
            }
            else
            {
                var result = char.ToUpperInvariant(x[i]).CompareTo(char.ToUpperInvariant(y[j]));
                if (result != 0)
                    return result;
                i++;
                j++;
            }
        }
        return (x.Length - i).CompareTo(y.Length - j);
    }
}
