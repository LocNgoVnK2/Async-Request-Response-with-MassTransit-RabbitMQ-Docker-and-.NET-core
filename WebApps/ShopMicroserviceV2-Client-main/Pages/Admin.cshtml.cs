using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspnetRunBasics.Pages
{
    [Authorize(Roles ="admin")]
    public class AdminModel : PageModel
    {
        public void OnGet()
        {
            
        }
        /*
            private readonly IDiscountService _discountService;

        public AdminModel(IDiscountService discountService)
        {
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LogTokenAndClaims();
            DiscountModel model = await _discountService.GetDiscount("IPhone X");
            return Page();
        }
        public async Task LogTokenAndClaims()
        {
            var identityToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);

            Debug.WriteLine($"Identity token: {identityToken}");

            foreach (var claim in User.Claims)
            {
                Debug.WriteLine($"Claim type: {claim.Type} - Claim value: {claim.Value}");
            }
        }

         */
    }
}
