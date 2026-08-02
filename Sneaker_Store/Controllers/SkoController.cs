using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Sneaker_Store.Controllers;

[ApiController]
[Route("api/sko")]
public class SkoController : ControllerBase
{
    private readonly ISkoRepository _repo;

    public SkoController(ISkoRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public ActionResult<List<Sko>> GetAll()
    {
        return Ok(_repo.GetAll());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Sko> GetById(int id)
    {
        try
        {
            return Ok(_repo.GetById(id));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public ActionResult<Sko> Add([FromBody] Sko sko)
    {
        var created = _repo.Add(sko);
        return CreatedAtAction(nameof(GetById), new { id = created.SkoId }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public ActionResult<Sko> Update(int id, [FromBody] Sko sko)
    {
        try
        {
            return Ok(_repo.Update(id, sko));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public ActionResult<Sko> Delete(int id)
    {
        try
        {
            return Ok(_repo.Delete(id));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
