using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Sneaker_Store.Controllers;

[ApiController]
[Route("api/kvittering")]
[Authorize]
public class KvitteringController : ControllerBase
{
    private readonly IKvitteringRepository _repo;

    public KvitteringController(IKvitteringRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public ActionResult<IEnumerable<Kvittering>> GetAll()
    {
        return Ok(_repo.HentAlleKvitteringer());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Kvittering> GetById(int id)
    {
        var kvittering = _repo.HentKvittering(id);
        return kvittering is null ? NotFound() : Ok(kvittering);
    }

    [HttpPost]
    public ActionResult<Kvittering> Create([FromBody] Kvittering kvittering)
    {
        _repo.OpretKvittering(kvittering);
        return CreatedAtAction(nameof(GetById), new { id = kvittering.Id }, kvittering);
    }
}
