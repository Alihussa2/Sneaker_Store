using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sneaker_Store.Model;
using Sneaker_Store.Services;
using Sneaker_Store.Validation;

namespace Sneaker_Store.Pages;

public class RegisterModel : PageModel
{
    private readonly IKundeRepository _kundeRepository;

    public RegisterModel(IKundeRepository kundeRepository)
    {
        _kundeRepository = kundeRepository;
    }

    [BindProperty] public string Navn { get; set; } = "";
    [BindProperty] public string? Efternavn { get; set; }
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string? Adresse { get; set; }
    [BindProperty] public string? By { get; set; }
    [BindProperty] public int Postnr { get; set; }
    [BindProperty] public string Kode { get; set; } = "";

    public string? FejlBesked { get; set; }
    public string PasswordKrav => PasswordPolicy.Beskrivelse;

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Kode) || string.IsNullOrWhiteSpace(Navn))
        {
            FejlBesked = "Navn, email og kode er påkrævet.";
            return Page();
        }

        if (!PasswordPolicy.ErGyldig(Kode))
        {
            FejlBesked = PasswordPolicy.Beskrivelse;
            return Page();
        }

        if (_kundeRepository.FindByEmail(Email) is not null)
        {
            FejlBesked = "Email er allerede i brug.";
            return Page();
        }

        var kunde = new Kunde(0, Navn, Efternavn ?? "", Email, Adresse ?? "", By ?? "", Postnr, "", false);
        _kundeRepository.AddUser(kunde, Kode);

        return RedirectToPage("Login");
    }
}
