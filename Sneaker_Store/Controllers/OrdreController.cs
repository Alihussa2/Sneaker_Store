using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Sneaker_Store.Controllers;

[ApiController]
[Route("api/ordre")]
[Authorize]
public class OrdreController : ControllerBase
{
    private readonly IOrdreRepository _ordreRepo;
    private readonly ISkoRepository _skoRepo;
    private readonly IKvitteringRepository _kvitteringRepo;

    public OrdreController(IOrdreRepository ordreRepo, ISkoRepository skoRepo, IKvitteringRepository kvitteringRepo)
    {
        _ordreRepo = ordreRepo;
        _skoRepo = skoRepo;
        _kvitteringRepo = kvitteringRepo;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public ActionResult<IEnumerable<Ordre>> GetAll()
    {
        return Ok(_ordreRepo.HentAlleOrdrer());
    }

    [HttpGet("mine")]
    public ActionResult<IEnumerable<Ordre>> GetMine()
    {
        var kundeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(_ordreRepo.HentAlleOrdrer().Where(o => o.KundeId.ToString() == kundeId));
    }

    [HttpGet("{id:int}")]
    public ActionResult<Ordre> GetById(int id)
    {
        var ordre = _ordreRepo.FindOrdre(id);
        if (ordre is null)
        {
            return NotFound();
        }

        if (!User.IsInRole("Admin") && ordre.KundeId.ToString() != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Forbid();
        }

        return Ok(ordre);
    }

    [HttpPost]
    public ActionResult<Ordre> Create([FromBody] Ordre ordre)
    {
        var kundeIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (kundeIdClaim is null || !int.TryParse(kundeIdClaim, out var kundeId))
        {
            return Unauthorized();
        }

        if (ordre.Antal <= 0)
        {
            return BadRequest("Antal skal være mindst 1.");
        }

        Sko sko;
        try
        {
            sko = _skoRepo.GetById(ordre.SkoId);
        }
        catch (KeyNotFoundException)
        {
            return BadRequest($"Sko med id {ordre.SkoId} findes ikke.");
        }

        try
        {
            _skoRepo.ReducerLager(ordre.SkoId, ordre.Antal);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        ordre.KundeId = kundeId;
        ordre.TotalPris = sko.Pris * ordre.Antal;
        _ordreRepo.TilføjOrdre(ordre);

        _kvitteringRepo.OpretKvittering(new Kvittering(
            id: 0,
            kundeId: kundeId,
            antal: ordre.Antal,
            totalPris: ordre.TotalPris,
            beskrivelse: $"{sko.Maerke} {sko.Model} (str. {sko.Str}) x{ordre.Antal}",
            koebsdato: DateTime.Now));

        return CreatedAtAction(nameof(GetById), new { id = ordre.OrdreId }, ordre);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        _ordreRepo.SletOrdre(id);
        return NoContent();
    }
}
