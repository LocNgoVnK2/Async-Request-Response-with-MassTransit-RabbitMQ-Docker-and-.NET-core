
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;


var builder = WebApplication.CreateBuilder(args);


/*
var serviceName = "ServiceApiGatewayA";
var serviceVersion = "1.0";
builder.Services.AddOpenTelemetryTracing((b) =>
{
    b.AddSource(serviceName)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion));
    b.AddAspNetCoreInstrumentation();
    b.AddHttpClientInstrumentation();
    b.AddConsoleExporter();
    b.AddJaegerExporter(opt =>
    {
        opt.ExportProcessorType = ExportProcessorType.Batch;
        opt.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>()
        {
 
            MaxQueueSize = 2048,
            ScheduledDelayMilliseconds = 5000,
            ExporterTimeoutMilliseconds = 30000,
            MaxExportBatchSize = 512,
        };
    });
});
*/
builder.Services.AddOpenTelemetry().WithTracing(b => {
    b.SetResourceBuilder(
        ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName))
     .AddAspNetCoreInstrumentation()
     .AddOtlpExporter(opts => { opts.Endpoint = new Uri(builder.Configuration["Jeager:IdsUrl"]); });
});



IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", true, true)
                            .Build();

var authenticationProviderKey = "IdentityApiKey";

// NUGET - Microsoft.AspNetCore.Authentication.JwtBearer

builder.Services.AddAuthentication()
 .AddJwtBearer(authenticationProviderKey, x =>
 {
     x.Authority = builder.Configuration["IdentityServerSetting:IdsUrl"];
     x.RequireHttpsMetadata = false;
     x.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateAudience = false,
         ValidateIssuer = false,
     };
 });

builder.Services.AddOcelot(configuration).AddCacheManager(settings => settings.WithDictionaryHandle());





var app = builder.Build();


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


app.UseOcelot().Wait();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Product.API started");

// Get IdentityServer Url
logger.LogInformation("IDENTITY_SERVER_URL: {url}", builder.Configuration["IdentityServerSetting:IdsUrl"]);

app.MapGet("/", () => "Hello World!");

app.Run();
