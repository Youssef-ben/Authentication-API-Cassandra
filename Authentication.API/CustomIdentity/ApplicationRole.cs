namespace Authentication.API.CustomIdentity
{
    using System;
    using AspNetCore.Identity.Cassandra.Models;
    using Cassandra.Mapping.Attributes;

    [Table("roles", Keyspace = "identity")]
    public class ApplicationRole : CassandraIdentityRole
    {
        public ApplicationRole()
            : base(Guid.NewGuid())
        {
        }
    }
}