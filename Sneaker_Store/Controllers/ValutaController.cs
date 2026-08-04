using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace Sneaker_Store.Controllers;

public record ValutaSvar(double BelobDkk, string TilValuta, double Kurs, double Konverteret);

file record FrankfurterSvar(double Amount, string Base, string Date, Dictionary<string, double> Rates);

[ApiController]
[Route("api/valuta")]
public class ValutaController : ControllerBase
{
    private static readonly HashSet<string> TilladteValutaer = new(StringComparer.OrdinalIgnoreCase)
    {
        "EUR", "USD", "GBP", "SEK", "NOK"
    };

    private readonly IHttpClientFactory _httpClientFactory;

    public ValutaController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("konverter")]
    public async Task<ActionResult<ValutaSvar>> Konverter([FromQuery] double belobDkk, [FromQuery] string til = "EUR")
    {
        if (belobDkk < 0)
        {
            return BadRequest("Beløb kan ikke være negativt.");
        }

        if (!TilladteValutaer.Contains(til))
        {
            return BadRequest($"Valuta skal være en af: {string.Join(", ", TilladteValutaer)}");
        }

        var client = _httpClientFactory.CreateClient("Frankfurter");
        try
        {
            var response = await client.GetFromJsonAsync<FrankfurterSvar>(
                $"latest?amount={belobDkk.ToString(System.Globalization.CultureInfo.InvariantCulture)}&from=DKK&to={til}");

            if (response is null || !response.Rates.TryGetValue(til.ToUpperInvariant(), out var konverteret))
            {
                return StatusCode(502, "Kunne ikke hente valutakurs.");
            }

            var kurs = belobDkk == 0 ? 0 : konverteret / belobDkk;
            return Ok(new ValutaSvar(belobDkk, til.ToUpperInvariant(), kurs, konverteret));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return StatusCode(502, "Ekstern valuta-API kunne ikke kontaktes.");
        }
    }
}
