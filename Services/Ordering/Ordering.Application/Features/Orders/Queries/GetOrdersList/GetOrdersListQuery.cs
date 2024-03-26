using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersList
{
    public class GetOrdersListQuery : IRequest<List<OrdersVm>>
    {
        public string UserName { get; set; }
        public string FromEvent { get; set; }
        public Guid EventId { get; set; }

        public GetOrdersListQuery(
            string userName,
            string fromEvent, Guid eventId)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            FromEvent = fromEvent;
            EventId = eventId;
        }
    }
}
