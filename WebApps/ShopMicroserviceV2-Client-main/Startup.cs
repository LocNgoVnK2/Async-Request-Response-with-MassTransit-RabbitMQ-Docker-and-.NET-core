using AspnetRunBasics.Services;

using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;

using System;
using System.Net.Http;
using IdentityModel;
using Microsoft.Net.Http.Headers;
using AspnetRunBasics.HttpHandlers;
using IdentityModel.Client;

using Microsoft.Extensions.Logging;
using static IdentityModel.OidcConstants;
using Microsoft.AspNetCore.Http;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;
using System.Diagnostics;


namespace AspnetRunBasics
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
          

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
      

            // config http clients and retry pattern
            services.AddHttpClient<ICatalogService, CatalogService>(c =>
                c.BaseAddress = new Uri(Configuration["ApiSettings:GatewayAddress"]))
              
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
    
            services.AddHttpClient<IBasketService, BasketService>(c =>
                c.BaseAddress = new Uri(Configuration["ApiSettings:GatewayAddress"]))
              
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<IOrderService, OrderService>(c =>
                c.BaseAddress = new Uri(Configuration["ApiSettings:GatewayAddress"]))
       
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
   
            // tracing with jeager
            services.AddOpenTelemetry().WithTracing(b => {
                b.SetResourceBuilder(
                    ResourceBuilder.CreateDefault().AddService(System.Reflection.Assembly.GetEntryAssembly().GetName().Name))
                 .AddAspNetCoreInstrumentation()
                 .AddHttpClientInstrumentation()
                 .AddOtlpExporter(opts => { opts.Endpoint = new Uri(Configuration["Jeager:IdsUrl"]); });
            });
            /*
            services.AddOpenTelemetry().WithTracing(b => {
                b.SetResourceBuilder(
                    ResourceBuilder.CreateDefault().AddService(System.Reflection.Assembly.GetEntryAssembly().GetName().Name))
                 .AddAspNetCoreInstrumentation()
                 .AddOtlpExporter(opts => { opts.Endpoint = new Uri("http://localhost:4317"); });
            });
            */

            // 1 create an HttpClient used for accessing the Movies.API
            //services.AddScoped<ClientCredentialsTokenRequest>();
            //config to consume identity server 4
            services.AddTransient<AuthenticationDelegatingHandler>();
                
      
            services.AddHttpClient("ShopAPIClient", client =>
            {
                client.BaseAddress = new Uri(Configuration["ApiSettings:GatewayAddress"]); // API GATEWAY URL
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
            });


            var connectTokenUrl = Configuration["IdentityServerSetting:IdsUrl"] + "/connect/token";
            services.AddSingleton(new ClientCredentialsTokenRequest
            {                                                
                Address = connectTokenUrl,// bugs
                ClientId = "shopClient",
                ClientSecret = "secret",
                Scope = "shopAPI"
            });

            //http://localhost:9091/
            services.AddAuthentication(options =>
            {
                
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
              .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
              .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
              {
                  options.Authority = Configuration["IdentityServerSetting:IdsUrl"];
                  options.RequireHttpsMetadata = false;

                  options.ClientId = "shop_mvc_client";
                  options.ClientSecret = "secret";
                  options.ResponseType = "code";

                  options.Scope.Add("openid");
                  options.Scope.Add("profile");
                  options.Scope.Add("roles");
                  options.ClaimActions.MapUniqueJsonKey("role", "role");
             

                  options.SaveTokens = true;
                  options.GetClaimsFromUserInfoEndpoint = true;
                  
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      NameClaimType = JwtClaimTypes.GivenName,
                      RoleClaimType = JwtClaimTypes.Role
                  };
                  
              });

            services.AddRazorPages();
            //add healchecks
            services.AddHealthChecks()
                .AddUrlGroup(new Uri(Configuration["ApiSettings:GatewayAddress"]+"/hc"), "Ocelot API Gw", HealthStatus.Degraded);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
               
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();
            // UseAuthentication after UseAuthorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }
        //custom policy 
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            // In this case will wait for
            //  2 ^ 1 = 2 seconds then
            //  2 ^ 2 = 4 seconds then
            //  2 ^ 3 = 8 seconds then
            //  2 ^ 4 = 16 seconds then
            //  2 ^ 5 = 32 seconds

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, retryCount, context) =>
                    {
                        Log.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
                    });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );
        }
    }
}
