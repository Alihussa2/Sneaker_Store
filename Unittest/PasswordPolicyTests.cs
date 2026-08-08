using NUnit.Framework;
using Sneaker_Store.Validation;

namespace Unittest;

// Unit test: PasswordPolicy har ingen dependencies overhovedet -> ren, isoleret funktion, intet at mocke
// Testcases er sporet direkte til Black-Box Test Design-dokumentet (afsnit 2.2 og 2.3, TC02-TC06, TC12-TC14, R1-R6)
public class PasswordPolicyTests
{
    // IKKE parametriseret: enkelt edge case (null-input)
    [Test]
    public void ErGyldig_returns_false_when_password_is_null()
    {
        // Arrange
        string? kode = null;

        // Act
        var result = PasswordPolicy.ErGyldig(kode);

        // Assert
        Assert.That(result, Is.False);
    }

    // BLACK-BOX: decision table (jf. dokumentets afsnit 2.3, R1-R6: 5 uafhængige betingelser -> resultat)
    // PARAMETRISERET: [TestCase] x6 - hver case isolerer PRÆCIS én betingelse ad gangen.
    // Passwords er identiske med dokumentets TC02-TC06, for fuld sporbarhed.
    [TestCase("Sko123!", true, "R1 / TC13: alt opfyldt -> Approved")]
    [TestCase("Sk1!", false, "R2 / TC02: kun længden fejler -> for kort")]
    [TestCase("sko123!", false, "R3 / TC03: kun stort bogstav mangler")]
    [TestCase("SKO123!", false, "R4 / TC04: kun lille bogstav mangler")]
    [TestCase("Skoabcd!", false, "R5 / TC05: kun tal mangler")]
    [TestCase("Sko1234", false, "R6 / TC06: kun specialtegn mangler")]
    public void ErGyldig_validates_password_according_to_decision_table(string kode, bool forventetResultat, string beskrivelse)
    {
        // Arrange
        var input = kode;

        // Act
        var result = PasswordPolicy.ErGyldig(input);

        // Assert
        Assert.That(result, Is.EqualTo(forventetResultat), beskrivelse);
    }

    // BLACK-BOX: equivalence class - EP7 (tom streng, jf. dokumentets afsnit 2.1)
    [Test]
    public void ErGyldig_returns_false_when_password_is_empty_string()
    {
        // Arrange
        var kode = "";

        // Act
        var result = PasswordPolicy.ErGyldig(kode);

        // Assert
        Assert.That(result, Is.False);
    }

    // BLACK-BOX: boundary value analysis (jf. dokumentets afsnit 2.2, TC12-TC14: grænseværdi = 7 tegn)
    [TestCase("Sko12!", false, "TC12: 6 tegn - under grænsen")]
    [TestCase("Sko123!", true, "TC13: 7 tegn - på grænsen")]
    [TestCase("Skoo123!", true, "TC14: 8 tegn - over grænsen")]
    public void ErGyldig_respects_minimum_length_boundary(string kode, bool forventetResultat, string beskrivelse)
    {
        // Arrange
        var input = kode;

        // Act
        var result = PasswordPolicy.ErGyldig(input);

        // Assert
        Assert.That(result, Is.EqualTo(forventetResultat), beskrivelse);
    }

    [Test]
    public void ErGyldig_returns_true_for_a_typical_valid_password()
    {
        // Arrange
        var kode = "Password1!";

        // Act
        var result = PasswordPolicy.ErGyldig(kode);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Beskrivelse_is_not_empty_and_describes_the_requirements()
    {
        // Arrange
        var forventetDelstreng = "7 tegn";

        // Act
        var beskrivelse = PasswordPolicy.Beskrivelse;

        // Assert
        Assert.That(PasswordPolicy.Beskrivelse, Is.Not.Empty);
        Assert.That(PasswordPolicy.Beskrivelse, Does.Contain("7 tegn"));
    }
}