using NUnit.Framework;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Unittest;

// INTEGRATIONSTEST: rammer en rigtig AppDbContext (EF Core InMemory) via IntegrationTestBase - ingen mocking
// -> spg. 22-23 relevante: her ses konkret hvordan man tester mod en database uden at bruge den rigtige produktions-DB
public class KundeRepositoryTests : IntegrationTestBase
{
    private KundeRepository _sut;

    // [SetUp] køres FØR HVER test -> frisk InMemory-database hver gang (arves fra IntegrationTestBase.BaseSetUp)
    [SetUp]
    public void SetUp()
    {
        _sut = new KundeRepository(Db);
    }

    // IKKE parametriseret: black-box - password skal hashes, ikke gemmes i klartekst
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

    // IKKE parametriseret: black-box - positiv case, korrekt password
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

    // IKKE parametriseret: black-box - negativ case, forkert password
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

    // IKKE parametriseret: black-box - kunde findes ikke
    [Test]
    public void FindByEmail_returns_null_when_kunde_does_not_exist()
    {
        // Act
        var result = _sut.FindByEmail("findes-ikke@a.dk");

        // Assert
        Assert.That(result, Is.Null);
    }

    // IKKE parametriseret: black-box - kunde findes
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

    // IKKE parametriseret: black-box - sletning lykkes
    [Test]
    public void RemoveUser_deletes_kunde_from_database()
    {
        // Arrange
        var kunde = new Kunde(0, "Anders", "And", "a@a.dk", "Adr", "By", 2000, "", false);
        _sut.AddUser(kunde, "Password1!");
        var gemtKunde = _sut.FindByEmail("a@a.dk"); // hent trackede version - undgår "detached entity"-fejl i EF Core

        // Act
        _sut.RemoveUser(gemtKunde!);

        // Assert
        Assert.That(_sut.FindByEmail("a@a.dk"), Is.Null);
    }
}