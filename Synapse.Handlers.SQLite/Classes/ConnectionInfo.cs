using System;

namespace Synapse.Handlers.SQLite
{
    public class ConnectionInfo
    {
        public string FileName { get; set; }

        internal string ConnectionString { get { return $"Data Source={FileName};Version=3;"; } }
    }
}