using Basket.API.GrpcServices;
using Basket.API.Repositories;

using Discount.Grpc.Protos;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using EventBus.Messages.MessageContracts;
using EventBus.Messages.Common;
var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//config Grpc is calling to Discount
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(o => o.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"])); ;
builder.Services.AddScoped<DiscountGrpcService>();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration["CacheSettings:ConnectionString"], "Redis Health", HealthStatus.Degraded);
//config AddMassTransit rappit MQ

builder.Services.AddMassTransit(x =>
{
    x.AddBus(context => Bus.Factory.CreateUsingRabbitMq(c =>
    {
        c.Host(builder.Configuration["EventBusSettings:HostAddress"]);
        c.ConfigureEndpoints(context);
    }));

    x.AddRequestClient<DiscountRequest>();
});



/*
   // configure the consumer on a specific endpoint address
    x.AddConsumer<CheckOrderStatusConsumer>()
        .Endpoint(e => e.Name = "order-status");
        
    // Sends the request to the specified address, instead of publishing it
    x.AddRequestClient<CheckOrderStatus>(new Uri("exchange:order-status"));
    
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    }));
 */
//builder.Services.AddMassTransitHostedService();

builder.Services.AddStackExchangeRedisCache(options =>
{
 
    options.Configuration = builder.Configuration.GetSection("CacheSettings:ConnectionString").Value;
    
});
builder.Services.AddOpenTelemetry().WithTracing(b => {
    b.SetResourceBuilder(
        ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName))
     .AddAspNetCoreInstrumentation()
     .AddOtlpExporter(opts => { opts.Endpoint = new Uri(builder.Configuration["Jeager:IdsUrl"]); });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
});
app.UseAuthorization();

app.MapControllers();

app.Run();
