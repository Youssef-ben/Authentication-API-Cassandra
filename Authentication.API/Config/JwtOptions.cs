namespace Authentication.API.Config
{
    using System.Collections.Generic;

    public class JwtOptions
    {
        public string JwtKey { get; set; }

        public string JwtIssuer { get; set; }

        public int JwtExpireDays { get; set; }

        public List<string> Audience { get; set; }
    }
}
