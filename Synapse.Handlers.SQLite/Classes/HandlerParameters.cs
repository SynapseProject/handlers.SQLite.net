using System;

namespace Synapse.Handlers.SQLite
{
    public class HandlerParameters
    {
        public QueryType Type { get; set; }
        public string Command { get; set; }
    }
}