using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Sneaker_Store.E2ETests;

// http://localhost:5083,
[TestFixture]
public class ShopTests : PageTest
{
    private const string BaseUrl = "http://localhost:5083";

    private static string NytEmail() => $"e2e{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}@sneakerstore.dk";

    // Forsiden viser overskrift og mindst én sko
    [Test]
    public async Task Forside_ViserOverskriftOgSko()
    {
        await Page.GotoAsync(BaseUrl);

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Sneaker Store" })).ToBeVisibleAsync();
        await Expect(Page.Locator(".sko-kort").First).ToBeVisibleAsync();
    }

    // Login med rigtige oplysninger sender brugeren videre fra login-siden
    [Test]
    public async Task Login_MedGyldigeOplysninger_Redirecter()
    {
        await Page.GotoAsync($"{BaseUrl}/login.html");

        await Page.FillAsync("input[type=email]", "test@sneakerstore.dk");
        await Page.FillAsync("input[type=password]", "Test1234!");
        await Page.ClickAsync("button[type=submit]");

        await Expect(Page).Not.ToHaveURLAsync($"{BaseUrl}/login.html");
    }

    // Happy path: log ind, køb en sko, se den på "Min side"
    [Test]
    public async Task MinSide_ViserOrdreEfterKøb()
    {
        await Page.GotoAsync($"{BaseUrl}/login.html");
        await Page.FillAsync("input[type=email]", "test@sneakerstore.dk");
        await Page.FillAsync("input[type=password]", "Test1234!");
        await Page.ClickAsync("button[type=submit]");
        await Expect(Page).Not.ToHaveURLAsync($"{BaseUrl}/login.html"); // vent på login-redirect

        await Page.GotoAsync(BaseUrl);
        await Expect(Page.Locator(".sko-kort").First).ToBeVisibleAsync();
        await Page.Locator(".btn-koeb").First.ClickAsync();
        await Expect(Page.Locator("#koebBesked")).ToContainTextAsync("Tak for din ordre");

        await Page.GotoAsync($"{BaseUrl}/min-side.html");
        await Expect(Page.Locator("#ordreTabel tr").First).ToBeVisibleAsync();
    }

    // Logout viser login-linket igen
    [Test]
    public async Task Logout_EfterLogin_ViserLoginLinkIgen()
    {
        await Page.GotoAsync($"{BaseUrl}/login.html");
        await Page.FillAsync("input[type=email]", "test@sneakerstore.dk");
        await Page.FillAsync("input[type=password]", "Test1234!");
        await Page.ClickAsync("button[type=submit]");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Logout" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();
    }

    // Registrering med gyldige oplysninger sender brugeren til login-siden
    [Test]
    public async Task Registrering_MedGyldigeOplysninger_Redirecter()
    {
        await Page.GotoAsync($"{BaseUrl}/register.html");

        await Page.FillAsync("#navn", "E2E");
        await Page.FillAsync("#email", NytEmail());
        await Page.FillAsync("#postnr", "2100");
        await Page.FillAsync("#kode", "Test1234!");
        await Page.ClickAsync("button[type=submit]");

        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login.html");
    }

    // Ikke logget ind: viser "log ind for at købe" i stedet for en købsknap
    [Test]
    public async Task IkkeLoggetInd_ViserLoginKnapIStedetForKøb()
    {
        await Page.GotoAsync(BaseUrl);

        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Log ind for at købe" }).First).ToBeVisibleAsync();
    }

    // Valutavælger viser prisen konverteret til den valgte valuta
    [Test]
    public async Task ValutaVaelger_ViserKonverteretPris()
    {
        await Page.GotoAsync(BaseUrl);
        await Expect(Page.Locator(".sko-kort").First).ToBeVisibleAsync();

        await Page.SelectOptionAsync("#valutaVaelger", "EUR");

        await Expect(Page.Locator(".pris-konverteret").First).ToContainTextAsync("EUR");
    }

    // Mærkefilter viser kun sko fra det valgte mærke
    [Test]
    public async Task MaerkeFilter_ViserKunValgtMaerke()
    {
        await Page.GotoAsync(BaseUrl);
        await Expect(Page.Locator(".sko-kort").First).ToBeVisibleAsync();

        await Page.SelectOptionAsync("#maerkeFilter", "Nike");

        var titler = Page.Locator(".sko-kort .card-title");
        await Expect(titler.First).ToContainTextAsync("Nike");
        Assert.That(await titler.AllTextContentsAsync(), Has.All.Contains("Nike"));
    }

    // Happy path: en rigtig Admin logger ind og ser sko-styringen
    [Test]
    public async Task AdminSide_MedAdminRettigheder_ViserSkoStyring()
    {
        await Page.GotoAsync($"{BaseUrl}/login.html");
        await Page.FillAsync("input[type=email]", "test@sneakerstore.dk");
        await Page.FillAsync("input[type=password]", "Test1234!");
        await Page.ClickAsync("button[type=submit]");

        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/admin.html");
        await Expect(Page.Locator("#skoTabel tr").First).ToBeVisibleAsync();
    }
}
