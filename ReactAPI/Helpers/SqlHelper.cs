using System.Data;
using System.Data.SqlClient;

namespace WebAPI.Helpers;

public class SqlHelper
{

    private string mstr_ConnectionString;
    private SqlConnection mobj_SqlConnection;
    private SqlCommand mobj_SqlCommand;
    private int mint_CommandTimeout = 30;

    public SqlHelper(IConfiguration configuration)
    {
        try
        {
            mstr_ConnectionString = configuration.GetConnectionString("EmployeeAppCon");
            mobj_SqlConnection = new SqlConnection(mstr_ConnectionString);
            mobj_SqlCommand = new SqlCommand();
            mobj_SqlCommand.CommandTimeout = mint_CommandTimeout;
            mobj_SqlCommand.Connection = mobj_SqlConnection;

            //ParseConnectionString();
        }
        catch (Exception ex)
        {
            throw new Exception("Error initializing data class." + Environment.NewLine + ex.Message);
        }
    }

    public void Dispose()
    {
        try
        {
            //Clean Up Connection Object
            if (mobj_SqlConnection != null)
            {
                if (mobj_SqlConnection.State != ConnectionState.Closed)
                {
                    mobj_SqlConnection.Close();
                }
                mobj_SqlConnection.Dispose();
            }

            //Clean Up Command Object
            if (mobj_SqlCommand != null)
            {
                mobj_SqlCommand.Dispose();
            }

        }

        catch (Exception ex)
        {
            throw new Exception("Error disposing data class." + Environment.NewLine + ex.Message);
        }

    }

    public void CloseConnection()
    {
        if (mobj_SqlConnection.State != ConnectionState.Closed) mobj_SqlConnection.Close();
    }

    public void OpenConnection()
    {
        if (mobj_SqlConnection.State != ConnectionState.Open) mobj_SqlConnection.Open();
    }

    public int GetExecuteScalarByCommand(string Command)
    {

        object identity;
        try
        {
            mobj_SqlCommand.CommandText = Command;
            mobj_SqlCommand.CommandTimeout = mint_CommandTimeout;
            mobj_SqlCommand.CommandType = CommandType.StoredProcedure;

            OpenConnection();

            mobj_SqlCommand.Connection = mobj_SqlConnection;
            identity = mobj_SqlCommand.ExecuteScalar();
            CloseConnection();
        }
        catch (Exception)
        {
            CloseConnection();
            throw;
        }
        return Convert.ToInt32(identity);
    }

    public async Task<int> GetExecuteScalarByStr(string sql)
    {


        object? identity;
        try
        {
            SqlCommand cmd = new SqlCommand(sql);
            cmd.CommandType = CommandType.Text;

            OpenConnection();

            cmd.Connection = mobj_SqlConnection;
            identity = await cmd.ExecuteScalarAsync();
            //CloseConnection();
        }
        catch (Exception)
        {
            CloseConnection();
            throw;
        }
        return Convert.ToInt32(identity);
    }

    public void GetExecuteNonQueryByCommand(string Command, SqlParameter[]? parameters = null)
    {
        try
        {
            mobj_SqlCommand.CommandText = Command;
            mobj_SqlCommand.CommandTimeout = mint_CommandTimeout;
            mobj_SqlCommand.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                mobj_SqlCommand.Parameters.AddRange(parameters);
            }
            OpenConnection();

            mobj_SqlCommand.Connection = mobj_SqlConnection;
            mobj_SqlCommand.ExecuteNonQuery();

            CloseConnection();
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            CloseConnection();
        }
    }

    public DataSet GetDatasetByCommand(string Command, SqlParameter[]? parameters = null)
    {
        try
        {
            mobj_SqlCommand.CommandText = Command;
            mobj_SqlCommand.CommandTimeout = mint_CommandTimeout;
            mobj_SqlCommand.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                mobj_SqlCommand.Parameters.AddRange(parameters);
            }
            OpenConnection();

            SqlDataAdapter adpt = new SqlDataAdapter(mobj_SqlCommand);
            DataSet ds = new DataSet();
            adpt.Fill(ds);
            ds.Tables[0].TableName = "data";
            return ds;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            CloseConnection();
        }
    }

    public async Task<SqlDataReader> GetReaderBySQL(string strSQL)
    {
        OpenConnection();
        try
        {
            SqlCommand myCommand = new SqlCommand(strSQL, mobj_SqlConnection);
            return await myCommand.ExecuteReaderAsync();
        }
        catch (Exception)
        {
            CloseConnection();
            throw;
        }
        finally
        {
            ;//CloseConnection();
        }
    }

    public SqlDataReader GetReaderByCmd(string Command)
    {
        SqlDataReader objSqlDataReader;
        try
        {
            mobj_SqlCommand.CommandText = Command;
            mobj_SqlCommand.CommandType = CommandType.StoredProcedure;
            mobj_SqlCommand.CommandTimeout = mint_CommandTimeout;

            OpenConnection();
            mobj_SqlCommand.Connection = mobj_SqlConnection;

            objSqlDataReader = mobj_SqlCommand.ExecuteReader();
            return objSqlDataReader;
        }
        catch (Exception)
        {
            CloseConnection();
            throw;
        }

    }
}
