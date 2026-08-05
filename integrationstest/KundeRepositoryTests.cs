using NUnit.Framework;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Unittest;

public class KundeRepositoryTests : IntegrationTestBase
{
    private KundeRepository _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new KundeRepository(Db);
    }

    [Test]
    public void AddUser_hashes_password_so_it_is_not_stored_in_plain_text()
    {
        // Arrange
        var kunde = new Kunde(0, "Anders", "And", "a@a.dk", "Adr", "By", 2000, "", false);

        // Act
        _sut.AddUser(kunde, "Password1!");

        // Assert
        Assert.That(kunde.Kode, Is.Not.EqualTo("Password1!"));
        Assert.That(kunde.Kode, Is.Not.Empty);
    }

    [Test]
    public void VerifyPassword_returns_true_for_correct_password()
    {
        // Arrange
        var kunde = new Kunde(0, "Anders", "And", "a@a.dk", "Adr", "By", 2000, "", false);
        _sut.AddUser(kunde, "Password1!");

        // Act
        var result = _sut.VerifyPassword(kunde, "Password1!");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void VerifyPassword_returns_false_for_wrong_password()
    {
        // Arrange
        var kunde = new Kunde(0, "Anders", "And", "a@a.dk", "Adr", "By", 2000, "", false);
        _sut.AddUser(kunde, "Password1!");

        // Act
        var result = _sut.VerifyPassword(kunde, "ForkertKode123");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void FindByEmail_returns_null_when_kunde_does_not_exist()
    {
        // Act
        var result = _sut.FindByEmail("findes-ikke@a.dk");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindByEmail_returns_kunde_when_it_exists()
    {
        // Arrange
        var kunde = new Kunde(0, "Anders", "And", "a@a.dk", "Adr", "By", 2000, "", false);
        _sut.AddUser(kunde, "Password1!");

        // Act
        var result = _sut.FindByEmail("a@a.dk");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Navn, Is.EqualTo("Anders"));
    }

    [Test]
    public void RemoveUser_deletes_kunde_from_database()
    {
        // Arrange
        var kunde = new Kunde(0, "Anders", "And", "a@a.dk", "Adr", "By", 2000, "", false);
        _sut.AddUser(kunde, "Password1!");
        var gemtKunde = _sut.FindByEmail("a@a.dk"); // hent den trackede/gemte version fra DB

        // Act
        _sut.RemoveUser(gemtKunde!);

        // Assert
        Assert.That(_sut.FindByEmail("a@a.dk"), Is.Null);
    }
}