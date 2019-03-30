namespace Authentication.API.Config.Validation
{
    using System.Collections;
    using System.ComponentModel.DataAnnotations;

    public class RequireOneElement : ValidationAttribute
    {
        private readonly int minElements;

        public RequireOneElement(int minElements)
        {
            this.minElements = minElements;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var list = value as IList;

            var result = list?.Count >= this.minElements;

            return result
                ? ValidationResult.Success
                : new ValidationResult($"{validationContext.DisplayName} requires at least {this.minElements} element" + (this.minElements > 1 ? "s" : string.Empty));
        }
    }
}
