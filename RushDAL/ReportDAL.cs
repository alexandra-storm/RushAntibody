using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using EvolutionDBFacade;
using Newtonsoft.Json;

namespace RushDAL
{
    public static class ReportDAL
    {
        public static DataSet GetReportData(string session, string sample, string ClassValue)
        {
            EvolutionDBFacade.EvolutionDBFacade.DBFacade dBFacade = new EvolutionDBFacade.EvolutionDBFacade.DBFacade();
            DataSet dr;
            using (AbDALDataContext abDAL = new AbDALDataContext(dBFacade.GetConn()))
            {
                var conn = dBFacade.GetConn();
                try
                {
                    Dictionary<string, object> param = new Dictionary<string, object>();
                    param.Add("batchID", session);
                    param.Add("sampleID", sample);
                    dr = dBFacade.ExecuteProc("GetSampleForLoad", param);
                }
                finally
                {
                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }                
            }
            return dr;
        }
        public static List<string> GetSortedAntigens(List<string> listofAntigens)
        {
            StringBuilder sb = new StringBuilder();
            List<string> ret = new List<string>();
            DataSet dr;
            EvolutionDBFacade.EvolutionDBFacade.DBFacade dBFacade = new EvolutionDBFacade.EvolutionDBFacade.DBFacade();
            foreach(string item in listofAntigens)
            {
                sb.Append($"'{item}',");
            }
            if (sb.ToString().Length > 0)
            {
                using (AbDALDataContext abDAL = new AbDALDataContext(dBFacade.GetConn()))
                {
                    var conn = dBFacade.GetConn();
                    try
                    {
                        string query = $"Select AntigenID,AntigenName,AntigenSortOrder from tbAntigens where  antigenName in ({sb.ToString().Remove(sb.ToString().Length - 1)}) order by AntigenSortOrder";
                        dr = dBFacade.ExecuteSQL(query);
                    }
                    finally
                    {
                        if (conn.State != ConnectionState.Closed)
                            conn.Close();
                    }
                }

                foreach (DataRow row in dr.Tables[0].Rows)
                {
                    ret.Add(row["AntigenName"].ToString());
                }

            }
            return ret;
        }
    }
}
