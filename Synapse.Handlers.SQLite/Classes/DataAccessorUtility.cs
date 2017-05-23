using System;
using System.Data;
using System.Data.SQLite;


namespace Synapse.Handlers.SQLite
{
    public class DataAccessorUtility
    {
        public static string ConnectionString { get; internal set; }

        static public bool TestConnection(out Exception exception, out string message)
        {
            bool ok = true;
            exception = null;

            try
            {
                using( SQLiteConnection c = new SQLiteConnection( ConnectionString ) )
                {
                    c.Open();
                    message = $"Using: {c.FileName}";
                }
            }
            catch( Exception ex )
            {
                exception = ex;
                message = ex.Message;
                ok = false;
            }

            return ok;
        }

        public DataTable ExecuteQuery(string sql, SQLiteConnection connection = null)
        {
            DataTable dt = new DataTable();

            if( connection == null )
            {
                using( SQLiteConnection c = new SQLiteConnection( ConnectionString ) )
                {
                    c.Open();
                    using( SQLiteCommand cmd = new SQLiteCommand( sql, c ) )
                    {
                        SQLiteDataReader reader = cmd.ExecuteReader( CommandBehavior.SingleResult );
                        dt.Load( reader );
                    }
                }
            }
            else
            {
                using( SQLiteCommand cmd = new SQLiteCommand( sql, connection ) )
                {
                    SQLiteDataReader reader = cmd.ExecuteReader( CommandBehavior.SingleResult );
                    dt.Load( reader );
                }
            }

            return dt;
        }

        public void ExecuteNonQuery(string sql, SQLiteConnection connection = null)
        {
            if( connection == null )
            {
                using( SQLiteConnection c = new SQLiteConnection( ConnectionString ) )
                {
                    c.Open();
                    using( SQLiteCommand cmd = new SQLiteCommand( sql, c ) )
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using( SQLiteCommand cmd = new SQLiteCommand( sql, connection ) )
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public long? GetLastRowId(SQLiteConnection c)
        {
            long? id = 0;
            using( SQLiteCommand cmd = new SQLiteCommand( "select last_insert_rowid()", c ) )
            {
                id = (long?)cmd.ExecuteScalar();
            }

            return id;
        }

        public int GetEpoch()
        {
            return (int)(DateTime.UtcNow - new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc )).TotalSeconds;
        }
    }
}