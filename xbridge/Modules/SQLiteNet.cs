using System;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using System.Collections.Generic;

namespace xbridge.Modules
{


    public class SQLiteNet
    {
        private XBridge bridge;


        public SQLiteNet(XBridge bridge)
        {
            this.bridge = bridge;
        }

        private Dictionary<string, SQLiteConnection> dbs = new Dictionary<string, SQLiteConnection>();
        private static Object[][] EMPTY_ROWS = new Object[][]{};
        private static String[] EMPTY_COLUMNS = new String[]{};
        private static readonly SQLiteResult EMPTY_RESULT = new SQLiteResult {rows = EMPTY_ROWS, columns = EMPTY_COLUMNS};

        public SQLiteConnection _GetDatabase(string name){

            lock (dbs)
            {
                if (!dbs.ContainsKey(name))
                {
                    var core = bridge.GetModule<Files>();
                    var dir = core.DataDir();
                    var path = dir + "/" + name;
                    return dbs[name] = new SQLiteConnection("Data Source=" + path + ";Version=3;");

                }
                else
                    return dbs[name];
            }
        }

        public async Task<object> ExecMany(String db, string[] queries, object[][] parameters, bool readOnly)
        {
            var dbc = _GetDatabase(db);
            Task<SQLiteResult>[] tasks = new Task<SQLiteResult>[queries.Length];
            for (var i = 0; i < queries.Length; i++)
            {
                var q = queries[i];
                object[] p = null;
                if (parameters != null)
                    p = parameters[i];
                tasks[i] = _ExecAndCatch(dbc, q, p, readOnly);
            }
            SQLiteResult[] results = await Task.WhenAll(tasks);
            return results;
        }

        private async Task<SQLiteResult> _ExecAndCatch(SQLiteConnection dbc, string q, object[] p, bool readOnly)
        {

            try
            {
                return await _Exec(dbc, q, p, readOnly);
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

        public async Task<SQLiteResult> Exec(String db, string query, object[] parameters, bool readOnly)
        {
            var dbc = _GetDatabase(db);
            var result = await _Exec(dbc, query, parameters, readOnly);
            //return new object[]{result.rows, result.columns, result.rowsAffected, result.error};
            return result;
        }
        public async Task<SQLiteResult> _Exec(SQLiteConnection dbc, string query, object[] parameters, bool readOnly)
        {

                if (parameters != null)
                {
                    var len = parameters.Length;
                    for (var i = 0; i < len; ++i)
                    {
                        cmd.Bind(parameters[i]);
                    }
                }
                if (startsWithCaseInsensitive(query, "select"))
                {
                var reader = dbc.Query(query, parameters);
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
                else if(readOnly)
                {
                    throw new SQLiteException("trying to write when readOnly is set");
                }   
                else if (startsWithCaseInsensitive(query, "insert"))
                {
                    var rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected < 0)
                        rowsAffected = 0;
                    return new SQLiteResult { rows = EMPTY_ROWS, columns = EMPTY_COLUMNS, rowsAffected = rowsAffected, insertId = dbc.LastInsertRowId };
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
