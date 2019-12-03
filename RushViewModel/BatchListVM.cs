using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using RushDAL;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RushViewModel
{
    public class BatchListVM
    {
        public ConcurrentDictionary<string, List<string>> batches { get; set; }
        public DateTime fromDate;
        public DateTime toDate;

        public BatchListVM(DateTime fromdt, DateTime todt)
        {
            batches = new ConcurrentDictionary<string, List<string>>();
            fromDate = fromdt;
            toDate = todt;
        }
        public void Load()
        {
            string results = RushDAL.SearchDAL.SearchbyDate(fromDate, toDate);
            List<BatchResults> batchSearch = JsonConvert.DeserializeObject<List<BatchResults>>(results);

            Parallel.ForEach(batchSearch, item => {
                if ((batches.ContainsKey(item.sessionid)))
                {
                    batches[item.sessionid].Add(item.sampleid);
                }
                else
                {
                    List<string> samples = new List<string>();
                    samples.Add(item.sampleid);
                    batches.TryAdd(item.sessionid, samples);
                }
            });
        }
    }
    public class BatchResults
    {
        public string sessionid { get; set; }
        public string sampleid { get; set; }
        public DateTime runDate { get; set; }
    }
}
