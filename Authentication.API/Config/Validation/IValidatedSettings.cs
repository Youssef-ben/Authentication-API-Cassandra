namespace Authentication.API.Config.Validation
{
    public interface IValidatedSettings<TSettings>
        where TSettings : class
    {
        TSettings ValidateAndGetSettings(bool forceValidate = false);
    }
}
