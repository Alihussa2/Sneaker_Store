using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Sneaker_Store.E2ETests;

// Kør appen lokalt (dotnet run) på http://localhost:5083, før disse tests køres.
[TestFixture]
public class ShopTests : PageTest
{
    private const string BaseUrl = "http://localhost:5083";

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
        await Page.GotoAsync($"{BaseUrl}/Login");

        await Page.FillAsync("input[type=email]", "test@sneakerstore.dk");
        await Page.FillAsync("input[type=password]", "Test1234!");
        await Page.ClickAsync("button[type=submit]");

        await Expect(Page).Not.ToHaveURLAsync($"{BaseUrl}/Login");
    }

    [Test]
    public async Task Login_MedForkertKode_ViserFejlbesked()
    {
        await Page.GotoAsync($"{BaseUrl}/Login");

        await Page.FillAsync("input[type=email]", "test@sneakerstore.dk");
        await Page.FillAsync("input[type=password]", "forkertKode123!");
        await Page.ClickAsync("button[type=submit]");

        await Expect(Page.GetByText("Forkert email eller kode.")).ToBeVisibleAsync();
    }
}
