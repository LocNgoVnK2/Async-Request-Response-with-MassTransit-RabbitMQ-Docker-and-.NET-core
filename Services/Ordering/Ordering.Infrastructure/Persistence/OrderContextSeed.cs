using Microsoft.Extensions.Logging;
using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Persistence
{
    public class OrderContextSeed
    {
        public static async Task SeedAsync(OrderContext orderContext, ILogger<OrderContextSeed> logger)
        {
            if (!orderContext.Orders.Any())
            {
                orderContext.Orders.AddRange(GetPreconfiguredOrders());
                await orderContext.SaveChangesAsync();
                logger.LogInformation("Seed database associated with context {DbContextName}", typeof(OrderContext).Name);
            }
        }

        private static IEnumerable<Order> GetPreconfiguredOrders()
        {
            var orders = new List<Order>
    {
        new Order() {
            UserName = "swn",
            FirstName = "Mehmet",
            LastName = "Ozkaya",
            EmailAddress = "ezozkme@gmail.com",
            AddressLine = "Bahcelievler",
            Country = "Turkey",
            State = "Istanbul",
            ZipCode = "12345",
            TotalPrice = 350,
            CardName = "John Doe",
            CardNumber = "1234 5678 9012 3456",
            Expiration = "12/24",
            CVV = "123",
            PaymentMethod = 1
        }
    };

            // Thêm thông tin cho EntityBase cho mỗi đơn hàng
            foreach (var order in orders)
            {
                order.CreatedBy = "System";
                order.CreatedDate = DateTime.UtcNow;
                order.LastModifiedBy = "system";
                order.LastModifiedDate = DateTime.UtcNow;
            }

            return orders;
       
        }
    }
}
