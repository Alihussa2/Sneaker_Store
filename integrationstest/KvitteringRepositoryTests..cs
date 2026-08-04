using NUnit.Framework;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Unittest;

public class KvitteringRepositoryTests : IntegrationTestBase
{
    private KvitteringRepository _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new KvitteringRepository(Db);
    }

    [Test]
    public void OpretKvittering_then_HentKvittering_returns_the_created_kvittering()
    {
        // Arrange
        var kvittering = new Kvittering(0, kundeId: 1, antal: 2, totalPris: 1000,
            beskrivelse: "Nike Air Max (str. 42) x2", koebsdato: new DateTime(2026, 1, 15));

        // Act
        _sut.OpretKvittering(kvittering);
        var fundet = _sut.HentKvittering(kvittering.Id);

        // Assert – comprehensive: alle felter tjekkes
        Assert.That(fundet, Is.Not.Null);
        Assert.That(fundet!.KundeId, Is.EqualTo(1));
        Assert.That(fundet.Antal, Is.EqualTo(2));
        Assert.That(fundet.TotalPris, Is.EqualTo(1000));
        Assert.That(fundet.Beskrivelse, Is.EqualTo("Nike Air Max (str. 42) x2"));
        Assert.That(fundet.Koebsdato, Is.EqualTo(new DateTime(2026, 1, 15)));
    }

    [Test]
    public void HentKvittering_returns_null_when_kvittering_does_not_exist()
    {
        // Act
        var result = _sut.HentKvittering(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void HentAlleKvitteringer_returns_all_created_kvitteringer()
    {
        // Arrange
        _sut.OpretKvittering(new Kvittering(0, kundeId: 1, antal: 1, totalPris: 500, beskrivelse: "Sko A x1", koebsdato: DateTime.Now));
        _sut.OpretKvittering(new Kvittering(0, kundeId: 2, antal: 3, totalPris: 1500, beskrivelse: "Sko B x3", koebsdato: DateTime.Now));

        // Act
        var alle = _sut.HentAlleKvitteringer().ToList();

        // Assert
        Assert.That(alle.Count, Is.EqualTo(2));
    }

    [Test]
    public void HentAlleKvitteringer_returns_empty_list_when_none_exist()
    {
        // Act
        var alle = _sut.HentAlleKvitteringer().ToList();

        // Assert
        Assert.That(alle, Is.Empty);
    }
}