using System;
using System.Data;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace xbridge.Modules
{
    public class ThreadHandler {
        Thread thread;
        public ThreadHandler() {
            thread = new Thread(Execute);
            thread.Start();
        }

        private BlockingCollection<Action> tasksCollection = new BlockingCollection<Action>();

        public Task<T> Run<T>(Func<T> action)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            tasksCollection.Add(() => {
                try
                {
                    var result = action.Invoke();
                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            return tcs.Task;
        }
        public async Task Run(Action action)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tasksCollection.Add(() =>
            {
                try
                {
                    action.Invoke();
                    tcs.SetResult(null);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            await tcs.Task;
        }

        void Execute()
        {
            foreach (var task in tasksCollection.GetConsumingEnumerable())
            {
                task.Invoke();
            }
        }
    }

    public class SQLiteThreadHandler: ThreadHandler
    {
        public SqliteConnection Connection;
        public SQLiteThreadHandler(String connectionString) {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Run(() =>
            {
                Connection = new SqliteConnection(connectionString);
                Connection.Open();
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }

    public class SQLiteResult
    {
        public object[][] rows;
        public string[] columns;
        // number of affected rows for an insert/update
        public int rowsAffected = 0;
        public long? insertId = null;
        public Exception error;
    }

    public class SQLite
    {
        private XBridge bridge;


        public SQLite(XBridge bridge)
        {
            this.bridge = bridge;
            SqliteConnection.SetConfig(SQLiteConfig.Serialized);
        }

        private Dictionary<string, SQLiteThreadHandler> dbs = new Dictionary<string, SQLiteThreadHandler>();
        private static Object[][] EMPTY_ROWS = new Object[][]{};
        private static String[] EMPTY_COLUMNS = new String[]{};
        private static readonly SQLiteResult EMPTY_RESULT = new SQLiteResult {rows = EMPTY_ROWS, columns = EMPTY_COLUMNS};

        public SQLiteThreadHandler _GetDatabase(string name){

            lock (dbs)
            {
                if (!dbs.ContainsKey(name))
                {
                    var core = bridge.GetModule<Files>();
                    var dir = core.DataDir() + "/sqlite";
                    Directory.CreateDirectory(dir);
                    if (name.Contains("/") || name.Contains(".."))
                        throw new Exception("invalid database name");
                    var path = dir + "/" + name + ".db";
                    if (!File.Exists(path))
                    {
                        //SqliteConnection.CreateFile(path);
                        File.Create(path);
                    }
                    var con = new SQLiteThreadHandler("URI=file:" + path + ",version=3");
                   
                    return dbs[name] = con;
                }
                else
                    return dbs[name];
            }
        }

        public Task<object> ExecMany(String db, string[] queries, object[][] parameters, bool readOnly)
        {
            var dbc = _GetDatabase(db);

            return dbc.Run(() =>
            {
                SQLiteResult[] tasks = new SQLiteResult[queries.Length];
                for (var i = 0; i < queries.Length; i++)
                {
                    var q = queries[i];
                    object[] p = null;
                    if (parameters != null)
                        p = parameters[i];
                    tasks[i] = _ExecAndCatch(dbc.Connection, q, p, readOnly);
                }
                return tasks as object;
            });
        }


        private SQLiteResult _ExecAndCatch(SqliteConnection dbc, string q, object[] p, bool readOnly)
        {

            try
            {
                return _Exec(dbc, q, p, readOnly);
            }
            catch (Exception e)
            {
                return new SQLiteResult { error = e };
            }
        }

        private static bool startsWithCaseInsensitive(String str, String substr)
        {
            int i = -1;
            int len = str.Length;
            while (++i < len)
            {
                char ch = str[i];
                if (!Char.IsWhiteSpace(ch))
                {
                    break;
                }
            }

            int j = -1;
            int substrLen = substr.Length;
            while (++j < substrLen)
            {
                if (j + i >= len)
                {
                    return false;
                }
                char ch = str[j + i];
                if (Char.ToLower(ch) != substr[j])
                {
                    return false;
                }
            }
            return true;
        }

        public Task<object> Exec(String db, string query, object[] parameters, bool readOnly)
        {
            var dbc = _GetDatabase(db);
            return dbc.Run(() =>
            {
                return _Exec(dbc.Connection, query, parameters, readOnly) as object;
            });
        }
        public SQLiteResult _Exec(SqliteConnection dbc, string query, object[] parameters, bool readOnly)
        {

            using (var cmd = new SqliteCommand(query, dbc))
            {
                if (startsWithCaseInsensitive(query, "insert"))
                {
                    cmd.CommandText = cmd.CommandText + ";select last_insert_rowid();";
                }
                cmd.Prepare();
                if (parameters != null)
                {
                    var len = parameters.Length;
                    for (var i = 0; i < len; ++i)
                    {
                        var p = new SqliteParameter
                        {
                            Value = parameters[i]
                        };
                        cmd.Parameters.Add(p);
                    }
                }
                if (startsWithCaseInsensitive(query, "select"))
                {
                    var reader = cmd.ExecuteReader();
                    var columns = new string[reader.FieldCount];
                    for (var i = reader.FieldCount - 1; i >= 0; --i)
                        columns[i] = reader.GetName(i);
                    if (!reader.HasRows)
                        return EMPTY_RESULT;
                    var result = new List<object[]>();
                    while (reader.Read())
                    {
                        var row = new object[reader.FieldCount];
                        for (var i = reader.FieldCount - 1; i >= 0; --i)
                        {
                            row[i] = reader.GetValue(i);
                        }
                        result.Add(row);
                    }
                    return new SQLiteResult { rows = result.ToArray(), columns = columns };
                }
                else if (readOnly)
                {
                    throw new SqliteException("trying to write when readOnly is set");
                }
                else if (startsWithCaseInsensitive(query, "insert"))
                {
                    var rowId = cmd.ExecuteScalar();
                    return new SQLiteResult { rows = EMPTY_ROWS, columns = EMPTY_COLUMNS, rowsAffected = 1, insertId = (long?)rowId };
                }
                else
                {
                    var rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected < 0)
                        rowsAffected = 0;
                    return new SQLiteResult { rows = EMPTY_ROWS, columns = EMPTY_COLUMNS, rowsAffected = rowsAffected };
                }
            }
        }
    }


}
