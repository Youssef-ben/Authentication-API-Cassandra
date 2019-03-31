namespace Authentication.API.Config.Settings
{
    using System.ComponentModel.DataAnnotations;

    public class JwtSettings
    {
        [Required]
        [MinLength(6)]
        public string JwtKey { get; set; }

        [Required]
        [Url]
        public string JwtIssuer { get; set; }

        [Required]
        [Range(1, 30)]
        public int JwtExpireDays { get; set; }
    }
}
