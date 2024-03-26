using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Persistence;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersList
{
    public class GetOrdersListQueryHandler : IRequestHandler<GetOrdersListQuery, List<OrdersVm>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOrdersListQueryHandler> _logger;

        public GetOrdersListQueryHandler(
            IOrderRepository orderRepository, IMapper mapper, ILogger<GetOrdersListQueryHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger;

        }

        public async Task<List<OrdersVm>> Handle(GetOrdersListQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{fromEvent}, Start {handler} with {eventId}, username={username}",
               request.FromEvent, nameof(GetOrdersListQueryHandler), request.EventId, request.UserName);

            var orderList = await _orderRepository.GetOrdersByUserName(request.UserName);

            _logger.LogInformation("{fromEvent}, End {handler} with {eventId}, username={username}",
             request.FromEvent, nameof(GetOrdersListQueryHandler), request.EventId, request.UserName);
            return _mapper.Map<List<OrdersVm>>(orderList);
        }
    }
}
