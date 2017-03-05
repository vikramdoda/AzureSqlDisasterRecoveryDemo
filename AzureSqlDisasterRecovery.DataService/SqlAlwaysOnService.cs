using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSqlDisasterRecovery.DataService
{
    public class SqlAlwaysOnService
    {
        public static bool IsServerConnected(string connectionString)
        {
            using (var l_oConnection = new SqlConnection(connectionString))
            {
                try
                {
                    l_oConnection.Open();
                    return true;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
        }

        public static bool IsPrimaryDatabase(string connectionString)
        {
            try
            {
                if (connectionString.Contains("metadata"))
                {
                    connectionString = new EntityConnectionStringBuilder(connectionString).ProviderConnectionString;
                }

                using (var conn = new SqlConnection(connectionString))
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT role FROM sys.dm_geo_replication_link_status";
                        var res = cmd.ExecuteScalar();
                        return Convert.ToInt32(res) == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                //handle exception
                return false;
            }
        }

        public static List<DimAccount> GetAccounts(string connectionString, string sidx, string sort, int page, int rows, out int totalRecords)
        {
            sort = (sort == null) ? "" : sort;
            int pageIndex = Convert.ToInt32(page) - 1;
            int pageSize = rows;

            AdventureworksDW2016CTP3Entities entities = new AdventureworksDW2016CTP3Entities(connectionString);

            totalRecords = entities.DimAccounts.Count();

            if (sort.ToUpper() == "DESC")
            {
                if (string.IsNullOrEmpty(sidx))
                {
                    return entities.DimAccounts.OrderByDescending(x => x.AccountDescription).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                }
                return entities.DimAccounts.OrderByDescending(sidx).Skip(pageIndex * pageSize).Take(pageSize).ToList();
            }
            else
            {
                if (string.IsNullOrEmpty(sidx))
                {
                    return entities.DimAccounts.OrderBy(x => x.AccountDescription).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                }
                return entities.DimAccounts.OrderBy(sidx).Skip(pageIndex * pageSize).Take(pageSize).ToList();
            }
        }

        public static string GetServerName(string connectionString)
        {
            if (connectionString.Contains("metadata"))
            {
                connectionString = new EntityConnectionStringBuilder(connectionString).ProviderConnectionString;
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            return builder.DataSource;
        }

        public static string UpdateAccount(string connectionString, int id, DimAccount model)
        {
            using (AdventureworksDW2016CTP3Entities db = new AdventureworksDW2016CTP3Entities(connectionString))
            {
                string msg;
                try
                {
                    var account = db.DimAccounts.SingleOrDefault(x => x.AccountKey == id);

                    if (account != null)
                    {
                        account.AccountDescription = model.AccountDescription;
                        account.AccountType = model.AccountType;
                        db.SaveChanges();
                        msg = "Saved Successfully";
                    }
                    else
                    {
                        msg = "Unique record not found on server";
                    }
                }
                catch (DbUpdateException ex)
                {
                    bool isPrimary = IsPrimaryDatabase(connectionString);
                    if (!isPrimary)
                    {
                        msg = "Can not add or modify the records as secondary database is readonly.";
                    }
                    else
                    {
                        msg = "Error occured:" + ex.Message;
                    }
                }
                catch (Exception ex)
                {
                    msg = "Error occured:" + ex.Message;
                }
                return msg;
            }
        }
    }
}
