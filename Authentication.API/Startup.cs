namespace Authentication.API
{
    using AspNetCore.Identity.Cassandra;
    using Authentication.API.Config;
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
            services.AddOptions();
            services.Configure<CassandraOptions>(this.Configuration.GetSection("Cassandra"));
            services.Configure<JwtOptions>(this.Configuration.GetSection("Jwt"));

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

            ApiConfiguration.Configure(services, this.Configuration);

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

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
