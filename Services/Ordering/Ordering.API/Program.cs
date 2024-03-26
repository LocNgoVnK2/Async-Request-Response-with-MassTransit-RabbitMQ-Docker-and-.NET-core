
using EventBus.Messages.Common;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Ordering.API.EventBusConsumer;
using Ordering.API.Extentions;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;

using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//regis config application ( in here have config pineline MediaR)
builder.Services.AddApplicationServices();
//regis config Infrastructure 
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<BasketCheckoutConsumer>();
// Add heal check .
builder.Services.AddHealthChecks().AddDbContextCheck<OrderContext>();
// Add services to the rappitMQ.
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<BasketCheckoutConsumer>();
    config.UsingRabbitMq((ctx, cfg) => {
        cfg.Host(/*"amqp://guest:guest@localhost:5672"*/builder.Configuration["EventBusSettings:HostAddress"]);
        
        cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue,c =>
        {
            c.ConfigureConsumer<BasketCheckoutConsumer>(ctx);
        });
        //cfg.UseHealthCheck(ctx);

    });
});
// FOR TRACING
builder.Services.AddOpenTelemetry().WithTracing(b => {
    b.SetResourceBuilder(
        ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName))
     .AddAspNetCoreInstrumentation()
     .AddOtlpExporter(opts => { opts.Endpoint = new Uri(builder.Configuration["Jeager:IdsUrl"]); });
});
var app = builder.Build();
app.MigrateDatabase<OrderContext>((context, services) =>
{

    var logger = services.GetService<ILogger<OrderContextSeed>>();
    OrderContextSeed
        .SeedAsync(context, logger)
        .Wait();
});
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
