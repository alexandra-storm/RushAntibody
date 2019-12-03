using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using EvolutionDBFacade;
using Newtonsoft.Json;

namespace RushDAL
{
    public static class SearchDAL
    {
        /// <summary>
        /// search for run date between the to and from date parameters
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static string SearchbyDate(DateTime fromdt, DateTime todt)
        {
            EvolutionDBFacade.EvolutionDBFacade.DBFacade dBFacade = new EvolutionDBFacade.EvolutionDBFacade.DBFacade();
            string returnValue;

            using (AbDALDataContext abDAL = new AbDALDataContext(dBFacade.GetConn()))
            {
                List<BatchResults> dr = (from item in abDAL.tbAntibodyMethods
                          where item.runDate >= fromdt
                          && item.runDate < todt
                          select new BatchResults {
                              sessionid = item.sessionID,
                              sampleid = item.sampleID,
                              runDate = item.runDate }).ToList();
                returnValue = JsonConvert.SerializeObject(dr);
            }
            return returnValue;
        }
    }
    public class BatchResults
    {
        public string sessionid { get; set; }
        public string sampleid { get; set; }
        public DateTime? runDate { get; set; }
    }
}
