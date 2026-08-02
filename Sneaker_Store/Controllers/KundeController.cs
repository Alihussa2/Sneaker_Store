using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sneaker_Store.Model;
using Sneaker_Store.Services;
using Sneaker_Store.Validation;

namespace Sneaker_Store.Controllers;

public record RegistrerKundeRequest(string Navn, string Efternavn, string Email, string Adresse, string By, int Postnr, string Kode);

[ApiController]
[Route("api/kunde")]
public class KundeController : ControllerBase
{
    private readonly IKundeRepository _repo;

    public KundeController(IKundeRepository repo)
    {
        _repo = repo;
    }

    [HttpPost("registrer")]
    public IActionResult Registrer([FromBody] RegistrerKundeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Kode))
        {
            return BadRequest("Email og kode er påkrævet.");
        }

        if (!PasswordPolicy.ErGyldig(request.Kode))
        {
            return BadRequest(PasswordPolicy.Beskrivelse);
        }

        if (_repo.FindByEmail(request.Email) is not null)
        {
            return Conflict("Email er allerede i brug.");
        }

        var kunde = new Kunde(0, request.Navn, request.Efternavn, request.Email, request.Adresse, request.By, request.Postnr, "", false);
        _repo.AddUser(kunde, request.Kode);

        return CreatedAtAction(nameof(Registrer), new { kunde.KundeId, kunde.Email, kunde.Navn, kunde.Efternavn });
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetById(int id)
    {
        var kunde = _repo.FindById(id);
        if (kunde is null)
        {
            return NotFound();
        }

        return Ok(new { kunde.KundeId, kunde.Navn, kunde.Efternavn, kunde.Email, kunde.Adresse, kunde.By, kunde.Postnr, kunde.IsAdmin });
    }
}
