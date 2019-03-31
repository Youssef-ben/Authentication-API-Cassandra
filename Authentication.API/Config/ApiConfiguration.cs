namespace Authentication.API.Config
{
    using System.Text;
    using AspNetCore.Identity.Cassandra;
    using AspNetCore.Identity.Cassandra.Extensions;
    using Authentication.API.Config.Settings;
    using Authentication.API.Config.Validation;
    using Authentication.API.CustomIdentity;
    using Authentication.API.Models;
    using Cassandra;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;

    public static class ApiConfiguration
    {
        public static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            ConfigureCassandraSession(services, configuration);

            ConfigureCassandraIdentity(services);

            ConfigureJwtToken(services, configuration);

            // Register Cassandra Identity Managers
            services.AddTransient<ApplicationSignInManager>();
            services.AddTransient<ApplicationUserManager>();
        }

        public static void ConfigureSettingsValidator(IServiceCollection services)
        {
            services.AddTransient<IValidatedSettings<JwtSettings>, ValidateSettings<JwtSettings>>();
        }

        public static void InitialData(this IApplicationBuilder self)
        {
            var userManager = self.ApplicationServices.GetService<ApplicationUserManager>();
            var roleManager = self.ApplicationServices.GetService<RoleManager<ApplicationRole>>();

            roleManager.CreateRoleAsync();
            userManager.CreateSystemUser();
        }

        private static void ConfigureCassandraSession(IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetConfigurationInstance<CassandraOptions>(nameof(CassandraSettings));
            var portOptions = configuration.GetConfigurationInstance<CassandraSettings>(nameof(CassandraSettings));

            services.AddCassandraSession(() =>
            {
                return Cluster.Builder()
                    .AddContactPoints(options.ContactPoints)
                    .WithPort(portOptions.Port)
                    .WithCredentials(options.Credentials.UserName, options.Credentials.Password)
                    .Build()
                    .Connect();
            });
        }

        private static void ConfigureCassandraIdentity(IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddCassandraErrorDescriber<CassandraErrorDescriber>()
                .UseCassandraStores<Cassandra.ISession>()
                .AddDefaultTokenProviders();
        }

        private static void ConfigureJwtToken(IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetConfigurationInstance<JwtSettings>(nameof(JwtSettings));

            var key = Encoding.ASCII.GetBytes(options.JwtKey);
            var tokenParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                ValidateIssuer = false,
                ValidateAudience = false,

                ValidateLifetime = true,
            };

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.IncludeErrorDetails = true;
                x.SaveToken = true;

                x.TokenValidationParameters = tokenParameters;

                x.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();
                        c.Response.StatusCode = 401;
                        c.Response.ContentType = "application/json";
                        return c.Response.WriteAsync(c.Exception.ToString());
                    }
                };
            });
        }

        private static async void CreateRoleAsync(this RoleManager<ApplicationRole> self)
        {
            var role = new ApplicationRole()
            {
                Name = "Admin"
            };

            var result = await self.CreateAsync(role);
        }

        private static async void CreateSystemUser(this ApplicationUserManager self)
        {
            var user = new ApplicationUser()
            {
                Firstname = "Ucef",
                Lastname = "Ben",
                UserName = "ucef-ben@hotmail.fr",
                Email = "ucef-ben@hotmail.fr",
            };

            var result = await self.CreateAsync(user, "Azerty&0123");

            var roleResult = await self.AddToRoleAsync(user, "ADMIN");
        }
    }
}
