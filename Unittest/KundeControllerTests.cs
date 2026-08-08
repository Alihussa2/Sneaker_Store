using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Sneaker_Store.Controllers;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Unittest;

// Unit test: IKundeRepository mockes -> relaterer til spg. 19 (mocking af unmanaged deps: her er repo IKKE ekstern/unmanaged, så mocking er standard-praksis)
public class KundeControllerTests
{
    // Hjælpemetode til at oprette SUT (system under test)
    private static KundeController CreateSut(Mock<IKundeRepository> repoMock)
    {
        return new KundeController(repoMock.Object);
    }

    // BLACK-BOX: decision table (email tom/email ugyldig format/kode tom/kode ugyldig/email findes -> resultat)
    // PARAMETRISERET: [TestCase] x6 - hver case isolerer én betingelse
    [TestCase("", "Password1!", "BadRequest")]                // Email mangler
    [TestCase("annamail.dk", "Password1!", "BadRequest")]     // TC07: Email mangler "@" (jf. black-box-dokumentet)
    [TestCase("ny@mail.dk", "", "BadRequest")]                 // Kode mangler
    [TestCase("ny@mail.dk", "kort", "BadRequest")]             // Kode ugyldig
    [TestCase("findes@mail.dk", "Password1!", "Conflict")]    // Email findes allerede
    [TestCase("ny@mail.dk", "Password1!", "Created")]          // Alt gyldigt
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

        // Assert - comprehensive: statuskode + Verify på om repository blev kaldt korrekt (spg. 17: classical approach, kun repo som test double)
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

    // IKKE parametriseret: enkeltstående negativ case, black-box: "kunde findes ikke"
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

    // IKKE parametriseret: enkeltstående positiv case, black-box: "kunde findes"
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

        // Assert – comprehensive: statuskode OG faktisk indhold, ikke kun "no exception"
        Assert.NotNull(result);
        Assert.That(result!.StatusCode, Is.EqualTo(200));
        Assert.NotNull(result.Value);
    }
}