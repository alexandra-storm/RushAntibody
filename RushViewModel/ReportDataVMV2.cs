using System;
using System.Configuration;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using RushDAL;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RushViewModel
{
    public class ReportDataVMV2
    {
        public DataSet reportDS { get; set; }
        public ReportData reportdb { get; set; }
        public string sessionid { get; set; }
        public string sampleid { get; set; }
        public string classtype { get; set; }

        public void LoadData()
        {
            reportDS = ReportDAL.GetReportData(sessionid, sampleid, classtype);
        }

        public void GetAllPositiveAntigens()
        {
            var pos = (from item in reportDS.Tables[1].AsEnumerable() where decimal.Parse(item["PctPositive"].ToString()) == 100m select item).ToList();
            foreach(DataRow dr in pos)
            {
                if (dr["antigen"].ToString().StartsWith("A"))
                {
                    var bRank = (from bead in reportDS.Tables[0].AsEnumerable() where bead["AlleleColA"].ToString() == dr["antigen"].ToString() select bead);
                    
                }
            }
        }

        

    }

    public class AAntigens
    {
        public string name { get; set; }
        public decimal mfi { get; set; }
        public string serology { get; set; }
        public bool call { get; set; }
        public int sortOrder { get; set; }
    }
}
