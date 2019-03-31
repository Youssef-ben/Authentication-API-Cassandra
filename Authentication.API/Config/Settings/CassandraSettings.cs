namespace Authentication.API.Config.Settings
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using AspNetCore.Identity.Cassandra;
    using Authentication.API.Config.Validation;

    public class CassandraSettings
    {
        [RequireOneElement(1)]
        public List<string> ContactPoints { get; set; }

        [Required]
        public CassandraCredentials Credentials { get; set; }

        [Required]
        public int Port { get; set; }

        [Required]
        public string KeyspaceName { get; set; }

        public Dictionary<string, string> Replication { get; set; } = null;

        [Required]
        public CassandraQueryOptions Query { get; set; }

        public bool DurableWrites { get; set; } = true;

        public static CassandraSettings Convert(CassandraOptions option)
        {
            return new CassandraSettings()
            {
                ContactPoints = option.ContactPoints,
                Credentials = option.Credentials,
                KeyspaceName = option.KeyspaceName,
                Replication = option.Replication,
                DurableWrites = option.DurableWrites,
                Query = option.Query
            };
        }
    }
}
