using NUnit.Framework;
using Sneaker_Store.Validation;

namespace Unittest;

// Unit test: PasswordPolicy har ingen dependencies overhovedet -> ren, isoleret funktion, intet at mocke
public class PasswordPolicyTests
{
    // IKKE parametriseret: enkelt edge case (null-input)
    [Test]
    public void ErGyldig_returns_false_when_password_is_nul()
    {
        // Act
        var result = PasswordPolicy.ErGyldig(null);

        // Assert
        Assert.That(result, Is.False);
    }

    // BLACK-BOX: decision table (5 betingelser: længde/småt/stort/tal/specialtegn -> resultat)
    // PARAMETRISERET: [TestCase] x9 - dækker alle kombinationer hvor én betingelse fejler ad gangen
    [TestCase("", false, "Tom streng")]
    [TestCase("Ab1!", false, "For kort (kun 4 tegn) selvom alle kategorier er opfyldt")]
    [TestCase("Abcde1", false, "6 tegn - lige under grænsen, mangler specialtegn")]
    [TestCase("Abcdef1!", true, "8 tegn, alle krav opfyldt")]
    [TestCase("abcdefg1!", false, "Mangler stort bogstav")]
    [TestCase("ABCDEFG1!", false, "Mangler lille bogstav")]
    [TestCase("Abcdefgh!", false, "Mangler tal")]
    [TestCase("Abcdefg1", false, "Mangler specialtegn")]
    [TestCase("Password1!", true, "Almindeligt gyldigt kodeord")]
    public void ErGyldig_validates_password_according_to_policy(string kode, bool forventetResultat, string beskrivelse)
    {
        // Act
        var result = PasswordPolicy.ErGyldig(kode);

        // Assert - "beskrivelse" bruges som fejlbesked, gør det let at se hvilken case der evt. fejler
        Assert.That(result, Is.EqualTo(forventetResultat), beskrivelse);
    }

    // BLACK-BOX: boundary value analysis (grænseværdi = 7 tegn)
    // PARAMETRISERET: [TestCase] x2 - præcis på grænsen vs. lige under
    [TestCase("Abcde1!", true)]   // 7 tegn - nedre grænse, skal være gyldig
    [TestCase("Abcd1!", false)]   // 6 tegn - lige under grænsen, skal være ugyldig
    public void ErGyldig_respects_minimum_length_boundary(string kode, bool forventetResultat)
    {
        // Act
        var result = PasswordPolicy.ErGyldig(kode);

        // Assert
        Assert.That(result, Is.EqualTo(forventetResultat));
    }

    // IKKE parametriseret: simpelt tjek af statisk tekststreng, ikke selve valideringslogikken
    [Test]
    public void Beskrivelse_is_not_empty_and_describes_the_requirements()
    {
        // Assert
        Assert.That(PasswordPolicy.Beskrivelse, Is.Not.Empty);
        Assert.That(PasswordPolicy.Beskrivelse, Does.Contain("7 tegn"));
    }
}