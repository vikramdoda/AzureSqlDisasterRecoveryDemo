using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSqlDisasterRecovery.DataService
{
    public class SqlUsedSpace
    {
        public string name { get; set; }

        public string rows { get; set; }

        public string data { get; set; }

        public string reserved { get; set; }

        public string index_size { get; set; }

        public string unused { get; set; }
    }

    public enum StorageMode
    {
        ALL,
        REMOTE_ONLY,
        LOCAL_ONLY
    }
}
