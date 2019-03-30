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
        }

        public static void ConfigureSettingsValidator(IServiceCollection services)
        {
            services.AddTransient<IValidatedSettings<JwtSettings>, ValidateSettings<JwtSettings>>();
        }

        private static void ConfigureCassandraSession(IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetConfigurationInstance<CassandraOptions>(nameof(CassandraSettings));

            services.AddCassandraSession(() =>
            {
                var contactPoints = options.ContactPoints;

                return Cluster.Builder()
                    .AddContactPoints(contactPoints)
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
    }
}
