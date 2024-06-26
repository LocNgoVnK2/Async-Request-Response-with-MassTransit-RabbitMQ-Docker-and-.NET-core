﻿using Dapper;
using Discount.API.Controllers;
using Discount.API.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Discount.API.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiscountRepository> _logger;
        public DiscountRepository(IConfiguration configuration, ILogger<DiscountRepository> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Coupon> GetDiscount(string productName)
        {


            using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            _logger.LogInformation("Start Repository {repository} Action {action}  with {connection}, productName={productName}"
               , nameof(DiscountRepository), nameof(GetDiscount), connection, productName);
            var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>
                ("SELECT * FROM Coupon WHERE ProductName = @ProductName", new { ProductName = productName });
            
            _logger.LogInformation("End Repository {repository} Action {action}  w productName={productName}"
               , nameof(DiscountRepository), nameof(GetDiscount), productName);
            if (coupon == null)
                return new Coupon { ProductName = "No Discount", Amount = 0, Description = "No Discount Desc" };
            return coupon;
        }

        public async Task<bool> CreateDiscount(Coupon coupon)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var affected =
                await connection.ExecuteAsync
                    ("INSERT INTO Coupon (ProductName, Description, Amount) VALUES (@ProductName, @Description, @Amount)",
                            new { ProductName = coupon.ProductName, Description = coupon.Description, Amount = coupon.Amount });

            if (affected == 0)
                return false;

            return true;
        }

        public async Task<bool> UpdateDiscount(Coupon coupon)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var affected = await connection.ExecuteAsync
                    ("UPDATE Coupon SET ProductName=@ProductName, Description = @Description, Amount = @Amount WHERE Id = @Id",
                            new { ProductName = coupon.ProductName, Description = coupon.Description, Amount = coupon.Amount, Id = coupon.Id });

            if (affected == 0)
                return false;

            return true;
        }

        public async Task<bool> DeleteDiscount(string productName)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var affected = await connection.ExecuteAsync("DELETE FROM Coupon WHERE ProductName = @ProductName",
                new { ProductName = productName });

            if (affected == 0)
                return false;

            return true;
        }
    }
}
