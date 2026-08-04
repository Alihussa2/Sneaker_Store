using NUnit.Framework;
using Sneaker_Store.Validation;

namespace Unittest;

public class PasswordPolicyTests
{
    [Test]
    public void ErGyldig_returns_false_when_password_is_null()
    {
        // Act
        var result = PasswordPolicy.ErGyldig(null);

        // Assert
        Assert.That(result, Is.False);
    }

    // Decision table fra black-box designet:
    // Længde | Småt bogstav | Stort bogstav | Tal | Specialtegn | -> Forventet resultat
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

        // Assert
        Assert.That(result, Is.EqualTo(forventetResultat), beskrivelse);
    }

    // Boundary value analysis: præcis på grænsen (7 tegn) vs. lige under (6 tegn)
    [TestCase("Abcde1!", true)]   // præcis 7 tegn - nedre grænse, skal være gyldig
    [TestCase("Abcd1!", false)]   // 6 tegn - lige under grænsen, skal være ugyldig
    public void ErGyldig_respects_minimum_length_boundary(string kode, bool forventetResultat)
    {
        // Act
        var result = PasswordPolicy.ErGyldig(kode);

        // Assert
        Assert.That(result, Is.EqualTo(forventetResultat));
    }

    [Test]
    public void Beskrivelse_is_not_empty_and_describes_the_requirements()
    {
        // Assert
        Assert.That(PasswordPolicy.Beskrivelse, Is.Not.Empty);
        Assert.That(PasswordPolicy.Beskrivelse, Does.Contain("7 tegn"));
    }
}