using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Aipuer.Common
{
    /// <summary>
    /// 数据库操作基类(for PostgreSQL)
    /// </summary>
    public class PostgreHelper : IDBHelper
    {
        private string connectionString;
        private string connString = System.Configuration.ConfigurationManager.ConnectionStrings["PgDBConnection_lpg"].ToString();

        public PostgreHelper()
        {
            this.connectionString = connString;
        }
        public PostgreHelper(string connString)
        {
            this.connectionString = connString;
        }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString
        {
            get { return this.connectionString; }
        }
        /// <summary>
        /// 得到数据条数
        /// </summary>
        public int GetCount(string cmdText)
        {
            StringBuilder sql = new StringBuilder(cmdText);
            object count = ExecuteScalar(CommandType.Text, sql.ToString(), null);
            if(count == null) {
                count = 0;
            }
            return int.Parse(count.ToString());
        }

        /// <summary>
        /// 执行查询，返回DataSet
        /// </summary>
        public DataSet ExecuteQuery(CommandType cmdType, string cmdText,
            params DbParameter[] cmdParms)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand())
                {
                    PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                        return ds;
                    }
                }
            }
        }

        /// <summary>
        /// 在事务中执行查询，返回DataSet
        /// </summary>
        public DataSet ExecuteQuery(DbTransaction trans, CommandType cmdType, string cmdText,
            params DbParameter[] cmdParms)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, cmdParms);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds, "ds");
            cmd.Parameters.Clear();
            return ds;
        }

        /// <summary>
        /// 执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        public int ExecuteNonQuery(CommandType cmdType, string cmdText,
            params DbParameter[] cmdParms)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        /// 在事务中执行 Transact-SQL 语句并返回受影响的行数。
        /// </summary>
        public int ExecuteNonQuery(DbTransaction trans, CommandType cmdType, string cmdText,
            params DbParameter[] cmdParms)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, cmdParms);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// 执行查询，返回DataReader
        /// </summary>
        public DbDataReader ExecuteReader(CommandType cmdType, string cmdText,
            params DbParameter[] cmdParms)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();
            NpgsqlConnection conn = new NpgsqlConnection(connectionString);

            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
                NpgsqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch
            {
                conn.Close();
                throw;
            }
        }

        /// <summary>
        /// 在事务中执行查询，返回DataReader
        /// </summary>
        public DbDataReader ExecuteReader(DbTransaction trans, CommandType cmdType, string cmdText,
            params DbParameter[] cmdParms)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, cmdParms);
            NpgsqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return rdr;
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        public object ExecuteScalar(CommandType cmdType, string cmdText,
            params DbParameter[] cmdParms)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, cmdParms);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }


        /// <summary>
        /// 在事务中执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        public object ExecuteScalar(DbTransaction trans, CommandType cmdType, string cmdText,
            params DbParameter[] cmdParms)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, cmdParms);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// 生成要执行的命令
        /// </summary>
        /// <remarks>参数的格式：冒号+参数名</remarks>
        private static void PrepareCommand(DbCommand cmd, DbConnection conn, DbTransaction trans, CommandType cmdType,
            string cmdText, DbParameter[] cmdParms)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                cmd.Connection = conn;
                cmd.CommandText = cmdText.Replace("@", ":").Replace("?", ":").Replace("[", "\"").Replace("]", "\"");

                if (trans != null)
                    cmd.Transaction = trans;

                cmd.CommandType = cmdType;

                if (cmdParms != null)
                {
                    foreach (NpgsqlParameter parm in cmdParms)
                    {
                        parm.ParameterName = parm.ParameterName.Replace("@", ":").Replace("?", ":");

                        cmd.Parameters.Add(parm);
                    }
                }
            }
            catch(Exception e)
            {

            }
        }

        /// <summary>
        /// 返回json格式的查询结果
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public string GetAsJson(CommandType cmdType, string cmdText, params DbParameter[] cmdParms)
        {
            string JsonString = string.Empty;
            DataSet ds = ExecuteQuery(cmdType, cmdText, cmdParms);
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                JsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            }
            return JsonString;
        }

        /// <summary>
        /// 得到查询的第一行的第一列
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public string GetOneValue(CommandType cmdType, string cmdText, params DbParameter[] cmdParms)
        {
            string JsonString = string.Empty;
            DataSet ds = ExecuteQuery(cmdType, cmdText, cmdParms);
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                JsonString = ds.Tables[0].Rows[0][0].ToString();
            }
            return JsonString;
        }

        /// <summary>
        /// 得到datatable
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public DataTable GetDataTable(CommandType cmdType, string cmdText, params DbParameter[] cmdParms)
        {
            DataSet ds = ExecuteQuery(cmdType, cmdText, cmdParms);
            if (ds == null || ds.Tables == null)
            {
                return null;
            }
            return ds.Tables[0];
        }


        //重写
        public int InsertReturnId(string safeSql)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))

            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand(safeSql, conn);
                object obj = cmd.ExecuteScalar();
                string objstr = obj != null ? obj.ToString() : string.Empty;
                return objstr == "" ? -1 : Int32.Parse(objstr);
            }


        }
        public void changeTable(string changeString)

        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))

            {
                try

                {

                    conn.Open();

                    NpgsqlCommand objCommand = new NpgsqlCommand(changeString, conn);

                    objCommand.ExecuteNonQuery();

                }

                catch (Exception e)

                {

                    Console.WriteLine(e.Message);

                }
            }
        }

        public string GetTableNames()
        {
            List<string> tables = new List<string>();
            DataSet ds = this.ExecuteQuery(CommandType.Text, "select tablename from pg_tables where schemaname = 'public';");
            DataTable tb= ds.Tables[0];
            foreach (DataRow row in tb.Rows)
            {
                tables.Add(row[0].ToString());
            }
            return "[\""+string.Join("\",\"",tables)+"\"]";
        }

        public string GetFieldNames(string tableName)
        {
            List<string> names = new List<string>();
            DataSet ds = this.ExecuteQuery(CommandType.Text, "SELECT A.attname AS colname FROM pg_class AS C,pg_attribute AS A WHERE C.relname = '" + tableName + "' AND A.attrelid = C.oid AND A.attnum > 0 and a.attname != 'geom'");
            DataTable tb = ds.Tables[0];
            foreach (DataRow row in tb.Rows)
            {
                names.Add(row[0].ToString());
            }
            return "[\"" + string.Join("\",\"", names) + "\"]";
        }

    }

}
