using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using EventBus.Messages.MessageContracts;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Basket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _repository;
        
        private readonly DiscountGrpcService _discountGrpcService;
        
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;
        private readonly ILogger<BasketController> _logger;
        IRequestClient<DiscountRequest> _client;
        public BasketController(IBasketRepository repository, DiscountGrpcService discountGrpcService, IPublishEndpoint publishEndpoint, IMapper mapper, ILogger<BasketController> logger, IRequestClient<DiscountRequest> client)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            
            _discountGrpcService = discountGrpcService ?? throw new ArgumentNullException(nameof(discountGrpcService));
            
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
        {
            Guid eventId = Guid.NewGuid();
            _logger.LogInformation("Start Controller {controller} Action {action}  with {eventId}, username={username}"
                , nameof(BasketController), nameof(GetBasket), eventId, userName);
            var basket = await _repository.GetBasket(userName);
            if (basket == null)
            {
                _logger.LogInformation("Controller {controller} Action {action}  with {eventId}, username={username}. Not found",
                    nameof(BasketController), nameof(GetBasket), eventId, userName);
            }
            else
            {
                _logger.LogInformation("Controller {controller} Action {action}  with {eventId}, username={username}. Found",
                    nameof(BasketController), nameof(GetBasket), eventId, userName);
            }

            _logger.LogInformation("End Controller {controller} Action {action}  with {eventId}, username={username}",
                nameof(BasketController), nameof(GetBasket), eventId, userName);
            return Ok(basket ?? new ShoppingCart(userName));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
        {
         
            try
            {
                foreach (var item in basket.Items)
                {
                    /*
                     {
                      "userName": "string",
                      "items": [
                        {
                          "quantity": 0,
                          "color": "string",
                          "price": 0,
                          "productId": "602d2149e773f2a3990b47f5",
                          "productName": "IPhone X"
                        }
                      ]
                    }
                     */
                    //var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
                    var response = await _client.GetResponse<DiscountReponse>(new DiscountRequest{ ProductName = item.ProductName });
                    item.Price -= response.Message.Amount;
                }
                // end communicate

                var updatedBasket = await _repository.UpdateBasket(basket);

                return Ok(updatedBasket);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating basket :"+ ex);
            }
        }

        [HttpDelete("{userName}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBasket(string userName)
        {
            await _repository.DeleteBasket(userName);
            return Ok();
        }
        
        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            // get existing basket with total price            
            // Set TotalPrice on basketCheckout eventMessage
            // send checkout event to rabbitmq
            // remove the basket

            // get existing basket with total price
            var basket = await _repository.GetBasket(basketCheckout.UserName);
            if (basket == null)
            {
                return BadRequest();
            }
            // send checkout event to rabbitmq
            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = basket.TotalPrice; // type of basket is shopping cart

            await _publishEndpoint.Publish<BasketCheckoutEvent>(eventMessage);
            // remove the basket
            await _repository.DeleteBasket(basket.UserName);

            return Accepted();
        }
        
    }
}
