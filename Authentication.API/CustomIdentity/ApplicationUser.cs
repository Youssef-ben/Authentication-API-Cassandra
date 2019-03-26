namespace Authentication.API.CustomIdentity
{
    using System;
    using AspNetCore.Identity.Cassandra.Models;
    using Cassandra.Mapping.Attributes;

    [Table("users", Keyspace = "identity")]
    public class ApplicationUser : CassandraIdentityUser
    {
        public ApplicationUser()
            : base(Guid.NewGuid())
        {
        }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string PasswordPublicSalt { get; internal set; }

        public string PasswordPrivateSalt { get; internal set; }
    }
}