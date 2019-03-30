namespace Authentication.API.Config.Validation
{
    internal class ValidationError
    {
        public ValidationError(string description)
        {
            this.Error = description;
        }

        public string Error { get; set; }
    }
}
