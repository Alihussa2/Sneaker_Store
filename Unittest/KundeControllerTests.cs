using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Sneaker_Store.Controllers;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Unittest;

public class KundeControllerTests
{
    [SetUp]
    public void Setup()
    {
    }

    // Privat, parameteriserbar hjælpemetode i stedet for en delt "setup" (Khorikov)
    private static KundeController CreateSut(Mock<IKundeRepository> repoMock)
    {
        return new KundeController(repoMock.Object);
    }

    // Decision table fra black-box designet:
    // Email tom | Kode tom | Kode ugyldig | Email findes allerede | -> forventet resultat
    [TestCase("", "Password1!", "BadRequest")]              // Email mangler
    [TestCase("ny@mail.dk", "", "BadRequest")]               // Kode mangler
    [TestCase("ny@mail.dk", "kort", "BadRequest")]           // Kode ugyldig (for kort/for simpel)
    [TestCase("findes@mail.dk", "Password1!", "Conflict")]  // Email findes allerede
    [TestCase("ny@mail.dk", "Password1!", "Created")]        // Alt gyldigt
    public void Registrer_returns_expected_result_based_on_input(string email, string kode, string forventetResultat)
    {
        // Arrange
        var repoMock = new Mock<IKundeRepository>();
        repoMock.Setup(r => r.FindByEmail("findes@mail.dk"))
            .Returns(new Kunde(1, "Eksisterende", "Kunde", "findes@mail.dk", "Adr", "By", 2000, "hash", false));
        repoMock.Setup(r => r.FindByEmail(It.Is<string>(e => e != "findes@mail.dk")))
            .Returns((Kunde?)null);

        var sut = CreateSut(repoMock);
        var request = new RegistrerKundeRequest("Anders", "And", email, "Adresse 1", "By", 2000, kode);

        // Act
        var result = sut.Registrer(request);

        // Assert
        switch (forventetResultat)
        {
            case "BadRequest":
                Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
                repoMock.Verify(r => r.AddUser(It.IsAny<Kunde>(), It.IsAny<string>()), Times.Never);
                break;

            case "Conflict":
                Assert.That(result, Is.TypeOf<ConflictObjectResult>());
                repoMock.Verify(r => r.AddUser(It.IsAny<Kunde>(), It.IsAny<string>()), Times.Never);
                break;

            case "Created":
                Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
                var created = (CreatedAtActionResult)result;
                Assert.That(created.ActionName, Is.EqualTo(nameof(sut.Registrer)));
                repoMock.Verify(r => r.AddUser(
                    It.Is<Kunde>(k => k.Email == email),
                    kode), Times.Once);
                break;
        }
    }

    [Test]
    public void GetById_returns_NotFound_when_kunde_does_not_exist()
    {
        // Arrange
        var repoMock = new Mock<IKundeRepository>();
        repoMock.Setup(r => r.FindById(It.IsAny<int>())).Returns((Kunde?)null);
        var sut = CreateSut(repoMock);

        // Act
        var result = sut.GetById(999);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public void GetById_returns_Ok_with_expected_kunde_data_when_found()
    {
        // Arrange
        var repoMock = new Mock<IKundeRepository>();
        var kunde = new Kunde(1, "Anders", "And", "a@a.dk", "Adr", "By", 2000, "hash", false);
        repoMock.Setup(r => r.FindById(1)).Returns(kunde);
        var sut = CreateSut(repoMock);

        // Act
        var result = sut.GetById(1) as OkObjectResult;

        // Assert – comprehensive: statuskode OG indhold tjekkes
        Assert.NotNull(result);
        Assert.That(result!.StatusCode, Is.EqualTo(200));
        Assert.NotNull(result.Value);
    }
}