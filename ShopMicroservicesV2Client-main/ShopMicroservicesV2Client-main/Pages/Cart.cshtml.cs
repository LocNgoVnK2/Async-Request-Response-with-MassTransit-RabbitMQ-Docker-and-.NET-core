using System;
using System.Linq;
using System.Threading.Tasks;
using AspnetRunBasics.Models;
using AspnetRunBasics.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspnetRunBasics
{
    [Authorize]
    public class CartModel : PageModel
    {
        private readonly IBasketService _basketService;

        public CartModel(IBasketService basketService)
        {
            _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
        }

        public BasketModel Cart { get; set; } = new BasketModel();

        public async Task<IActionResult> OnGetAsync()
        {
            var userName = "";
            //get username form cokie
            var givenNameClaim = User.Claims.FirstOrDefault(c => c.Type == "given_name");
            if (givenNameClaim != null)
            {
                userName = givenNameClaim.Value;
                // Sử dụng giá trị givenName ở đây
            }
            Cart = await _basketService.GetBasket(userName);

            return Page();
        }

        public async Task<IActionResult> OnPostLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            return Page();
        }
        public async Task<IActionResult> OnPostRemoveToCartAsync(string productId)
        {
            var userName = "";
            //get username form cokie
            var givenNameClaim = User.Claims.FirstOrDefault(c => c.Type == "given_name");
            if (givenNameClaim != null)
            {
                userName = givenNameClaim.Value;
                // Sử dụng giá trị givenName ở đây
            }
            var basket = await _basketService.GetBasket(userName);

            var item = basket.Items.Single(x => x.ProductId == productId);
            basket.Items.Remove(item);

            var basketUpdated = await _basketService.UpdateBasket(basket);

            return RedirectToPage();
        }
    }
}