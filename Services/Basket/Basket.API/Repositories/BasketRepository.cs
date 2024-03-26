using Basket.API.Controllers;
using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDistributedCache _redisCache;
        private readonly ILogger<BasketRepository> _logger;
        public BasketRepository(IDistributedCache cache, ILogger<BasketRepository> logger)
        {
            _redisCache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ShoppingCart> GetBasket(string userName,Guid eventId)
        {
            var funtion = "GetBasket";
            _logger.LogInformation("Start {funtion} from {class} with {eventId} : userName( {value} )", funtion, "BasketRepository", eventId, userName);
            var basket = await _redisCache.GetStringAsync(userName);
            _logger.LogInformation("Start {funtion} from {class} with {eventId} : return value( {value} )", funtion, "BasketRepository", eventId, basket.ToString());
            if (String.IsNullOrEmpty(basket))
                return null;

            return JsonConvert.DeserializeObject<ShoppingCart>(basket);
        }
        public async Task<ShoppingCart> GetBasket(string userName)
        {
            var basket = await _redisCache.GetStringAsync(userName);

            if (String.IsNullOrEmpty(basket))
                return null;

            return JsonConvert.DeserializeObject<ShoppingCart>(basket);
        }
        public async Task<ShoppingCart> UpdateBasket(ShoppingCart basket)
        {
            await _redisCache.SetStringAsync(basket.UserName, JsonConvert.SerializeObject(basket));

            return await GetBasket(basket.UserName);
        }

        public async Task DeleteBasket(string userName)
        {
            await _redisCache.RemoveAsync(userName);
        }
    }
}
