using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sneaker_Store.Model;
using Sneaker_Store.Services;

namespace Sneaker_Store.Pages.KundeSideLogin
{
    public class IndexLoginK : PageModel
    {
        private readonly IKundeRepository _kunder;

        public Kunde? KundeLoggedIn { get; private set; }

        public IndexLoginK(IKundeRepository kunder)
        {
            _kunder = kunder;
        }

        public IActionResult OnGet()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Login");
            }

            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (idClaim is null || !int.TryParse(idClaim, out var kundeId))
            {
                return RedirectToPage("/Login");
            }

            KundeLoggedIn = _kunder.FindById(kundeId);
            if (KundeLoggedIn is null)
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }
    }
}
