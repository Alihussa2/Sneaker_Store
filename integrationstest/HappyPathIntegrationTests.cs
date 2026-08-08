using NUnit.Framework;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Unittest;

// BRED "happy path"-integrationstest (jf. Khorikov, Integration Testing-slidet):
// i stedet for at teste ét repository ad gangen, følger denne test hele forretningsflowet
// gennem FLERE repositories samtidig - ligesom slidets eget eksempel
// ("add five products to the cart, confirm delivery address, check out").
// De øvrige integrationstests (KundeRepositoryTests osv.) er bevidst smalle og dækker
// edge-cases pr. repository; denne test dækker i stedet den længste sammenhængende,
// realistiske brugerrejse på tværs af Kunde -> Sko -> Ordre -> Kvittering.
public class HappyPathIntegrationTests : IntegrationTestBase
{
    [Test]
    public void Customer_registers_buys_a_sneaker_and_gets_a_receipt()
    {
        // Arrange
        var kundeRepo = new KundeRepository(Db);
        var skoRepo = new SkoRepository(Db);
        var ordreRepo = new OrdreRepository(Db);
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
        ordreRepo.TilføjOrdre(ordre);
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
        Assert.That(ordreRepo.HentAlleOrdrer().Count(), Is.EqualTo(1));
        var gemtOrdre = ordreRepo.HentAlleOrdrer().Single();
        Assert.That(gemtOrdre.KundeId, Is.EqualTo(gemtKunde.KundeId));
        Assert.That(gemtOrdre.SkoId, Is.EqualTo(sko.SkoId));
        var kvitteringer = kvitteringRepo.HentAlleKvitteringer().ToList();
        Assert.That(kvitteringer, Has.Count.EqualTo(1));
        Assert.That(kvitteringer[0].TotalPris, Is.EqualTo(899.0));
    }
}
