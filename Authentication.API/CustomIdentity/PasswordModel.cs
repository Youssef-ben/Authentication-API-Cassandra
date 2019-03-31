namespace Authentication.API.CustomIdentity
{
    using Newtonsoft.Json;

    public class PasswordModel
    {
        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("password_hash")]
        public string PasswordHash { get; set; }

        [JsonProperty("public_salt")]
        public string PasswordPublicSalt { get; set; }

        [JsonProperty("private_salt")]
        public string PasswordPrivateSalt { get; set; }
    }
}
