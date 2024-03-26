using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;
using IdentityServer4;
using System.Security.Claims;

namespace IdentityServer
{
    public class AppSettings
    {
        private static readonly IConfiguration _configuration;

        static AppSettings()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public static string GetIdentityClientInUrl()
        {
            return _configuration["IdentityClientSetting:IdsUrl"];
        }

    }
    public class Config
    {
       

        public static IEnumerable<Client> Clients =>

            new Client[]
            {
                   new Client
                   {
                        ClientId = "shopClient",
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        ClientSecrets =
                        {
                            new Secret("secret".Sha256())
                        },
                        AllowedScopes = { "shopAPI" }
                   },
                   new Client
                   {
                       ClientId = "shop_mvc_client",
                       ClientName = "Shop MVC Web App",
                       AllowedGrantTypes = GrantTypes.Code,
                       
                       AllowRememberConsent = false,
                       RedirectUris = new List<string>()
                       {
                           "https://localhost:5002/signin-oidc",
                           AppSettings.GetIdentityClientInUrl()+"/signin-oidc"
                       },
                       PostLogoutRedirectUris = new List<string>()
                       {
                           "https://localhost:5002/signout-callback-oidc",
                           AppSettings.GetIdentityClientInUrl()+"/signout-callback-oidc"
                       },
                       ClientSecrets = new List<Secret>
                       {
                           new Secret("secret".Sha256())
                       },
                       AllowedScopes = new List<string>
                       {
                           IdentityServerConstants.StandardScopes.OpenId,
                           IdentityServerConstants.StandardScopes.Profile,
                           "roles"
                       }
                   }

            };

        public static IEnumerable<ApiScope> ApiScopes =>
           new ApiScope[]
           {
               new ApiScope("shopAPI", "Shop API")
           };

        public static IEnumerable<ApiResource> ApiResources =>
          new ApiResource[]
          {
               //new ApiResource("movieAPI", "Movie API")
          };

        public static IEnumerable<IdentityResource> IdentityResources =>
          new IdentityResource[]
          {
               new IdentityResources.OpenId(),
               new IdentityResources.Profile(),
               new IdentityResource(
                    "roles",
                    "Your role(s)",
                    new List<string>() { "role" })
          };

        public static List<TestUser> TestUsers =>
            new List<TestUser>
            {
                
              new TestUser
                {
                    SubjectId = "5BE86359-073C-434B-AD2D-A3932222DABE",
                    Username = "locngo",
                    Password = "123",
                    Claims = new List<Claim>
                    {
                        new Claim(JwtClaimTypes.GivenName, "locngo"),
                        new Claim(JwtClaimTypes.FamilyName, "loc"),
                        new Claim(JwtClaimTypes.Role,"admin")
                    }
                },
                new TestUser
                {
                    SubjectId = "9A2B0A89-C182-4FAC-BC33-2E500E92D5F1",
                    Username = "test",
                    Password = "123",
                    Claims = new List<Claim>
                    {
                        new Claim(JwtClaimTypes.GivenName, "John"),
                        new Claim(JwtClaimTypes.FamilyName, "Doe"),
                        new Claim(JwtClaimTypes.Role,"user")
                    }
                }

            };
     
    }
}
