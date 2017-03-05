using AzureSqlDisasterRecovery.DataService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace AzureSqlDisasterRecoveryDemo.Models
{
    public class HomeModel
    {
        public HomeModel()
        {
           Data = new DataSet();
        }

        public DataSet Data { get; set; }

        public bool IsDatabaseOnline { get; set; }

        public string StretchTableName { get; set; }

        public SqlUsedSpace TotalSpaceUsed { get; set; }
        public SqlUsedSpace LocalSpaceUsed { get; set; }
        public SqlUsedSpace RemoteSpaceUsed { get; set; }
        
        public string AlwaysOnPrimaryDatabaseServer { get; set; }

        public string AlwaysOnSecondaryDatabaseServer { get; set; }

        public bool StretchEnabled { get; set; }
    }
}