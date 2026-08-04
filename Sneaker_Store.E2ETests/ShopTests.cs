using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Sneaker_Store.E2ETests;

// Kør appen lokalt (dotnet run) på http://localhost:5083, før disse tests køres.
[TestFixture]
public class ShopTests : PageTest
{
    private const string BaseUrl = "http://localhost:5083";

    private static string NytEmail() => $"e2e{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}@sneakerstore.dk";

    [Test]
    public async Task Forside_ViserOverskriftOgSko()
    {
        await Page.GotoAsync(BaseUrl);

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Sneaker Store" })).ToBeVisibleAsync();
        await Expect(Page.Locator(".sko-kort").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_MedGyldigeOplysninger_Redirecter()
    {
        await Page.GotoAsync($"{BaseUrl}/login.html");

        await Page.FillAsync("input[type=email]", "test@sneakerstore.dk");
        await Page.FillAsync("input[type=password]", "Test1234!");
        await Page.ClickAsync("button[type=submit]");

        await Expect(Page).Not.ToHaveURLAsync($"{BaseUrl}/login.html");
    }

    [Test]
    public async Task Login_MedForkertKode_ViserFejlbesked()
    {
        await Page.GotoAsync($"{BaseUrl}/login.html");

        await Page.FillAsync("input[type=email]", "test@sneakerstore.dk");
        await Page.FillAsync("input[type=password]", "forkertKode123!");
        await Page.ClickAsync("button[type=submit]");

        await Expect(Page.GetByText("Forkert email eller kode.")).ToBeVisibleAsync();
    }

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

    [Test]
    public async Task Registrering_MedGyldigeOplysninger_Redirecter()
    {
        await Page.GotoAsync($"{BaseUrl}/register.html");

        await Page.FillAsync("#navn", "E2E");
        await Page.FillAsync("#email", NytEmail());
        await Page.FillAsync("#kode", "Test1234!");
        await Page.ClickAsync("button[type=submit]");

        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login.html");
    }

    [Test]
    public async Task IkkeLoggetInd_ViserLoginKnapIStedetForKøb()
    {
        await Page.GotoAsync(BaseUrl);

        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Log ind for at købe" }).First).ToBeVisibleAsync();
    }

    [Test]
    public async Task ValutaVaelger_ViserKonverteretPris()
    {
        await Page.GotoAsync(BaseUrl);
        await Expect(Page.Locator(".sko-kort").First).ToBeVisibleAsync();

        await Page.SelectOptionAsync("#valutaVaelger", "EUR");

        await Expect(Page.Locator(".pris-konverteret").First).ToContainTextAsync("EUR");
    }

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

    [Test]
    public async Task AdminSide_UdenAdminRettigheder_Redirecter()
    {
        var email = NytEmail();

        await Page.GotoAsync($"{BaseUrl}/register.html");
        await Page.FillAsync("#navn", "Almindelig");
        await Page.FillAsync("#email", email);
        await Page.FillAsync("#kode", "Test1234!");
        await Page.ClickAsync("button[type=submit]");

        await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login.html");
        await Page.FillAsync("input[type=email]", email);
        await Page.FillAsync("input[type=password]", "Test1234!");
        await Page.ClickAsync("button[type=submit]");

        await Page.GotoAsync($"{BaseUrl}/admin.html");

        await Expect(Page).Not.ToHaveURLAsync($"{BaseUrl}/admin.html");
    }
}
