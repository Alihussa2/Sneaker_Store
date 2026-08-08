using System.Text.RegularExpressions;

namespace Sneaker_Store.Validation;

public static class PasswordPolicy
{
    // Mindst 7 tegn, mindst ét lille bogstav, ét stort bogstav, ét tal og ét specialtegn.
    private static readonly Regex Regel = new(
        // \z (ikke $) for at anker helt til den absolutte slutning af strengen -
        // $ ville også matche lige før et afsluttende linjeskift, hvilket ville lade
        // et kodeord uden noget rigtigt specialtegn snige sig igennem via et "\n" til sidst.
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{7,}\z",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    public const string Beskrivelse = "Mindst 7 tegn, med store og små bogstaver, mindst ét tal og ét specialtegn.";

    public static bool ErGyldig(string? kode)
    {
        if (kode is null)
        {
            return false;
        }

        try
        {
            return Regel.IsMatch(kode);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}