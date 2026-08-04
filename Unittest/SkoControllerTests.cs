using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Sneaker_Store.Controllers;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Unittest;

public class SkoControllerTests
{
    private static SkoController CreateSut(Mock<ISkoRepository> repoMock) => new(repoMock.Object);

    [Test]
    public void GetAll_returns_Ok_with_all_shoes()
    {
        // Arrange
        var skoListe = new List<Sko> { new(1, "Nike", "Air Max", 42, 999) };
        var repoMock = new Mock<ISkoRepository>();
        repoMock.Setup(r => r.GetAll()).Returns(skoListe);
        var sut = CreateSut(repoMock);

        // Act
        var result = sut.GetAll();

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        var value = okResult.Value as List<Sko>;
        Assert.NotNull(value);
        Assert.That(value!.Count, Is.EqualTo(1));
    }

    [Test]
    public void GetById_returns_NotFound_when_sko_does_not_exist()
    {
        // Arrange
        var repoMock = new Mock<ISkoRepository>();
        repoMock.Setup(r => r.GetById(It.IsAny<int>())).Throws<KeyNotFoundException>();
        var sut = CreateSut(repoMock);

        // Act
        var result = sut.GetById(999);

        // Assert
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    // Boundary value analysis: lagerantal 0, negativt tal og et normalt positivt tal
    [TestCase(0)]
    [TestCase(-5)]
    [TestCase(10)]
    public void Add_creates_shoe_regardless_of_stock_amount(int lagerAntal)
    {
        // Arrange
        var nySko = new Sko(0, "Nike", "Air Max", 42, 999, lagerAntal);
        var repoMock = new Mock<ISkoRepository>();
        repoMock.Setup(r => r.Add(It.IsAny<Sko>())).Returns((Sko s) => { s.SkoId = 1; return s; });
        var sut = CreateSut(repoMock);

        // Act
        var result = sut.Add(nySko);

        // Assert
        Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
        var created = (CreatedAtActionResult)result.Result!;
        var value = created.Value as Sko;
        Assert.NotNull(value);
        Assert.That(value!.LagerAntal, Is.EqualTo(lagerAntal));
        repoMock.Verify(r => r.Add(nySko), Times.Once);
    }

    [Test]
    public void Update_returns_BadRequest_when_id_does_not_match()
    {
        // Arrange
        var repoMock = new Mock<ISkoRepository>();
        repoMock.Setup(r => r.Update(It.IsAny<int>(), It.IsAny<Sko>()))
            .Throws(new ArgumentException("kan ikke opdatere id og obj.Id er forskellige"));
        var sut = CreateSut(repoMock);
        var sko = new Sko(2, "Nike", "Air Max", 42, 999);

        // Act
        var result = sut.Update(1, sko);

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        var badRequest = (BadRequestObjectResult)result.Result!;
        Assert.That(badRequest.Value, Is.EqualTo("kan ikke opdatere id og obj.Id er forskellige"));
    }

    [Test]
    public void Update_returns_NotFound_when_sko_does_not_exist()
    {
        // Arrange
        var repoMock = new Mock<ISkoRepository>();
        repoMock.Setup(r => r.Update(It.IsAny<int>(), It.IsAny<Sko>())).Throws<KeyNotFoundException>();
        var sut = CreateSut(repoMock);
        var sko = new Sko(1, "Nike", "Air Max", 42, 999);

        // Act
        var result = sut.Update(1, sko);

        // Assert
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public void Delete_returns_Ok_with_deleted_shoe_when_found()
    {
        // Arrange
        var sko = new Sko(1, "Nike", "Air Max", 42, 999);
        var repoMock = new Mock<ISkoRepository>();
        repoMock.Setup(r => r.Delete(1)).Returns(sko);
        var sut = CreateSut(repoMock);

        // Act
        var result = sut.Delete(1);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = (OkObjectResult)result.Result!;
        Assert.That(ok.Value, Is.EqualTo(sko));
    }

    [Test]
    public void Delete_returns_NotFound_when_sko_does_not_exist()
    {
        // Arrange
        var repoMock = new Mock<ISkoRepository>();
        repoMock.Setup(r => r.Delete(It.IsAny<int>())).Throws<KeyNotFoundException>();
        var sut = CreateSut(repoMock);

        // Act
        var result = sut.Delete(999);

        // Assert
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }
}