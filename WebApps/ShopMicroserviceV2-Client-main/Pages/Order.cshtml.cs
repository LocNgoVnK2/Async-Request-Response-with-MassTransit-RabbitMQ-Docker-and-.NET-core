using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspnetRunBasics.Models;
using AspnetRunBasics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspnetRunBasics
{
    [Authorize]
    public class OrderModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ICatalogService _catalogService;
        public OrderModel(IOrderService orderService,ICatalogService catalogService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
        }

        public IEnumerable<OrderResponseModel> Orders { get; set; } = new List<OrderResponseModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            DiscountModel model = await _catalogService.GetDiscount();
            var userName = "";
            //get username form cokie
            var givenNameClaim = User.Claims.FirstOrDefault(c => c.Type == "given_name");
            if (givenNameClaim != null)
            {
                userName = givenNameClaim.Value;
                // Sử dụng giá trị givenName ở đây
            }
            Orders = await _orderService.GetOrdersByUserName(userName);

            return Page();
        }
    }
}