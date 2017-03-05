using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace AzureSqlDisasterRecoveryDemo.Models
{
    public static class Constants
    {
        public static string StretchDbOnPremiseConnectionString { get { return ConfigurationManager.ConnectionStrings["StretchDBOnPremiseConnection"].ConnectionString; } }
        public static string StretchDBAzureConnectionString { get { return ConfigurationManager.ConnectionStrings["StretchDBAzureConnection"].ConnectionString; } }
        public static string StretchTableName { get { return ConfigurationManager.AppSettings["StretchTableName"]; } }
        public static string SqlAlwaysOnPrimaryConnectionString { get { return ConfigurationManager.ConnectionStrings["AdventureworksDW2016CTP3EntitiesPrimary"].ConnectionString; } }
        public static string SqlAlwaysOnSecondaryConnectionString { get { return ConfigurationManager.ConnectionStrings["AdventureworksDW2016CTP3EntitiesSecondary"].ConnectionString; } }
        public static string SQLAlwaysOnConnectionString { get { return ConfigurationManager.ConnectionStrings["SQLAlwaysOnConnection"].ConnectionString; } }

    }
}