using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSqlDisasterRecovery.DataService
{
    public class StretchDbService
    {
        public static string GetRemoteTableName(string localTableName, string onPremiseDbCnnectionString, string remoteDatabaseName)
        {
            string remoteTableName = null;
            string query = @"select* from (
                            select distinct rdad.remote_database_name RemoteDatabaseName, ss.name + '.' + sb.name as LocalTableName, remote_table_name RemoteTableName from sys.remote_data_archive_tables rdat
                            join sys.objects sb on rdat.object_id = sb.object_id
                            join sys.schemas ss on ss.schema_id = sb.schema_id
                            join sys.remote_data_archive_databases rdad on rdad.remote_database_id = rdat.remote_database_id
                            ) a where a.LocalTableName = @localTableName and RemoteDatabaseName = @remoteDatabaseName";

            SqlDataReader rdr = null;
            SqlConnection conn = new SqlConnection(onPremiseDbCnnectionString);

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add(new SqlParameter("@localTableName", localTableName));
            cmd.Parameters.Add(new SqlParameter("@remoteDatabaseName", remoteDatabaseName));

            try
            {
                conn.Open();
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    remoteTableName = rdr["RemoteTableName"].ToString();
                }
            }
            catch(Exception ex)
            {
                return null;
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }

                if (conn != null)
                {
                    conn.Close();
                }
            }
            return remoteTableName;

        }
        public static DataSet GetTableData(string tableName, string connectionString)
        {
            DataSet ds = new DataSet();

            SqlConnection conn = new SqlConnection(connectionString);

           tableName = tableName.Replace(".", "].[");
            try
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter("Select top 10 * from [" + tableName+"]", conn);

                SqlCommandBuilder cmdBldr = new SqlCommandBuilder(da);

                da.Fill(ds);
            }
            catch (Exception ex)
            {
                return ds;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return ds;
        }
        public static SqlUsedSpace GetUsedSpace(string tableName, string connectionString, StorageMode storageMode)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;
            try
            {
                conn = new
                    SqlConnection(connectionString);
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_spaceused", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@objname", tableName));
                cmd.Parameters.Add(new SqlParameter("@mode", storageMode.ToString()));
                rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {

                    return new SqlUsedSpace
                    {
                        name = rdr["name"].ToString(),
                        rows = rdr["rows"].ToString(),
                        data = rdr["data"].ToString(),
                        reserved = rdr["reserved"].ToString(),
                        index_size = rdr["index_size"].ToString(),
                        unused = rdr["unused"].ToString()
                    };
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }

            return null;
        }

        public static string GetDatabaseName(string connectionString)
        {
            if(connectionString.Contains("metadata"))
            {
                connectionString = new EntityConnectionStringBuilder(connectionString).ProviderConnectionString;
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);

            return builder.InitialCatalog;
        }

        public static DataSet GetTableDataWithPaging(string tableName, string connectionString, string sidx, string sord, int page, int rows, out int totalRecords)
        {
            totalRecords = 100;
            DataSet ds = new DataSet();

            SqlConnection conn = new SqlConnection(connectionString);

            tableName = tableName.Replace(".", "].[");
            try
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter("Select top 10 * from [" + tableName + "]", conn);

                SqlCommandBuilder cmdBldr = new SqlCommandBuilder(da);

                da.Fill(ds);
            }
            catch (Exception ex)
            {
                return ds;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return ds;
        }

    }
}
