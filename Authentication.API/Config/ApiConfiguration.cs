namespace Authentication.API.Config
{
    using System.Text;
    using AspNetCore.Identity.Cassandra;
    using AspNetCore.Identity.Cassandra.Extensions;
    using Authentication.API.CustomIdentity;
    using Cassandra;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        private static void ConfigureCassandraSession(IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetConfigurationInstance<CassandraOptions>("Cassandra");

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
                .UseCassandraStores<ISession>()
                .AddDefaultTokenProviders();
        }

        private static void ConfigureJwtToken(IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetConfigurationInstance<JwtOptions>("Jwt");

            var key = Encoding.ASCII.GetBytes(options.JwtKey);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        /// <summary>
        /// Extension Used to Create an IOptionsSnapshot instance of the specified configuration Section.
        /// </summary>
        /// <typeparam name="TClass">Class to be used for the configuration.</typeparam>
        /// <param name="self">{IConfiguration} The instance of the configuration object.</param>
        /// <param name="section">{String} Section to be mapped</param>
        /// <returns>New Instance of the Specified Section</returns>
        private static TClass GetConfigurationInstance<TClass>(this IConfiguration self, string section)
            where TClass : class, new()
        {
            var instance = new TClass();
            self.Bind(section, instance);
            return instance;
        }
    }
}
