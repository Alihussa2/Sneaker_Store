using NUnit.Framework;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Sneaker_Store.Controllers;

namespace Unittest;

public class ValutaControllerTests
{
    // Privat hjælpemetode: bygger en HttpClient hvor det udgående kald er mocket,
    // så testen ikke rammer det rigtige Frankfurter-API (ekstern dependency skal mockes).
    // BaseAddress SKAL sættes, fordi controlleren bruger en relativ URL ("latest?...")
    // og HttpClient kan ikke sende en relativ URL uden en BaseAddress at kombinere med.
    private static IHttpClientFactory CreateFactoryReturning(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var client = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.frankfurter.app/") // matcher Program.cs
        };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("Frankfurter")).Returns(client);
        return factoryMock.Object;
    }

    [Test]
    public async Task Konverter_returns_BadRequest_when_belob_is_negative()
    {
        // Boundary value analysis: -1 er lige under den gyldige grænse (0)
        var factoryMock = new Mock<IHttpClientFactory>();
        var sut = new ValutaController(factoryMock.Object);

        // Act
        var result = await sut.Konverter(-1, "EUR");

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    // Equivalence partitioning: gyldig valuta-klasse vs. ugyldig valuta-klasse
    [TestCase("EUR", true)]
    [TestCase("USD", true)]
    [TestCase("JPY", false)]
    [TestCase("", false)]
    public async Task Konverter_validates_currency_correctly(string valuta, bool erGyldig)
    {
        // Arrange
        // Content-Type skal være "application/json", ellers kan GetFromJsonAsync ikke deserialisere svaret
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
            BaseAddress = new Uri("https://api.frankfurter.app/")
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

        // Assert – comprehensive: alle felter i svaret tjekkes
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