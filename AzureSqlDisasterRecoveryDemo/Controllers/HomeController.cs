using AzureSqlDisasterRecovery.DataService;
using AzureSqlDisasterRecoveryDemo.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzureSqlDisasterRecoveryDemo.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View(GetHomeModel());
        }

        private HomeModel GetHomeModel(FormCollection formCollection = null)
        {
            var model = new Models.HomeModel();

            model.StretchTableName = Constants.StretchTableName;
            string remoteDatabaseName = StretchDbService.GetDatabaseName(Constants.StretchDBAzureConnectionString);
            string remoteTableName = StretchDbService.GetRemoteTableName(Constants.StretchTableName, Constants.StretchDbOnPremiseConnectionString , remoteDatabaseName);
            model.StretchEnabled = !string.IsNullOrWhiteSpace(remoteTableName);
            model.IsDatabaseOnline = SqlAlwaysOnService.IsServerConnected(Constants.SQLAlwaysOnConnectionString);

            if (SqlAlwaysOnService.IsPrimaryDatabase(Constants.SqlAlwaysOnPrimaryConnectionString))
            {
                model.AlwaysOnPrimaryDatabaseServer = SqlAlwaysOnService.GetServerName(Constants.SqlAlwaysOnPrimaryConnectionString);
                model.AlwaysOnSecondaryDatabaseServer = SqlAlwaysOnService.GetServerName(Constants.SqlAlwaysOnSecondaryConnectionString); ;
            }
            else
            {
                model.AlwaysOnPrimaryDatabaseServer = SqlAlwaysOnService.GetServerName(Constants.SqlAlwaysOnSecondaryConnectionString);
                model.AlwaysOnSecondaryDatabaseServer = SqlAlwaysOnService.GetServerName(Constants.SqlAlwaysOnPrimaryConnectionString); ;
            }

            model.TotalSpaceUsed = StretchDbService.GetUsedSpace(Constants.StretchTableName, Constants.StretchDbOnPremiseConnectionString, StorageMode.ALL);
            model.RemoteSpaceUsed = StretchDbService.GetUsedSpace(Constants.StretchTableName, Constants.StretchDbOnPremiseConnectionString, StorageMode.REMOTE_ONLY);
            model.LocalSpaceUsed = StretchDbService.GetUsedSpace(Constants.StretchTableName, Constants.StretchDbOnPremiseConnectionString, StorageMode.LOCAL_ONLY);

            return model;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public JsonResult GetAccounts(string store, string sidx, string sord, int page, int rows)
        {
            int totalRecords;

            bool isServerOnline = SqlAlwaysOnService.IsServerConnected(Constants.SQLAlwaysOnConnectionString);
            string primaryConnectionString = string.Empty;
            string secondaryConnectionString = string.Empty;
            if (isServerOnline)
            {
                if (SqlAlwaysOnService.IsPrimaryDatabase(Constants.SqlAlwaysOnPrimaryConnectionString))
                {
                    primaryConnectionString = Constants.SqlAlwaysOnPrimaryConnectionString;
                    secondaryConnectionString = Constants.SqlAlwaysOnSecondaryConnectionString;
                }
                else
                {
                    primaryConnectionString = Constants.SqlAlwaysOnSecondaryConnectionString;
                    secondaryConnectionString = Constants.SqlAlwaysOnPrimaryConnectionString;
                }
            }

            string connectionString = !string.IsNullOrEmpty(store) && store == "Secondary" ? secondaryConnectionString : primaryConnectionString;

            var accounts = SqlAlwaysOnService.GetAccounts(connectionString, sidx, sord, page, rows, out totalRecords);

            var totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);

            var jsonData = new
            {
                total = totalPages,
                page,
                records = totalRecords,
                rows = accounts
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public string EditAccount(string store, int id, DimAccount model)
        {
            bool isServerOnline = SqlAlwaysOnService.IsServerConnected(Constants.SQLAlwaysOnConnectionString);
            string primaryConnectionString = string.Empty;
            string secondaryConnectionString = string.Empty;
            if (isServerOnline)
            {
                if (SqlAlwaysOnService.IsPrimaryDatabase(Constants.SqlAlwaysOnPrimaryConnectionString))
                {
                    primaryConnectionString = Constants.SqlAlwaysOnPrimaryConnectionString;
                    secondaryConnectionString = Constants.SqlAlwaysOnSecondaryConnectionString;
                }
                else
                {
                    primaryConnectionString = Constants.SqlAlwaysOnSecondaryConnectionString;
                    secondaryConnectionString = Constants.SqlAlwaysOnPrimaryConnectionString;
                }
            }

            string connectionString = !string.IsNullOrEmpty(store) && store == "Secondary" ? secondaryConnectionString : primaryConnectionString;

            return SqlAlwaysOnService.UpdateAccount(connectionString, id, model);
        }

        public JsonResult GetTableData(string location)
        {
            int totalRecords;
            string tableName = Constants.StretchTableName;
            string connectionString = !string.IsNullOrEmpty(location) && location.Trim() == "Azure" ? Constants.StretchDBAzureConnectionString : Constants.StretchDbOnPremiseConnectionString;

            if(!string.IsNullOrEmpty(location) && location.Trim() == "Azure")
            {
                string remoteDatabaseName = StretchDbService.GetDatabaseName(Constants.StretchDBAzureConnectionString);
                tableName = StretchDbService.GetRemoteTableName(tableName, Constants.StretchDbOnPremiseConnectionString, remoteDatabaseName);
            }
            var dataset = StretchDbService.GetTableData(tableName, connectionString);
           
            string rowData = "[]";

            if(dataset != null && dataset.Tables.Count>0)
            {
                rowData=dataset.Tables[0].ToJson();
            }
            return Json(rowData, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}