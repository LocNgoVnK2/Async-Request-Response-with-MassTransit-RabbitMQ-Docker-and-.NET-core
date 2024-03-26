using Discount.API.Repositories;
using EventBus.Messages.MessageContracts;
using MassTransit;

namespace Discount.API.EventBusConsumer
{
    public class DiscountResponseConsumer : IConsumer<DiscountRequest>
    {
        private readonly IDiscountRepository _discountRepository;
        public DiscountResponseConsumer(IDiscountRepository discountRepository)
        {
            _discountRepository = discountRepository;
        }
        public async Task Consume(ConsumeContext<DiscountRequest> context)
        {
            var discount = await _discountRepository.GetDiscount(context.Message.ProductName);
            if (discount == null)
                throw new InvalidOperationException("Discount not found");
            // thay vì gán tay có thể dùng IMapper
            await context.RespondAsync<DiscountReponse>(new DiscountReponse()
            {
                Id = discount.Id,
                ProductName = discount.ProductName,
                Description = discount.Description,
                Amount = discount.Amount,
            });
        }
    }
}
