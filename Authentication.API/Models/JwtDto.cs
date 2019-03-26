namespace Authentication.API.Models
{
    using System;

    public class JwtDto
    {
        public string JwtToken { get; set; }

        public string UserID { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}
