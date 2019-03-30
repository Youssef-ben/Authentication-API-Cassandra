namespace Authentication.API
{
    using AspNetCore.Identity.Cassandra.Extensions;
    using Authentication.API.CustomIdentity;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
                .Build()
                .InitializeIdentityDb<ApplicationUser, ApplicationRole>()
                .Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
             .ConfigureAppConfiguration((hostingContext, config) =>
             {
                 config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);

                 // You can set ASPNETCORE_ENVIRONMENT = Local, Development, Staging or Production
                 config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                 config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                 config.AddEnvironmentVariables();
                 config.AddCommandLine(args);
             })
            .UseStartup<Startup>();
    }
}
