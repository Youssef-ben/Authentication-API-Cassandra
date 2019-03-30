namespace Authentication.API
{
    using Authentication.API.Config;
    using Authentication.API.Config.Settings;
    using Authentication.API.Config.Validation;
    using Authentication.API.CustomIdentity;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        private readonly string allowOriginPolicy = "AllowAllOrigines";

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Register the classes needed for the configuration validation.
            ApiConfiguration.ConfigureSettingsValidator(services);

            services.AddOptions();
            services.ConfigureAndValidate<CassandraSettings>(this.Configuration);
            services.ConfigureAndValidate<JwtSettings>(this.Configuration);

            services.AddCors(options =>
            {
                options.AddPolicy(
                    this.allowOriginPolicy,
                    builder =>
                    {
                        builder.AllowAnyOrigin();
                    });
            });

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Register all the API classes.
            ApiConfiguration.Configure(services, this.Configuration);

            // Register Cassandra Identity Managers
            services.AddTransient<ApplicationSignInManager>();
            services.AddTransient<ApplicationUserManager>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors(this.allowOriginPolicy);

            // API Custom Validation for the IOptions.
            app.UseCustomSettingsValidation(this.Configuration);

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
