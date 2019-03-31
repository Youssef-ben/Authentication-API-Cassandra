namespace Authentication.API.Models
{
    using System.Collections.Generic;
    using AspNetCore.Identity.Cassandra.Models;
    using Newtonsoft.Json;

    public class UserDto
    {
        public string ID { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public PhoneInfo Phone { get; set; }

        public ICollection<string> Roles { get; set; }

        public string Fullname
        {
            get
            {
                return $"{this.Firstname} {this.Lastname}";
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }
    }
}
