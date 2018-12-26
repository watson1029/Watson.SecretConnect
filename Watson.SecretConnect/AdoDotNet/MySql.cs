using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watson.SecretConnect.AdoDotNet
{
    public class MySql : SecretAdoDotNet
    { 
        #region 构造函数
        /// <summary>
        /// 设置数据库连接字符串(默认ConnectionStrings.Name=ConnectionString)
        /// </summary>
        public MySql() : base("name=ConnectionString") { }

        /// <summary>
        /// 设置数据库连接字符串
        /// </summary>
        /// <param name="nameOrConnectionString"></param>
        public MySql(string nameOrConnectionString) : base(nameOrConnectionString) { }
        #endregion 构造函数

        #region 私有方法

        private void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (MySqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        #endregion

        #region 公用方法

        /// <summary>
        /// 获取对应字段的当前最大值+1(Int)
        /// </summary>
        /// <param name="FieldName">字段名</param>
        /// <param name="TableName">表名</param>
        /// <returns>最大值+1</returns>
        public int GetMaxID(string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = GetSingle(strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }

        /// <summary>
        /// 判断MySql语句是否能返回有效值
        /// </summary>
        /// <param name="strMySql">查询语句</param>
        /// <returns>是否返回有效值</returns>
        public bool Exists(string strMySql)
        {
            object obj = GetSingle(strMySql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 判断MySql语句是否能返回有效值
        /// </summary>
        /// <param name="strMySql">查询语句</param>
        /// <returns>是否返回有效值</returns>
        public bool Exists(string strMySql, params MySqlParameter[] cmdParms)
        {
            object obj = GetSingle(strMySql, cmdParms);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region  执行简单MySql语句

        /// <summary>
        /// 执行MySql语句，返回影响的记录数
        /// </summary>
        /// <param name="MySqlString">MySql语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteMySql(string MySqlString)
        {
            using (MySqlConnection connection = new MySqlConnection(connectString))
            {
                using (MySqlCommand cmd = new MySqlCommand(MySqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (MySqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行带一个存储过程参数的的MySql语句。
        /// </summary>
        /// <param name="MySqlString">MySql语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteMySql(string MySqlString, string content)
        {
            using (MySqlConnection connection = new MySqlConnection(connectString))
            {
                MySqlCommand cmd = new MySqlCommand(MySqlString, connection);
                MySqlParameter myParameter = new MySqlParameter("@content", SqlDbType.NText);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (MySqlException e)
                {
                    throw e;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行MySql语句，返回影响的记录数
        /// </summary>
        /// <param name="MySqlString">MySql语句</param>
        /// <param name="Times">响应时间</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteMySql(string MySqlString, int Times)
        {
            using (MySqlConnection connection = new MySqlConnection(connectString))
            {
                using (MySqlCommand cmd = new MySqlCommand(MySqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (MySqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条MySql语句，实现数据库事务。
        /// </summary>
        /// <param name="MySqlStringList">多条MySql语句</param>		
        public int ExecuteMySqlTran(List<String> MySqlStringList)
        {
            using (MySqlConnection conn = new MySqlConnection(connectString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                MySqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    int count = 0;
                    for (int n = 0; n < MySqlStringList.Count; n++)
                    {
                        string strsql = MySqlStringList[n];
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            count += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return count;
                }
                catch
                {
                    tx.Rollback();
                    return 0;
                }
            }
        }

        /// <summary>
        /// 执行带一个存储过程参数的的MySql语句。
        /// </summary>
        /// <param name="MySqlString">MySql语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public object ExecuteMySqlGet(string MySqlString, string content)
        {
            using (MySqlConnection connection = new MySqlConnection(connectString))
            {
                MySqlCommand cmd = new MySqlCommand(MySqlString, connection);
                MySqlParameter myParameter = new MySqlParameter("@content", SqlDbType.NText);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    object obj = cmd.ExecuteScalar();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (MySqlException e)
                {
                    throw e;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="strMySql">MySql语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteMySqlInsertImg(string strMySql, byte[] fs)
        {
            using (MySqlConnection connection = new MySqlConnection(connectString))
            {
                MySqlCommand cmd = new MySqlCommand(strMySql, connection);
                MySqlParameter myParameter = new MySqlParameter("@fs", SqlDbType.Image);
                myParameter.Value = fs;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (MySqlException e)
                {
                    throw e;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="MySqlString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string MySqlString)
        {
            using (MySqlConnection connection = new MySqlConnection(connectString))
            {
                using (MySqlCommand cmd = new MySqlCommand(MySqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (MySqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="MySqlString">计算查询结果语句</param>
        /// <param name="Times">响应时间</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string MySqlString, int Times)
        {
            using (MySqlConnection connection = new MySqlConnection(connectString))
            {
                using (MySqlCommand cmd = new MySqlCommand(MySqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (MySqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
        /// </summary>
        /// <param name="strMySql">查询语句</param>
        /// <returns>MySqlDataReader</returns>
        public MySqlDataReader ExecuteReader(string strMySql)
        {
            MySqlConnection connection = new MySqlConnection(connectString);
            MySqlCommand cmd = new MySqlCommand(strMySql, connection);
            try
            {
                connection.Open();
                MySqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (MySqlException e)
            {
                throw e;
            }

        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="MySqlString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string MySqlString)
        {
            using (MySqlConnection connection = new MySqlConnection(connectString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    MySqlDataAdapter command = new MySqlDataAdapter(MySqlString, connection);
                    command.Fill(ds, "ds");
                }
                catch (MySqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                return ds;
            }
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="MySqlString">查询语句</param>
        /// <param name="Times">响应时间</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string MySqlString, int Times)
        {
            using (MySqlConnection connection = new MySqlConnection(connectString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    MySqlDataAdapter command = new MySqlDataAdapter(MySqlString, connection);
                    command.SelectCommand.CommandTimeout = Times;
                    command.Fill(ds, "ds");
                }
                catch (MySqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                return ds;
            }
        }



        #endregion

        #region 执行带参数的MySql语句

        /// <summary>
        /// 执行MySql语句，返回影响的记录数
        /// </summary>
        /// <param name="MySqlString">MySql语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteMySql(string MySqlString, params MySqlParameter[] cmdParms)
        {
            using (MySqlConnection connection = new MySqlConnection(connectString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, MySqlString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (MySqlException e)
                    {
                        throw e;
                    }
                }
            }
        }


        /// <summary>
        /// 执行多条MySql语句，实现数据库事务。
        /// </summary>
        /// <param name="MySqlStringList">MySql语句的哈希表（key为MySql语句，value是该语句的MySqlParameter[]）</param>
        public void ExecuteMySqlTran(Hashtable MySqlStringList)
        {
            using (MySqlConnection conn = new MySqlConnection(connectString))
            {
                conn.Open();
                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    MySqlCommand cmd = new MySqlCommand();
                    try
                    {
                        //循环
                        foreach (DictionaryEntry myDE in MySqlStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="MySqlString">计算查询结果语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string MySqlString, params MySqlParameter[] cmdParms)
        {
            using (MySqlConnection connection = new MySqlConnection(connectString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, MySqlString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (MySqlException e)
                    {
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
        /// </summary>
        /// <param name="strMySql">查询语句</param>
        /// <returns>MySqlDataReader</returns>
        public MySqlDataReader ExecuteReader(string strMySql, params MySqlParameter[] cmdParms)
        {
            MySqlConnection connection = new MySqlConnection(connectString);
            MySqlCommand cmd = new MySqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, strMySql, cmdParms);
                MySqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (MySqlException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="MySqlString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string MySqlString, params MySqlParameter[] cmdParms)
        {
            using (MySqlConnection connection = new MySqlConnection(connectString))
            {
                MySqlCommand cmd = new MySqlCommand();
                PrepareCommand(cmd, connection, null, MySqlString, cmdParms);
                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (MySqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds;
                }
            }
        }
        #endregion
    }
}
