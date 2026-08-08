using NUnit.Framework;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Unittest;

// INTEGRATIONSTEST: rammer en rigtig AppDbContext (EF Core InMemory) via IntegrationTestBase - ingen mocking
public class OrdreRepositoryTests : IntegrationTestBase
{
    private OrdreRepository _sut;

    // [SetUp] køres FØR HVER test -> frisk InMemory-database hver gang
    [SetUp]
    public void SetUp()
    {
        _sut = new OrdreRepository(Db);
    }

    // BRED "happy path"-integrationstest (jf. Khorikov, Integration Testing-slidet):
    // i stedet for kun at teste ét repository ad gangen, følger denne test hele
    // forretningsflowet gennem FLERE repositories samtidig - ligesom slidets eget
    // eksempel ("add five products to the cart, confirm delivery address, check out").
    // Ligger her fordi det er oprettelsen af selve Ordren, der binder de andre tre
    // repositories (Kunde, Sko, Kvittering) sammen.
    [Test]
    public void Customer_registers_buys_a_sneaker_and_gets_a_receipt()
    {
        // Arrange
        var kundeRepo = new KundeRepository(Db);
        var skoRepo = new SkoRepository(Db);
        var kvitteringRepo = new KvitteringRepository(Db);

        // Act 1: kunden registrerer sig
        var kunde = new Kunde(0, "Anders", "And", "anders@and.dk", "Andebyvej 1", "Andeby", 2000, "", false);
        kundeRepo.AddUser(kunde, "Andeby123!");
        var gemtKunde = kundeRepo.FindByEmail("anders@and.dk");

        // Act 2: admin opretter en sko i kataloget
        var sko = skoRepo.Add(new Sko(0, "Nike", "Air Max 90", 42, 899.0, 5));

        // Act 3: kunden køber skoen (samme rækkefølge som OrdreController.Create: tjek lager -> reducer -> opret ordre -> opret kvittering)
        skoRepo.ReducerLager(sko.SkoId, 1);
        var ordre = new Ordre(0, gemtKunde!.KundeId, sko.SkoId, 1, sko.Pris);
        _sut.TilføjOrdre(ordre);
        kvitteringRepo.OpretKvittering(new Kvittering(
            id: 0,
            kundeId: gemtKunde.KundeId,
            antal: 1,
            totalPris: sko.Pris,
            beskrivelse: $"{sko.Maerke} {sko.Model} (str. {sko.Str}) x1",
            koebsdato: DateTime.Now));

        // Assert: hele kæden hænger sammen, på tværs af alle fire repositories
        Assert.That(kundeRepo.FindByEmail("anders@and.dk"), Is.Not.Null);
        Assert.That(skoRepo.GetById(sko.SkoId).LagerAntal, Is.EqualTo(4), "lager skal være reduceret med 1");
        Assert.That(_sut.HentAlleOrdrer().Count(), Is.EqualTo(1));
        var gemtOrdre = _sut.HentAlleOrdrer().Single();
        Assert.That(gemtOrdre.KundeId, Is.EqualTo(gemtKunde.KundeId));
        Assert.That(gemtOrdre.SkoId, Is.EqualTo(sko.SkoId));
        var kvitteringer = kvitteringRepo.HentAlleKvitteringer().ToList();
        Assert.That(kvitteringer, Has.Count.EqualTo(1));
        Assert.That(kvitteringer[0].TotalPris, Is.EqualTo(899.0));
    }

    // IKKE parametriseret: black-box - tilføj og find igen, positiv case
    [Test]
    public void TilfoejOrdre_then_FindOrdre_returns_the_added_order()
    {
        // Arrange
        var ordre = new Ordre(0, kundeId: 1, skoId: 1, antal: 2, totalPris: 1000);

        // Act
        _sut.TilføjOrdre(ordre);
        var fundet = _sut.FindOrdre(ordre.OrdreId);

        // Assert - comprehensive: alle 4 felter tjekkes
        Assert.That(fundet, Is.Not.Null);
        Assert.That(fundet!.KundeId, Is.EqualTo(1));
        Assert.That(fundet.SkoId, Is.EqualTo(1));
        Assert.That(fundet.Antal, Is.EqualTo(2));
        Assert.That(fundet.TotalPris, Is.EqualTo(1000));
    }

    // IKKE parametriseret: black-box - opdatering lykkes, alle felter ændres
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

        // Assert – comprehensive: alle 4 felter tjekkes efter opdatering
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.KundeId, Is.EqualTo(2));
        Assert.That(result.SkoId, Is.EqualTo(3));
        Assert.That(result.Antal, Is.EqualTo(5));
        Assert.That(result.TotalPris, Is.EqualTo(2500));
    }

    // IKKE parametriseret: black-box - negativ case, opdatering af ikke-eksisterende ordre
    [Test]
    public void OpdaterOrdre_throws_KeyNotFoundException_when_ordre_does_not_exist()
    {
        // Arrange
        var ordre = new Ordre(999, kundeId: 1, skoId: 1, antal: 1, totalPris: 500);

        // Act + Assert
        Assert.Throws<KeyNotFoundException>(() => _sut.OpdaterOrdre(ordre));
    }

    // IKKE parametriseret: black-box - sletning lykkes
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
}