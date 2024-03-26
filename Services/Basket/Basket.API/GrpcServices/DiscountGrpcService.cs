using Discount.Grpc.Protos;

namespace Basket.API.GrpcServices
{
    public class DiscountGrpcService
    {
        
        private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoService;
        private readonly ILogger<DiscountGrpcService> _logger;
        public DiscountGrpcService(DiscountProtoService.DiscountProtoServiceClient discountProtoService, ILogger<DiscountGrpcService> logger)
        {
            _discountProtoService = discountProtoService ?? throw new ArgumentNullException(nameof(discountProtoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CouponModel> GetDiscount(string productName, Guid eventId)
        {
            
            _logger.LogInformation("Start {class} with {action} method with {eventId}. ProductName: {productName}", nameof(DiscountGrpcService), nameof(GetDiscount), eventId, productName);

            try
            {
                var discountRequest = new GetDiscountRequest { ProductName = productName };
                var coupon = await _discountProtoService.GetDiscountAsync(discountRequest);
                _logger.LogInformation("{class} with {action} method with {eventId} completed successfully. ProductName: {productName}. Discount: {discountAmount}"
                    , nameof(DiscountGrpcService), nameof(GetDiscount),eventId, productName, coupon.Amount);
                return coupon;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{class} with {action}  method with {eventId} failed unexpectedly. ProductName: {productName}",
                   nameof(DiscountGrpcService), nameof(GetDiscount), eventId, productName);
                throw; 
            }
            finally
            {
                _logger.LogInformation("End {class} with {action}  method with {eventId}. ProductName: {productName}", nameof(DiscountGrpcService), nameof(GetDiscount), eventId, productName);
            }
        }
        
    }
}
