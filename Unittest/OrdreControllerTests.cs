using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Sneaker_Store.Controllers;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Unittest;

public class OrdreControllerTests
{
    // Privat, parameteriserbar hjælpemetode: opretter controlleren med en "logget ind" bruger
    private static OrdreController CreateSut(
        Mock<IOrdreRepository> ordreRepoMock,
        Mock<ISkoRepository> skoRepoMock,
        Mock<IKvitteringRepository> kvitteringRepoMock,
        int kundeId,
        bool isAdmin = false)
    {
        var sut = new OrdreController(ordreRepoMock.Object, skoRepoMock.Object, kvitteringRepoMock.Object);

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, kundeId.ToString()) };
        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return sut;
    }

    [Test]
    public void Create_returns_BadRequest_when_antal_is_zero()
    {
        // Boundary value analysis: antal = 0 er den nedre grænse og skal fejle
        var ordreRepoMock = new Mock<IOrdreRepository>();
        var skoRepoMock = new Mock<ISkoRepository>();
        var kvitteringRepoMock = new Mock<IKvitteringRepository>();
        var sut = CreateSut(ordreRepoMock, skoRepoMock, kvitteringRepoMock, kundeId: 1);
        var ordre = new Ordre(0, 0, 1, 0, 0);

        // Act
        var result = sut.Create(ordre);

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        var badRequest = (BadRequestObjectResult)result.Result!;
        Assert.That(badRequest.Value, Is.EqualTo("Antal skal være mindst 1."));
    }

    [Test]
    public void Create_returns_BadRequest_when_shoe_does_not_exist()
    {
        // Arrange
        var ordreRepoMock = new Mock<IOrdreRepository>();
        var skoRepoMock = new Mock<ISkoRepository>();
        skoRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Throws<KeyNotFoundException>();
        var kvitteringRepoMock = new Mock<IKvitteringRepository>();
        var sut = CreateSut(ordreRepoMock, skoRepoMock, kvitteringRepoMock, kundeId: 1);
        var ordre = new Ordre(0, 0, 999, 1, 0);

        // Act
        var result = sut.Create(ordre);

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        kvitteringRepoMock.Verify(r => r.OpretKvittering(It.IsAny<Kvittering>()), Times.Never);
    }

    [Test]
    public void Create_returns_BadRequest_when_not_enough_stock()
    {
        // Arrange
        var ordreRepoMock = new Mock<IOrdreRepository>();
        var skoRepoMock = new Mock<ISkoRepository>();
        skoRepoMock.Setup(r => r.GetById(1)).Returns(new Sko(1, "Nike", "Air Max", 42, 999, lagerAntal: 2));
        skoRepoMock.Setup(r => r.ReducerLager(1, 5)).Throws(new InvalidOperationException("Kun 2 stk. på lager."));
        var kvitteringRepoMock = new Mock<IKvitteringRepository>();
        var sut = CreateSut(ordreRepoMock, skoRepoMock, kvitteringRepoMock, kundeId: 1);
        var ordre = new Ordre(0, 0, 1, 5, 0);

        // Act
        var result = sut.Create(ordre);

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        var badRequest = (BadRequestObjectResult)result.Result!;
        Assert.That(badRequest.Value, Is.EqualTo("Kun 2 stk. på lager."));
    }

    [Test]
    public void Create_returns_Created_and_generates_kvittering_when_order_is_valid()
    {
        // Arrange
        var ordreRepoMock = new Mock<IOrdreRepository>();
        var skoRepoMock = new Mock<ISkoRepository>();
        var sko = new Sko(1, "Nike", "Air Max", 42, 500, lagerAntal: 10);
        skoRepoMock.Setup(r => r.GetById(1)).Returns(sko);
        skoRepoMock.Setup(r => r.ReducerLager(1, 2)).Returns(sko);
        var kvitteringRepoMock = new Mock<IKvitteringRepository>();
        var sut = CreateSut(ordreRepoMock, skoRepoMock, kvitteringRepoMock, kundeId: 42);
        var ordre = new Ordre(0, 0, 1, 2, 0);

        // Act
        var result = sut.Create(ordre);

        // Assert – comprehensive: statuskode, beregnet totalpris og at kvittering blev oprettet
        Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
        var created = (CreatedAtActionResult)result.Result!;
        var createdOrdre = created.Value as Ordre;
        Assert.NotNull(createdOrdre);
        Assert.That(createdOrdre!.KundeId, Is.EqualTo(42));
        Assert.That(createdOrdre.TotalPris, Is.EqualTo(1000)); // 500 * 2
        ordreRepoMock.Verify(r => r.TilføjOrdre(It.IsAny<Ordre>()), Times.Once);
        kvitteringRepoMock.Verify(r => r.OpretKvittering(
            It.Is<Kvittering>(k => k.KundeId == 42 && k.Antal == 2)), Times.Once);
    }

    [Test]
    public void GetById_returns_Forbid_when_ordre_belongs_to_another_kunde()
    {
        // Arrange
        var ordreRepoMock = new Mock<IOrdreRepository>();
        ordreRepoMock.Setup(r => r.FindOrdre(1)).Returns(new Ordre(1, kundeId: 99, skoId: 1, antal: 1, totalPris: 500));
        var skoRepoMock = new Mock<ISkoRepository>();
        var kvitteringRepoMock = new Mock<IKvitteringRepository>();
        var sut = CreateSut(ordreRepoMock, skoRepoMock, kvitteringRepoMock, kundeId: 1, isAdmin: false);

        // Act
        var result = sut.GetById(1);

        // Assert
        Assert.That(result.Result, Is.TypeOf<ForbidResult>());
    }

    [Test]
    public void GetById_returns_Ok_when_admin_views_any_order()
    {
        // Arrange
        var ordreRepoMock = new Mock<IOrdreRepository>();
        ordreRepoMock.Setup(r => r.FindOrdre(1)).Returns(new Ordre(1, kundeId: 99, skoId: 1, antal: 1, totalPris: 500));
        var skoRepoMock = new Mock<ISkoRepository>();
        var kvitteringRepoMock = new Mock<IKvitteringRepository>();
        var sut = CreateSut(ordreRepoMock, skoRepoMock, kvitteringRepoMock, kundeId: 1, isAdmin: true);

        // Act
        var result = sut.GetById(1);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public void GetById_returns_NotFound_when_ordre_does_not_exist()
    {
        // Arrange
        var ordreRepoMock = new Mock<IOrdreRepository>();
        ordreRepoMock.Setup(r => r.FindOrdre(It.IsAny<int>())).Returns((Ordre?)null);
        var skoRepoMock = new Mock<ISkoRepository>();
        var kvitteringRepoMock = new Mock<IKvitteringRepository>();
        var sut = CreateSut(ordreRepoMock, skoRepoMock, kvitteringRepoMock, kundeId: 1);

        // Act
        var result = sut.GetById(999);

        // Assert
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public void GetMine_returns_only_orders_belonging_to_logged_in_kunde()
    {
        // Arrange
        var ordreRepoMock = new Mock<IOrdreRepository>();
        ordreRepoMock.Setup(r => r.HentAlleOrdrer()).Returns(new List<Ordre>
        {
            new(1, kundeId: 1, skoId: 1, antal: 1, totalPris: 500),
            new(2, kundeId: 2, skoId: 1, antal: 1, totalPris: 500)
        });
        var skoRepoMock = new Mock<ISkoRepository>();
        var kvitteringRepoMock = new Mock<IKvitteringRepository>();
        var sut = CreateSut(ordreRepoMock, skoRepoMock, kvitteringRepoMock, kundeId: 1);

        // Act
        var result = sut.GetMine();

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = (OkObjectResult)result.Result!;
        var ordrer = (ok.Value as IEnumerable<Ordre>)!.ToList();
        Assert.That(ordrer.Count, Is.EqualTo(1));
        Assert.That(ordrer.All(o => o.KundeId == 1), Is.True);
    }
}