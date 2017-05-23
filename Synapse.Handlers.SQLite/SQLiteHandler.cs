using System;
using System.Data;

using Synapse.Core;
using Synapse.Handlers.SQLite;
using Newtonsoft.Json;

public class SQLiteHandler : HandlerRuntimeBase
{
    DataAccessorUtility _da = null;

    public override IHandlerRuntime Initialize(string config)
    {
        //deserialize the Config from the Handler declaration
        ConnectionInfo conn = DeserializeOrNew<ConnectionInfo>( config );
        DataAccessorUtility.ConnectionString = conn.ConnectionString;
        _da = new DataAccessorUtility();

        return this;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        //declare/initialize method-scope variables
        int cheapSequence = 0; //used to order message flowing out from the Handler
        const string __context = "Execute";
        ExecuteResult result = new ExecuteResult()
        {
            Status = StatusType.Complete,
            Sequence = Int32.MaxValue
        };
        string msg = "Complete";
        Exception exc = null;

        //deserialize the Parameters from the Action declaration
        HandlerParameters parms = DeserializeOrNew<HandlerParameters>( startInfo.Parameters );

        try
        {
            //if IsDryRun == true, test if ConnectionString is valid and works.
            if( startInfo.IsDryRun )
            {
                OnProgress( __context, "Attempting connection", sequence: cheapSequence++ );

                Exception ex = null;
                if( !DataAccessorUtility.TestConnection( out ex, out msg ) )
                    throw ex;

                result.ExitData = DataAccessorUtility.ConnectionString;
                result.Message = msg =
                    $"Connection test successful! Connection string: {DataAccessorUtility.ConnectionString}";
            }
            //else, select data as declared in Parameters.QueryString
            else
            {
                switch( parms.Type )
                {
                    case QueryType.NonQuery:
                    {
                        _da.ExecuteNonQuery( parms.Command );
                        break;
                    }
                    case QueryType.Query:
                    {
                        DataTable data = _da.ExecuteQuery( parms.Command );
                        result.ExitData = JsonConvert.SerializeObject( data, Formatting.Indented );
                        break;
                    }
                }
            }
        }
        //something wnet wrong: hand-back the Exception and mark the execution as Failed
        catch( Exception ex )
        {
            exc = ex;
            result.Status = StatusType.Failed;
            result.ExitData = msg =
                ex.Message + " | " + ex.InnerException?.Message;
        }

        //final runtime notification, return sequence=Int32.MaxValue by convention to supercede any other status message
        OnProgress( __context, msg, result.Status, sequence: Int32.MaxValue, ex: exc );

        return result;
    }

    public override object GetConfigInstance()
    {
        return new ConnectionInfo() { FileName = "path to yourDb.sqlite3/yourDb.db/etc." };
    }

    public override object GetParametersInstance()
    {
        return new HandlerParameters() { Type = QueryType.Query, Command = "select * from myTable" };
    }
}