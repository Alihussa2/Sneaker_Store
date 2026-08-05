using NUnit.Framework;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Unittest;

// INTEGRATIONSTEST: rammer en rigtig AppDbContext (EF Core InMemory) via IntegrationTestBase - ingen mocking
public class SkoRepositoryTests : IntegrationTestBase
{
    private SkoRepository _sut;

    // [SetUp] køres FØR HVER test -> frisk InMemory-database hver gang
    [SetUp]
    public void SetUp()
    {
        _sut = new SkoRepository(Db);
    }

    // IKKE parametriseret: black-box - negativ case, sko findes ikke
    [Test]
    public void GetById_throws_KeyNotFoundException_when_sko_does_not_exist()
    {
        // Act + Assert
        Assert.Throws<KeyNotFoundException>(() => _sut.GetById(999));
    }

    // IKKE parametriseret: black-box - tilføj og hent igen, positiv case
    [Test]
    public void Add_then_GetAll_returns_the_added_shoe()
    {
        // Arrange
        var nySko = new Sko(0, "Nike", "Air Max", 42, 999, lagerAntal: 5);

        // Act
        var tilfoejet = _sut.Add(nySko);
        var alle = _sut.GetAll();

        // Assert
        Assert.That(tilfoejet.SkoId, Is.GreaterThan(0));
        Assert.That(alle.Count, Is.EqualTo(1));
        Assert.That(alle[0].Maerke, Is.EqualTo("Nike"));
        Assert.That(alle[0].LagerAntal, Is.EqualTo(5));
    }

    // BLACK-BOX: boundary value analysis (præcis på grænsen, lige under, normalt tilfælde)
    // PARAMETRISERET: [TestCase] x3
    [TestCase(10, 10, 0)]   // køber præcis alt på lager
    [TestCase(10, 9, 1)]    // lige under grænsen
    [TestCase(10, 1, 9)]    // normalt tilfælde
    public void ReducerLager_reduces_stock_correctly_when_enough_available(int startLager, int antalKoebt, int forventetLager)
    {
        // Arrange
        var sko = _sut.Add(new Sko(0, "Nike", "Air Max", 42, 999, lagerAntal: startLager));

        // Act
        var result = _sut.ReducerLager(sko.SkoId, antalKoebt);

        // Assert
        Assert.That(result.LagerAntal, Is.EqualTo(forventetLager));
        Assert.That(result.SkoId, Is.EqualTo(sko.SkoId));
    }

    // BLACK-BOX: boundary value analysis (1 over grænsen, langt over grænsen)
    // PARAMETRISERET: [TestCase] x2
    [TestCase(10, 11)]
    [TestCase(10, 100)]
    public void ReducerLager_throws_when_antal_exceeds_stock(int startLager, int antalKoebt)
    {
        // Arrange
        var sko = _sut.Add(new Sko(0, "Nike", "Air Max", 42, 999, lagerAntal: startLager));

        // Act + Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _sut.ReducerLager(sko.SkoId, antalKoebt));
        Assert.That(ex.Message, Does.Contain(startLager.ToString()));
    }

    // IKKE parametriseret: black-box - negativ case, ID mismatch ved opdatering
    [Test]
    public void Update_throws_ArgumentException_when_id_mismatch()
    {
        // Arrange
        var sko = _sut.Add(new Sko(0, "Nike", "Air Max", 42, 999));
        var opdateret = new Sko(sko.SkoId + 1, "Adidas", "Ultraboost", 43, 1200);

        // Act + Assert
        Assert.Throws<ArgumentException>(() => _sut.Update(sko.SkoId, opdateret));
    }

    // IKKE parametriseret: black-box - opdatering lykkes, alle felter ændres
    [Test]
    public void Update_changes_all_fields_when_id_matches()
    {
        // Arrange
        var sko = _sut.Add(new Sko(0, "Nike", "Air Max", 42, 999, lagerAntal: 5));
        var opdateret = new Sko(sko.SkoId, "Adidas", "Ultraboost", 43, 1200, lagerAntal: 20);

        // Act
        var result = _sut.Update(sko.SkoId, opdateret);

        // Assert – comprehensive: alle 5 felter tjekkes
        Assert.That(result.Maerke, Is.EqualTo("Adidas"));
        Assert.That(result.Model, Is.EqualTo("Ultraboost"));
        Assert.That(result.Str, Is.EqualTo(43));
        Assert.That(result.Pris, Is.EqualTo(1200));
        Assert.That(result.LagerAntal, Is.EqualTo(20));
    }

    // IKKE parametriseret: black-box - sletning lykkes
    [Test]
    public void Delete_removes_shoe_from_database()
    {
        // Arrange
        var sko = _sut.Add(new Sko(0, "Nike", "Air Max", 42, 999));

        // Act
        var slettet = _sut.Delete(sko.SkoId);

        // Assert
        Assert.That(slettet.SkoId, Is.EqualTo(sko.SkoId));
        Assert.Throws<KeyNotFoundException>(() => _sut.GetById(sko.SkoId));
    }
}