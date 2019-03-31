namespace Authentication.API.Config.Validation
{
    using System;
    using System.Diagnostics;
    using AspNetCore.Identity.Cassandra;
    using Authentication.API.Config.Settings;
    using Authentication.API.Models;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public static class ValidationExtensions
    {
        public static IApplicationBuilder UseCustomSettingsValidation(this IApplicationBuilder self, IConfiguration config)
        {
            try
            {
                self.ApplicationServices.UseSettingsValidation<CassandraSettings>(config);
                self.ApplicationServices.UseSettingsValidation<JwtSettings>(null);
                return self;
            }
            catch (Exception)
            {
                var appLifeTime = self.ApplicationServices.GetService<IApplicationLifetime>();
                appLifeTime.StopApplication();
                return null;
            }
        }

        /// <summary>
        /// Create an IOption of the selected section form the {appsettings.json} file and validate it's values.
        /// </summary>
        /// <typeparam name="SettingsClass">{Class} The settings class.</typeparam>
        /// <param name="self">{IServiceCollection} Service Collection.</param>
        /// <param name="config">{IConfiguration} Configuration from the appsettings</param>
        /// <returns>{IServiceCollection}</returns>
        public static IServiceCollection ConfigureAndValidate<SettingsClass>(this IServiceCollection self, IConfiguration config)
            where SettingsClass : class, new()
        {
            if (typeof(SettingsClass).Name.Equals(nameof(CassandraSettings)))
            {
                self.Configure<CassandraOptions>(config.GetSection(typeof(SettingsClass).Name));
            }
            else
            {
                self.Configure<SettingsClass>(config.GetSection(typeof(SettingsClass).Name));
            }

            // Validate the Settings section.
            var option = config.GetConfigurationInstance<SettingsClass>(typeof(SettingsClass).Name);
            ValidateSettings<SettingsClass>(option);

            return self;
        }

        /// <summary>
        /// Create an IOptionMonitor to watch for the changes in the {appsettings.json} and validate it if updated.
        /// </summary>
        /// <typeparam name="SettingsClass">{Class} The settings class.</typeparam>
        /// <param name="self">{IServiceProvider} The app service provider.</param>
        /// <param name="config">{IConfiguration} The app Configuration service.</param>
        /// <returns>{IServiceProvider} Return it self.</returns>
        private static IServiceProvider UseSettingsValidation<SettingsClass>(this IServiceProvider self, IConfiguration config)
            where SettingsClass : class, new()
        {
            if (typeof(SettingsClass).Name.Equals(nameof(CassandraSettings)))
            {
                self.GetService<IOptionsMonitor<CassandraOptions>>()
                    .AddOnChangeToMonitor(self, config);
            }
            else
            {
                self.GetService<IOptionsMonitor<SettingsClass>>()
                    .AddOnChangeToMonitor(self, null);
            }

            return self;
        }

        /// <summary>
        /// Add {OnChange} listener for the IOptionMonitor.
        /// </summary>
        /// <typeparam name="SettingsClass">{Class} The setting class.</typeparam>
        /// <param name="self">{OptionMonitor}</param>
        /// <param name="provider">{ServiceProvider}</param>
        /// <param name="config">{Iconfiguration}</param>
        /// <returns>{OptionMonitor} the new value.</returns>
        private static IOptionsMonitor<SettingsClass> AddOnChangeToMonitor<SettingsClass>(this IOptionsMonitor<SettingsClass> self, IServiceProvider provider, IConfiguration config)
            where SettingsClass : class, new()
        {
            self.OnChange(settings =>
            {
                try
                {
                    // Make sure that every time when the settings file changes, it will be validated.
                    if (typeof(SettingsClass).Name.Equals(nameof(CassandraOptions)))
                    {
                        var option = CassandraSettings.Convert(self.CurrentValue as CassandraOptions); // config.GetConfigurationInstance<CassandraSettings>(typeof(CassandraSettings).Name);
                        ValidateSettings<CassandraSettings>(option);
                    }
                    else
                    {
                        ValidateSettings<SettingsClass>(self.CurrentValue);
                    }
                }
                catch (Exception ex)
                {
                    // Normaly we should have a logger istead of Debug.
                    Debug.WriteLine($"Fatal : [{ex.Message}]");
                    var appLifeTime = provider.GetService<IApplicationLifetime>();
                    appLifeTime.StopApplication();
                }
            });

            return self;
        }

        /// <summary>
        /// Validate the values of the settings class that map to a sectionin the {appsettings.json} file.
        /// </summary>
        /// <typeparam name="SettingsClass">{Class} The settings class.</typeparam>
        /// <param name="option">{Class} The settings class instance.</param>
        /// <returns>{Class} new version of the setting class.</returns>
        private static SettingsClass ValidateSettings<SettingsClass>(SettingsClass option)
            where SettingsClass : class, new()
        {
            var validatedSettings = new ValidateSettings<SettingsClass>(option);
            return validatedSettings.ValidateAndGetSettings(true);
        }
    }
}
