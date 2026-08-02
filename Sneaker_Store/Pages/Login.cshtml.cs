using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sneaker_Store.Services;

namespace Sneaker_Store.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IKundeRepository _kundeRepository;

        public LoginModel(IKundeRepository kundeRepository)
        {
            _kundeRepository = kundeRepository;
        }

        [BindProperty] public string Email { get; set; } = "";

        [BindProperty] public string Kode { get; set; } = "";

        public string? FejlBesked { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Kode))
            {
                FejlBesked = "Udfyld email og kode.";
                return Page();
            }

            var kunde = _kundeRepository.FindByEmail(Email);
            if (kunde is null || !_kundeRepository.VerifyPassword(kunde, Kode))
            {
                FejlBesked = "Forkert email eller kode.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, kunde.KundeId.ToString()),
                new(ClaimTypes.Email, kunde.Email),
                new(ClaimTypes.Name, $"{kunde.Navn} {kunde.Efternavn}"),
                new(ClaimTypes.Role, kunde.IsAdmin ? "Admin" : "Kunde"),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return kunde.IsAdmin ? RedirectToPage("/adminSideLogin/IndexLoginA") : RedirectToPage("/kundeSideLogin/IndexLoginK");
        }
    }
}
