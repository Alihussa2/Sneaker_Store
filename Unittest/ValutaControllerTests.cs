using NUnit.Framework;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Sneaker_Store.Controllers;

namespace Unittest;

// Unit test: HttpClient mockes (via HttpMessageHandler) i stedet for at ramme det rigtige eksterne Frankfurter-API
// -> spg. 19: ekstern/unmanaged dependency mockes bevidst for hurtige, stabile tests
public class ValutaControllerTests
{
    // Hjælpemetode: bygger en HttpClient med mocket SendAsync + BaseAddress (nødvendig pga. relativ URL i controlleren)
    private static IHttpClientFactory CreateFactoryReturning(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var client = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.frankfurter.dev/v1/") // matcher Program.cs
        };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("Frankfurter")).Returns(client);
        return factoryMock.Object;
    }

    // IKKE parametriseret. BLACK-BOX: boundary value analysis - beløb=-1 er lige under grænsen (0)
    [Test]
    public async Task Konverter_returns_BadRequest_when_belob_is_negative()
    {
        var factoryMock = new Mock<IHttpClientFactory>();
        var sut = new ValutaController(factoryMock.Object);

        // Act
        var result = await sut.Konverter(-1, "EUR");

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    // BLACK-BOX: equivalence partitioning (gyldig valuta-klasse vs. ugyldig valuta-klasse)
    // PARAMETRISERET: [TestCase] x4
    [TestCase("EUR", true)]
    [TestCase("USD", true)]
    [TestCase("JPY", false)]
    [TestCase("", false)]
    public async Task Konverter_validates_currency_correctly(string valuta, bool erGyldig)
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"amount\":100,\"base\":\"DKK\",\"date\":\"2026-01-01\",\"rates\":{\"" + valuta + "\":13.4}}",
                Encoding.UTF8,
                "application/json")
        };
        var factory = CreateFactoryReturning(response);
        var sut = new ValutaController(factory);

        // Act
        var result = await sut.Konverter(100, valuta);

        // Assert
        if (erGyldig)
        {
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var ok = (OkObjectResult)result.Result!;
            Assert.NotNull(ok.Value);
        }
        else
        {
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }
    }

    // IKKE parametriseret: enkelt negativ case, black-box: "eksternt API kan ikke nås"
    [Test]
    public async Task Konverter_returns_502_when_external_api_is_unreachable()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());
        var client = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.frankfurter.dev/v1/")
        };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("Frankfurter")).Returns(client);
        var sut = new ValutaController(factoryMock.Object);

        // Act
        var result = await sut.Konverter(100, "EUR");

        // Assert
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(502));
    }

    // IKKE parametriseret: enkelt positiv case, black-box: "gyldig konvertering"
    [Test]
    public async Task Konverter_returns_correct_conversion_values()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"amount\":100,\"base\":\"DKK\",\"date\":\"2026-01-01\",\"rates\":{\"EUR\":13.4}}",
                Encoding.UTF8,
                "application/json")
        };
        var factory = CreateFactoryReturning(response);
        var sut = new ValutaController(factory);

        // Act
        var result = await sut.Konverter(100, "EUR");

        // Assert – comprehensive: alle 4 felter i svaret tjekkes, ikke kun statuskode
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = (OkObjectResult)result.Result!;
        var svar = ok.Value as ValutaSvar;
        Assert.NotNull(svar);
        Assert.That(svar!.BelobDkk, Is.EqualTo(100));
        Assert.That(svar.TilValuta, Is.EqualTo("EUR"));
        Assert.That(svar.Konverteret, Is.EqualTo(13.4));
        Assert.That(svar.Kurs, Is.EqualTo(0.134).Within(0.001));
    }
}