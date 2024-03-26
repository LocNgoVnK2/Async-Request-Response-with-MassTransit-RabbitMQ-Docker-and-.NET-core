using Discount.API.Entities;
using Discount.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Discount.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize("ClientIdPolicy")]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountRepository _repository;
        private readonly ILogger<DiscountController> _logger;
        public DiscountController(IDiscountRepository repository, ILogger<DiscountController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{productName}", Name = "GetDiscount")]
        [ProducesResponseType(typeof(Coupon), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Coupon>> GetDiscount(string productName)
        {
            Guid eventId = Guid.NewGuid();
            _logger.LogInformation("Start Controller {controller} Action {action}  with {eventId}, productName={productName}"
                , nameof(DiscountController), nameof(GetDiscount), eventId, productName);
            try
            {
                var discount = await _repository.GetDiscount(productName);

                if (discount == null)
                {
                    _logger.LogInformation("Controller {controller} Action {action}  with {eventId}, productName={productName}. Not found",
                        nameof(DiscountController), nameof(GetDiscount), eventId, productName);
                }
                else
                {
                    _logger.LogInformation("Controller {controller} Action {action}  with {eventId}, productName={productName}. Found",
                        nameof(DiscountController), nameof(GetDiscount), eventId, productName);
                }
                _logger.LogInformation("End Controller {controller} Action {action}  with {eventId}, productName={productName}",
                 nameof(DiscountController), nameof(GetDiscount), eventId, productName);
                return Ok(discount);
            }catch (Exception ex)
            {
                _logger.LogError("End Controller {controller} Action {action}  with {eventId}, productName={productName} with error : {error}",
                 nameof(DiscountController), nameof(GetDiscount), eventId, productName,ex);
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Coupon), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Coupon>> CreateDiscount([FromBody] Coupon coupon)
        {
            await _repository.CreateDiscount(coupon);
            return CreatedAtRoute("GetDiscount", new { productName = coupon.ProductName }, coupon);
        }

        [HttpPut]
        [ProducesResponseType(typeof(Coupon), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Coupon>> UpdateBasket([FromBody] Coupon coupon)
        {
            return Ok(await _repository.UpdateDiscount(coupon));
        }

        [HttpDelete("{productName}", Name = "DeleteDiscount")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<bool>> DeleteDiscount(string productName)
        {
            return Ok(await _repository.DeleteDiscount(productName));
        }
    }
}
