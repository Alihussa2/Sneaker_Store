using NUnit.Framework;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Unittest;

// INTEGRATIONSTEST: rammer en rigtig AppDbContext (EF Core InMemory) via IntegrationTestBase - ingen mocking
public class KvitteringRepositoryTests : IntegrationTestBase
{
    private KvitteringRepository _sut;

    // [SetUp] køres FØR HVER test -> frisk InMemory-database hver gang
    [SetUp]
    public void SetUp()
    {
        _sut = new KvitteringRepository(Db);
    }

    // IKKE parametriseret: black-box - opret og find igen, positiv case
    [Test]
    public void OpretKvittering_then_HentKvittering_returns_the_created_kvittering()
    {
        // Arrange
        var kvittering = new Kvittering(0, kundeId: 1, antal: 2, totalPris: 1000,
            beskrivelse: "Nike Air Max (str. 42) x2", koebsdato: new DateTime(2026, 1, 15));

        // Act
        _sut.OpretKvittering(kvittering);
        var fundet = _sut.HentKvittering(kvittering.Id);

        // Assert – comprehensive: alle 5 felter tjekkes, ikke kun at objektet "findes"
        Assert.That(fundet, Is.Not.Null);
        Assert.That(fundet!.KundeId, Is.EqualTo(1));
        Assert.That(fundet.Antal, Is.EqualTo(2));
        Assert.That(fundet.TotalPris, Is.EqualTo(1000));
        Assert.That(fundet.Beskrivelse, Is.EqualTo("Nike Air Max (str. 42) x2"));
        Assert.That(fundet.Koebsdato, Is.EqualTo(new DateTime(2026, 1, 15)));
    }

}