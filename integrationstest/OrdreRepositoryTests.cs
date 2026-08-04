using NUnit.Framework;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Unittest;

public class OrdreRepositoryTests : IntegrationTestBase
{
    private OrdreRepository _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new OrdreRepository(Db);
    }

    [Test]
    public void TilfoejOrdre_then_FindOrdre_returns_the_added_order()
    {
        // Arrange
        var ordre = new Ordre(0, kundeId: 1, skoId: 1, antal: 2, totalPris: 1000);

        // Act
        _sut.TilføjOrdre(ordre);
        var fundet = _sut.FindOrdre(ordre.OrdreId);

        // Assert
        Assert.That(fundet, Is.Not.Null);
        Assert.That(fundet!.KundeId, Is.EqualTo(1));
        Assert.That(fundet.SkoId, Is.EqualTo(1));
        Assert.That(fundet.Antal, Is.EqualTo(2));
        Assert.That(fundet.TotalPris, Is.EqualTo(1000));
    }

    [Test]
    public void FindOrdre_returns_null_when_ordre_does_not_exist()
    {
        // Act
        var result = _sut.FindOrdre(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void HentAlleOrdrer_returns_all_added_orders()
    {
        // Arrange
        _sut.TilføjOrdre(new Ordre(0, kundeId: 1, skoId: 1, antal: 1, totalPris: 500));
        _sut.TilføjOrdre(new Ordre(0, kundeId: 2, skoId: 2, antal: 3, totalPris: 1500));

        // Act
        var alle = _sut.HentAlleOrdrer().ToList();

        // Assert
        Assert.That(alle.Count, Is.EqualTo(2));
    }

    [Test]
    public void OpdaterOrdre_updates_all_fields_when_ordre_exists()
    {
        // Arrange
        var ordre = new Ordre(0, kundeId: 1, skoId: 1, antal: 1, totalPris: 500);
        _sut.TilføjOrdre(ordre);
        var opdateret = new Ordre(ordre.OrdreId, kundeId: 2, skoId: 3, antal: 5, totalPris: 2500);

        // Act
        _sut.OpdaterOrdre(opdateret);
        var result = _sut.FindOrdre(ordre.OrdreId);

        // Assert – comprehensive: alle felter tjekkes
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.KundeId, Is.EqualTo(2));
        Assert.That(result.SkoId, Is.EqualTo(3));
        Assert.That(result.Antal, Is.EqualTo(5));
        Assert.That(result.TotalPris, Is.EqualTo(2500));
    }

    [Test]
    public void OpdaterOrdre_throws_KeyNotFoundException_when_ordre_does_not_exist()
    {
        // Arrange
        var ordre = new Ordre(999, kundeId: 1, skoId: 1, antal: 1, totalPris: 500);

        // Act + Assert
        Assert.Throws<KeyNotFoundException>(() => _sut.OpdaterOrdre(ordre));
    }

    [Test]
    public void SletOrdre_removes_ordre_from_database()
    {
        // Arrange
        var ordre = new Ordre(0, kundeId: 1, skoId: 1, antal: 1, totalPris: 500);
        _sut.TilføjOrdre(ordre);

        // Act
        _sut.SletOrdre(ordre.OrdreId);

        // Assert
        Assert.That(_sut.FindOrdre(ordre.OrdreId), Is.Null);
    }

    [Test]
    public void SletOrdre_does_nothing_when_ordre_does_not_exist()
    {
        // Act + Assert – skal ikke kaste exception, selvom ordren ikke findes
        Assert.DoesNotThrow(() => _sut.SletOrdre(999));
    }
}