namespace Authentication.API.Config.Validation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Newtonsoft.Json;

    internal class ValidateSettings<TSettings> : IValidatedSettings<TSettings>
        where TSettings : class, new()
    {
        private static TSettings currentSettings;
        private readonly TSettings newSettings;

        public ValidateSettings(TSettings options)
        {
            this.newSettings = options;
        }

        public TSettings ValidateAndGetSettings(bool forceValidate = false)
        {
            if (currentSettings == null || forceValidate)
            {
                var validationResults = this.ValidateModel(this.newSettings).ToArray();

                if (validationResults.Any())
                {
                    var errors = new List<ValidationError>();

                    foreach (var result in validationResults)
                    {
                        errors.Add(new ValidationError(result.ErrorMessage));
                    }

                    var error = new
                    {
                        Description = "The configuration file contains errors.",
                        Errors = errors
                    };

                    throw new ApplicationException(JsonConvert.SerializeObject(error));
                }

                currentSettings = this.newSettings;
            }

            return currentSettings;
        }

        private IEnumerable<ValidationResult> ValidateModel(object model)
        {
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(model, context, results, true);

            return results;
        }
    }
}
