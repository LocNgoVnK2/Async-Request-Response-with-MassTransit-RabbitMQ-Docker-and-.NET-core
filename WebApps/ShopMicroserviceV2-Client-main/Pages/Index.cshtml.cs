using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspnetRunBasics.Models;
using AspnetRunBasics.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OpenTelemetry.Trace;

namespace AspnetRunBasics.Pages
{
    
    public class IndexModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly IBasketService _basketService;
   
        public IndexModel(ICatalogService catalogService, IBasketService basketService, TracerProvider provider)
        {
            _catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
       
            _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
       
        }

        public IEnumerable<CatalogModel> ProductList { get; set; } = new List<CatalogModel>();

        public async Task<IActionResult> OnGetAsync()
        {
          
            // await LogTokenAndClaims();
            ProductList = await _catalogService.GetCatalog();
            //DiscountModel model = await _catalogService.GetDiscount();
            return Page();
        }
        /*
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

        public async Task<IActionResult> OnPostAddToCartAsync(string productId)
        {
            //var identityToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
            var product = await _catalogService.GetCatalog(productId);
            var userName = "";
            //get username form cokie
            var givenNameClaim = User.Claims.FirstOrDefault(c => c.Type == "given_name");
            if (givenNameClaim != null)
            {
                 userName = givenNameClaim.Value;
                // Sử dụng giá trị givenName ở đây
            }
           
            var basket = await _basketService.GetBasket(userName);

            basket.Items.Add(new BasketItemModel
            {
                ProductId = productId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = 1,
                Color = "Black"
            });

            var basketUpdated = await _basketService.UpdateBasket(basket);
            return RedirectToPage("Cart");
        }
    }
}
