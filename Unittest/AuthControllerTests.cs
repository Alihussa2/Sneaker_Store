using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sneaker_Store.Controllers;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Sneaker_Store.UnitTests;

public class AuthControllerTests
{
    private Mock<IKundeRepository> _kundeRepo = null!;
    private Mock<IAuthenticationService> _authService = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _kundeRepo = new Mock<IKundeRepository>();
        _authService = new Mock<IAuthenticationService>();

        var services = new ServiceCollection();
        services.AddSingleton(_authService.Object);
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        _controller = new AuthController(_kundeRepo.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [Test]
    public async Task Login_ManglerEmail_ReturnererBadRequest()
    {
        var request = new LoginRequest("", "Abcdef1!");
        var result = await _controller.Login(request);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Login_KundeFindesIkke_ReturnererUnauthorized()
    {
        _kundeRepo.Setup(r => r.FindByEmail("ukendt@test.dk")).Returns((Kunde?)null);
        var request = new LoginRequest("ukendt@test.dk", "Abcdef1!");

        var result = await _controller.Login(request);

        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Login_ForkertPassword_ReturnererUnauthorized()
    {
        var kunde = new Kunde(1, "Test", "Testesen", "test@test.dk", "", "", 0, "hash", false);
        _kundeRepo.Setup(r => r.FindByEmail("test@test.dk")).Returns(kunde);
        _kundeRepo.Setup(r => r.VerifyPassword(kunde, "forkert")).Returns(false);
        var request = new LoginRequest("test@test.dk", "forkert");

        var result = await _controller.Login(request);

        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Login_KorrektLogin_SignerIndOgReturnererOk()
    {
        var kunde = new Kunde(1, "Test", "Testesen", "test@test.dk", "", "", 0, "hash", false);
        _kundeRepo.Setup(r => r.FindByEmail("test@test.dk")).Returns(kunde);
        _kundeRepo.Setup(r => r.VerifyPassword(kunde, "Abcdef1!")).Returns(true);
        var request = new LoginRequest("test@test.dk", "Abcdef1!");

        var result = await _controller.Login(request);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        _authService.Verify(a => a.SignInAsync(
            It.IsAny<HttpContext>(),
            Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<AuthenticationProperties?>()),
            Times.Once);
    }
}