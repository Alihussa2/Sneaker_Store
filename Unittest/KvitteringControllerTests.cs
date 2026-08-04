using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Sneaker_Store.Controllers;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Unittest;

public class KvitteringControllerTests
{
    private static KvitteringController CreateSut(Mock<IKvitteringRepository> repoMock) => new(repoMock.Object);

    [Test]
    public void GetAll_returns_Ok_with_all_kvitteringer()
    {
        // Arrange
        var kvitteringer = new List<Kvittering>
        {
            new(1, kundeId: 1, antal: 2, totalPris: 1000, beskrivelse: "Nike Air Max x2", koebsdato: DateTime.Now)
        };
        var repoMock = new Mock<IKvitteringRepository>();
        repoMock.Setup(r => r.HentAlleKvitteringer()).Returns(kvitteringer);
        var sut = CreateSut(repoMock);

        // Act
        var result = sut.GetAll();

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = (OkObjectResult)result.Result!;
        var value = ok.Value as IEnumerable<Kvittering>;
        Assert.That(value, Is.Not.Null);
        Assert.That(value!.Count(), Is.EqualTo(1));
    }

    [Test]
    public void GetById_returns_NotFound_when_kvittering_does_not_exist()
    {
        // Arrange
        var repoMock = new Mock<IKvitteringRepository>();
        repoMock.Setup(r => r.HentKvittering(It.IsAny<int>())).Returns((Kvittering?)null);
        var sut = CreateSut(repoMock);

        // Act
        var result = sut.GetById(999);

        // Assert
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public void GetById_returns_Ok_with_kvittering_when_found()
    {
        // Arrange
        var kvittering = new Kvittering(1, kundeId: 1, antal: 2, totalPris: 1000, beskrivelse: "Nike Air Max x2", koebsdato: DateTime.Now);
        var repoMock = new Mock<IKvitteringRepository>();
        repoMock.Setup(r => r.HentKvittering(1)).Returns(kvittering);
        var sut = CreateSut(repoMock);

        // Act
        var result = sut.GetById(1);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = (OkObjectResult)result.Result!;
        Assert.That(ok.Value, Is.EqualTo(kvittering));
    }

    [Test]
    public void Create_returns_CreatedAtAction_with_the_created_kvittering()
    {
        // Arrange
        var kvittering = new Kvittering(1, kundeId: 1, antal: 2, totalPris: 1000, beskrivelse: "Nike Air Max x2", koebsdato: DateTime.Now);
        var repoMock = new Mock<IKvitteringRepository>();
        var sut = CreateSut(repoMock);

        // Act
        var result = sut.Create(kvittering);

        // Assert – comprehensive: statuskode, ActionName og at repository blev kaldt
        Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
        var created = (CreatedAtActionResult)result.Result!;
        Assert.That(created.ActionName, Is.EqualTo(nameof(sut.GetById)));
        Assert.That(created.Value, Is.EqualTo(kvittering));
        repoMock.Verify(r => r.OpretKvittering(kvittering), Times.Once);
    }
}