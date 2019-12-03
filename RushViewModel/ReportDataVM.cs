using System;
using System.Configuration;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using RushDAL;
using System.Linq;
using System.Xml;
using System.Text;
using System.Threading.Tasks;

namespace RushViewModel
{
    public class ReportDataVM
    {
        public string sessionid { get; set; }
        public string sampleid { get; set; }
        public string classtype { get; set; }
        public DataSet reportDS { get; set; }
        public ReportData reportdb { get; set; }
        public string highRange { get; set; }
        public string midRange { get; set; }
        public  string lowRange { get; set; }
        public string unacceptable { get; set; }
        
        public ReportDataVM(string batch, string sample, string classid)
        {
            sampleid = sample;
            sessionid = batch;
            classtype = classid;
            reportdb = new ReportData();
            reportdb.StrongAlleles = new List<string>();
            reportdb.StrongSerology = new List<string>();
            reportdb.ModAlleles = new List<string>();
            reportdb.ModSerology = new List<string>();
            reportdb.WeakAlleles = new List<string>();
            reportdb.WeakSerology = new List<string>();
            reportdb.UnacceptableSerology = new List<string>();
        }

        public void LoadMFIGroups()
        {
            string fileName = String.Format("{0}\\LIFECODES\\RUSHSettings.xml", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNodeList nodes;// = new XmlNodeList();
            nodes = doc.SelectNodes("/Settings");
            highRange = nodes.Item(0).SelectSingleNode("highrange").InnerText;
            midRange = nodes.Item(0).SelectSingleNode("midrange").InnerText;
            lowRange = nodes.Item(0).SelectSingleNode("lowrange").InnerText;
            unacceptable = nodes.Item(0).SelectSingleNode("unacceptable").InnerText;
        }
        public void BuildReportData()
        {
            LoadMFIGroups();
            int high = int.Parse(highRange);
            int midlow = int.Parse(midRange.Split('-')[0]);
            int midhigh = int.Parse(midRange.Split('-')[1]);
            int lowlow = int.Parse(lowRange.Split('-')[0]);
            int lowhight = int.Parse(lowRange.Split('-')[1]);
            int unacceptablelow = int.Parse(unacceptable);

            //columns rawValue, assignment, Serology,AlleleColA, AlleleColB, AlleleColC, AlleleBw,AlleleDRCol, AlleleDQACol, AlleleDQBCol, 
            //AlleleDPACol, AlleleDPBCol, A_AlleleSortOrder, A_serologySortOrder
            //B_AlleleSortOrder, B_SerologySortOrder, C_AlleleSortOrder, C_SerologySortOrder
            if (reportDS.Tables[0].Columns.Contains("Serology"))
            {

                reportdb.SampleID = sampleid;
                reportdb.SerumDate = DateTime.Parse(reportDS.Tables[6].Rows[0]["rundate"].ToString()).ToString("MM/dd/yyyy");
                Dictionary<string, List<decimal>> seroValuePairs = new Dictionary<string, List<decimal>>();
                Dictionary<string, List<decimal>> allValuePairs = new Dictionary<string, List<decimal>>();
                bool excludeBW4 = false;
                bool excludeBW6 = false;

                //check for 100% Bw4/Bw6
                var bw4 = (from b4 in reportDS.Tables[0].AsEnumerable() where b4["AlleleBw"].ToString() == "Bw4" && b4["assignment"].ToString() == "Negative" select b4).Any();
                var bw6 = (from b4 in reportDS.Tables[0].AsEnumerable() where b4["AlleleBw"].ToString() == "Bw6" && b4["assignment"].ToString() == "Negative" select b4).Any();

                excludeBW4 = bw4;
                excludeBW6 = bw6;
              
                foreach (DataRow dr in reportDS.Tables[0].Rows)
                {
                    decimal raw = decimal.Parse(dr["rawValue"].ToString());
                    string call = dr["assignment"].ToString();
                    string rawSerology = dr["serology"].ToString();

                    string serology = dr["serology"].ToString().Contains("(") ? dr["serology"].ToString().Split('(')[0] : dr["serology"].ToString();

                    string alleles = GetAlleleFromRow(dr);

                    if (alleles.Contains("C*"))
                    {
                        //need to look at the first 2 digits
                        if (int.Parse(alleles.Split('*')[1].Split(':')[0]) > 10)
                        {
                            serology = alleles;
                        }
                        else
                        {
                            serology = rawSerology.Contains("(") ? rawSerology.Split('(')[0] : rawSerology;
                        }
                    }
                    else
                    {
                        var currSero = (from ser in reportDS.Tables[0].AsEnumerable() where ser["Serology"].ToString() == rawSerology && ser["assignment"].ToString() == "Negative" select ser).Any();
                        if (currSero)
                        {
                            serology = alleles;
                        }
                        if (serology.Length == 0)
                        {
                            serology = alleles;
                        }
                    }
                    if (call.ToLower() == "positive")
                    {
                        if (seroValuePairs.ContainsKey(serology))
                        {
                            seroValuePairs[serology].Add(raw);
                        }
                        else
                        {
                            List<decimal> elem = new List<decimal>();
                            elem.Add(raw);
                            seroValuePairs.Add(serology, elem);
                        }
                        List<string> alleleList = alleles.Split(',').ToList<string>();
                        foreach (string i in alleleList)
                        {
                            if (allValuePairs.ContainsKey(i))
                            {
                                allValuePairs[i].Add(raw);
                            }
                            else
                            {
                                List<decimal> elem = new List<decimal>();
                                elem.Add(raw);
                                allValuePairs.Add(i, elem);
                            }
                        }
                        if (!excludeBW4)
                        {
                            var bw4a = (from i in reportDS.Tables[0].AsEnumerable() where i["AlleleBw"].ToString() == "Bw4" select i).ToList();
                            List<decimal> elem = new List<decimal>();
                            foreach (DataRow data in bw4a)
                            {
                                elem.Add(decimal.Parse(data["rawValue"].ToString()));
                            }
                            if (!allValuePairs.ContainsKey("Bw4"))
                            {
                                allValuePairs.Add("Bw4", elem);
                            }                            
                        }
                        if (!excludeBW6)
                        {
                            var bw6a = (from i in reportDS.Tables[0].AsEnumerable() where i["AlleleBw"].ToString() == "Bw6" select i).ToList();
                            List<decimal> elem = new List<decimal>();
                            foreach (DataRow data in bw6a)
                            {
                                elem.Add(decimal.Parse(data["rawValue"].ToString()));
                            }
                            if (!allValuePairs.ContainsKey("Bw6"))
                            {
                                allValuePairs.Add("Bw6", elem);
                            }                          
                        }
                    }
                }

                foreach (string s in seroValuePairs.Keys)
                {
                    decimal finalsero;
                    if (seroValuePairs[s].Count > 1)
                    {
                        //take the average of the values
                        finalsero = seroValuePairs[s].Average();
                    }
                    else
                    {
                        finalsero = seroValuePairs[s][0];
                    }
                    if (finalsero > high)
                    {
                        reportdb.StrongSerology.Add(s);
                    }
                    else if (finalsero > midlow && finalsero <= midhigh)
                    {
                        reportdb.ModSerology.Add(s);
                    }
                    else if (finalsero > lowlow && finalsero <= lowhight)
                    {
                        reportdb.WeakSerology.Add(s);
                    }
                    if (finalsero >= unacceptablelow)
                    {
                        reportdb.UnacceptableSerology.Add(s);
                    }
                }

                foreach (string a in allValuePairs.Keys)
                {
                    decimal finalallele;
                    if (a.Length > 0)
                    {
                        if (allValuePairs[a].Count > 1)
                        {
                            finalallele = allValuePairs[a].Average();
                        }
                        else
                        {
                            finalallele = allValuePairs[a][0];
                        }

                        if (finalallele > high)
                        {
                            reportdb.StrongAlleles.Add(a);
                        }
                        else if (finalallele > midlow && finalallele <= midhigh)
                        {
                            reportdb.ModAlleles.Add(a);
                        }
                        else if (finalallele > lowlow && finalallele <= lowhight)
                        {
                            reportdb.WeakAlleles.Add(a);
                        }
                        if((a == "Bw4" || a == "Bw6"))
                        {
                            if (finalallele >= unacceptablelow)
                            {
                                reportdb.UnacceptableSerology.Add(a);
                            }
                            if (finalallele > high)
                            {
                                reportdb.StrongSerology.Add(a);
                            }
                            else if (finalallele > midlow && finalallele <= midhigh)
                            {
                                reportdb.ModSerology.Add(a);
                            }
                            else if (finalallele > lowlow && finalallele <= lowhight)
                            {
                                reportdb.WeakSerology.Add(a);
                            }
                        }
                    }
                }

                if (reportdb.StrongAlleles.Contains("Bw6") || reportdb.StrongAlleles.Contains("Bw4"))
                {
                    List<string> aAllele = (from item in reportdb.StrongAlleles where item.StartsWith("A") select item).ToList<string>();
                    List<string> bAllele = (from itemb in reportdb.StrongAlleles where itemb.StartsWith("B") select itemb).ToList<string>();
                    List<string> cAllele = (from itemc in reportdb.StrongAlleles where itemc.StartsWith("C") select itemc).ToList<string>();
                    reportdb.StrongAlleleFinal = $"{string.Join(" ", aAllele)} {string.Join(" ", bAllele)} {string.Join(" ", cAllele)}";
                }
                else
                {
                    reportdb.StrongAlleleFinal = string.Join(" ", reportdb.StrongAlleles);
                }
                if (reportdb.ModAlleles.Contains("Bw6") || reportdb.ModAlleles.Contains("Bw4"))
                {
                    List<string> aAllele = (from item in reportdb.ModAlleles where item.StartsWith("A") select item).ToList<string>();
                    List<string> bAllele = (from itemb in reportdb.ModAlleles where itemb.StartsWith("B") select itemb).ToList<string>();
                    List<string> cAllele = (from itemc in reportdb.ModAlleles where itemc.StartsWith("C") select itemc).ToList<string>();
                    reportdb.ModAllelesFinal = $"{string.Join(" ", aAllele)} {string.Join(" ", bAllele)} {string.Join(" ", cAllele)}";
                }
                else
                {
                    reportdb.ModAllelesFinal = string.Join(" ", reportdb.ModAlleles);
                }
                if (reportdb.WeakAlleles.Contains("Bw6") || reportdb.WeakAlleles.Contains("Bw4"))
                {
                    List<string> aAllele = (from item in reportdb.WeakAlleles where item.StartsWith("A") select item).ToList<string>();
                    List<string> bAllele = (from itemb in reportdb.WeakAlleles where itemb.StartsWith("B") select itemb).ToList<string>();
                    List<string> cAllele = (from itemc in reportdb.WeakAlleles where itemc.StartsWith("C") select itemc).ToList<string>();
                    reportdb.WeakAllelesFinal = $"{string.Join(" ", aAllele)} {string.Join(" ", bAllele)} {string.Join(" ", cAllele)}";
                }
                else
                {
                    reportdb.WeakAllelesFinal = string.Join(" ", reportdb.WeakAlleles);
                }
                if (reportdb.WeakSerology.Contains("Bw6") || reportdb.WeakSerology.Contains("Bw4"))
                {
                    List<string> aAllele = (from item in reportdb.WeakSerology where item.StartsWith("A") select item).ToList<string>();
                    List<string> bAllele = (from itemb in reportdb.WeakSerology where itemb.StartsWith("B") select itemb).ToList<string>();
                    List<string> cAllele = (from itemc in reportdb.WeakSerology where itemc.StartsWith("C") select itemc).ToList<string>();
                    reportdb.WeakSerologyFinal = $"{string.Join(" ", aAllele)} {string.Join(" ", bAllele)} {string.Join(" ", cAllele)}";
                }
                else
                {
                    reportdb.WeakSerologyFinal = string.Join(" ", reportdb.WeakSerology);
                }
                if (reportdb.ModSerology.Contains("Bw6") || reportdb.ModSerology.Contains("Bw4"))
                {
                    List<string> aAllele = (from item in reportdb.ModSerology where item.StartsWith("A") select item).ToList<string>();
                    List<string> bAllele = (from itemb in reportdb.ModSerology where itemb.StartsWith("B") select itemb).ToList<string>();
                    List<string> cAllele = (from itemc in reportdb.ModSerology where itemc.StartsWith("C") select itemc).ToList<string>();
                    reportdb.ModSerologyFinal = $"{string.Join(" ", aAllele)} {string.Join(" ", bAllele)} {string.Join(" ", cAllele)}";
                }
                else
                {
                    reportdb.ModSerologyFinal = string.Join(" ", reportdb.ModSerology);
                }
                if (reportdb.StrongSerology.Contains("Bw6") || reportdb.StrongSerology.Contains("Bw4"))
                {
                    List<string> aAllele = (from item in reportdb.StrongSerology where item.StartsWith("A") select item).ToList<string>();
                    List<string> bAllele = (from itemb in reportdb.StrongSerology where itemb.StartsWith("B") select itemb).ToList<string>();
                    List<string> cAllele = (from itemc in reportdb.StrongSerology where itemc.StartsWith("C") select itemc).ToList<string>();
                    reportdb.StrongSerologyFinal = $"{string.Join(" ", aAllele)} {string.Join(" ", bAllele)} {string.Join(" ", cAllele)}";
                }
                else
                {
                    reportdb.StrongSerologyFinal = string.Join(" ", reportdb.StrongSerology);
                }

                
                var cserology = (from cTmp in reportdb.UnacceptableSerology where cTmp.StartsWith("C") select cTmp).ToList();
                if(cserology.Count() > 0)
                {
                    foreach(string item in cserology)
                    {
                        reportdb.UnacceptableSerology.Remove(item);
                    }                    
                }
                if (reportdb.UnacceptableSerology.Contains("Bw6") || reportdb.UnacceptableSerology.Contains("Bw4"))
                {
                    List<string> aAllele = (from item in reportdb.UnacceptableSerology where item.StartsWith("A") select item).ToList<string>();
                    List<string> bAllele = (from itemb in reportdb.UnacceptableSerology where itemb.StartsWith("B") select itemb).ToList<string>();
                    reportdb.StrongSerologyFinal = $"{string.Join(" ", aAllele)} {string.Join(" ", bAllele)}";
                }
                else
                {
                    reportdb.UnacceptableSeroFinal = string.Join(" ", reportdb.UnacceptableSerology);
                }
                

                if (excludeBW4)
                {
                    if (reportdb.StrongAlleleFinal.Contains("Bw4"))
                    {
                        reportdb.StrongAlleleFinal = reportdb.StrongAlleleFinal.Replace("Bw4", "");
                    }
                    if (reportdb.ModAllelesFinal.Contains("Bw4"))
                    {
                        reportdb.ModAllelesFinal = reportdb.ModAllelesFinal.Replace("Bw4", "");
                    }
                    if (reportdb.WeakAllelesFinal.Contains("Bw4"))
                    {
                        reportdb.WeakAllelesFinal = reportdb.WeakAllelesFinal.Replace("Bw4", "");
                    }
                }
                if (excludeBW6)
                {
                    if (reportdb.StrongAlleleFinal.Contains("Bw6"))
                    {
                        reportdb.StrongAlleleFinal.Replace("Bw6", "");
                    }
                    if (reportdb.ModAllelesFinal.Contains("Bw6"))
                    {
                        reportdb.ModAllelesFinal = reportdb.ModAllelesFinal.Replace("Bw6", "");
                    }
                    if (reportdb.WeakAllelesFinal.Contains("Bw6"))
                    {
                        reportdb.WeakAllelesFinal = reportdb.WeakAllelesFinal.Replace("Bw6", "");
                    }
                }

                //reportdb.WeakSerologyFinal = reportdb.WeakSerologyFinal.Replace("w", "");
                //reportdb.ModSerologyFinal = reportdb.ModSerologyFinal.Replace("w", "");
                //reportdb.StrongSerologyFinal = reportdb.StrongSerologyFinal.Replace("w", "");
            }
            else
            {
                BuildReportDataCII();
            }
        }
        public string GetAlleleFromRow(DataRow dr)
        {
            List<string> ret = new List<string>();
            if(dr["AlleleColA"].ToString().Length != 0)
            {
                ret.Add(dr["AlleleColA"].ToString());
            }
            if(dr["AlleleColB"].ToString().Length != 0)
            {
                ret.Add(dr["AlleleColB"].ToString());
            }
            if(dr["AlleleColC"].ToString().Length != 0)
            {
                ret.Add(dr["AlleleColC"].ToString());
            }
            if (dr["AlleleDRCol"].ToString().Length != 0)
            {
                ret.Add(dr["AlleleDRCol"].ToString());
            }
            if (dr["AlleleDQACol"].ToString().Length != 0)
            {
                ret.Add(dr["AlleleDQACol"].ToString());
            }
            if (dr["AlleleDQBCol"].ToString().Length != 0)
            {
                ret.Add(dr["AlleleDQBCol"].ToString());
            }
            if (dr["AlleleDPACol"].ToString().Length != 0)
            {
                ret.Add(dr["AlleleDPACol"].ToString());
            }
            if (dr["AlleleDPBCol"].ToString().Length != 0)
            {
                ret.Add(dr["AlleleDPBCol"].ToString());
            }
            return string.Join(",", ret);
        }
        public string getSerologyFromRow(DataRow dr)
        {
            List<string> ret = new List<string>();
            if (dr["AlleleDRSerology"].ToString().Length != 0)
            {
                ret.Add(dr["AlleleDRSerology"].ToString());
            }
            if (dr["AlleleDQSerology"].ToString().Length != 0)
            {
                ret.Add(dr["AlleleDQSerology"].ToString());
            }
            if (dr["AlleleDPSerology"].ToString().Length != 0)
            {
                ret.Add(dr["AlleleDPSerology"].ToString());
            }
            return string.Join(",", ret);
        }
        public void LoadData()
        {
            reportDS = ReportDAL.GetReportData(sessionid, sampleid, classtype);
        }

        public void BuildReportDataCII()
        {
            //sessionID, sampleID,  rawValue,assignment, test_date 
            //          AlleleDRCol, AlleleDPACol, AlleleDPBCol, AlleleDQACol, AlleleDQBCol, AlleleDRSerology
            //, AlleleDPASerology, AlleleDPBSerology, AlleleDQASerology, AlleleDQBSerology

            int high = int.Parse(highRange);
            int midlow = int.Parse(midRange.Split('-')[0]);
            int midhigh = int.Parse(midRange.Split('-')[1]);
            int lowlow = int.Parse(lowRange.Split('-')[0]);
            int lowhight = int.Parse(lowRange.Split('-')[1]);
            int unacceptablelow = int.Parse(unacceptable);
            Dictionary<string, List<decimal>> seroValuePairs = new Dictionary<string, List<decimal>>();
            Dictionary<string, List<decimal>> allValuePairs = new Dictionary<string, List<decimal>>();
            var pos = (from item in reportDS.Tables[1].AsEnumerable() where decimal.Parse(item["PctPositive"].ToString()) == 100m select item).ToList();
            
            foreach(DataRow row in pos)
            {
                var rawValueRows = (from raw in reportDS.Tables[0].AsEnumerable()
                                    where
                                    raw["AlleleDRCol"].ToString() == row["Antigen"].ToString()
                                    || raw["AlleleDPACol"].ToString() == row["Antigen"].ToString()
                                    || raw["AlleleDPBCol"].ToString() == row["Antigen"].ToString()
                                    || raw["AlleleDQACol"].ToString() == row["Antigen"].ToString()
                                    || raw["AlleleDQBCol"].ToString() == row["Antigen"].ToString()
                                    select raw).ToList();
                decimal avgRawValue = 0m;
                List<decimal> tmp = new List<decimal>();
                string finS = string.Empty;
                reportdb.SampleID = sampleid;
                reportdb.SerumDate = DateTime.Parse(reportDS.Tables[6].Rows[0]["rundate"].ToString()).ToString("MM/dd/yyyy");
                foreach (DataRow data in rawValueRows)
                {
                    switch (row["Antigen"].ToString().Substring(0, 3))
                    {
                        case "DRB":

                            if(data["AlleleDRSerology"].ToString().Length != 0)
                            {
                                //is serology all positive?
                                var seroPOS = (from i in reportDS.Tables[0].AsEnumerable()
                                               where i["AlleleDRSerology"].ToString() == data["AlleleDRSerology"].ToString()
                                                && i["assignment"].ToString() == "Negative"
                                               select i).Any();
                                if (seroPOS)
                                    finS = row["Antigen"].ToString();
                                else
                                    finS = data["AlleleDRSerology"].ToString().Length != 0 ? data["AlleleDRSerology"].ToString() : row["Antigen"].ToString();
                            }
                            break;
                        case "DQB":
                            if (data["AlleleDQSerology"].ToString().Length != 0)
                            {
                                //is serology all positive?
                                var seroPOS = (from i in reportDS.Tables[0].AsEnumerable()
                                               where i["AlleleDQSerology"].ToString() == data["AlleleDQSerology"].ToString()
                                                && i["assignment"].ToString() == "Negative"
                                               select i).Any();
                                if (seroPOS)
                                    finS = row["Antigen"].ToString();
                                else
                                    finS = data["AlleleDQSerology"].ToString().Length != 0 ? data["AlleleDQSerology"].ToString() : row["Antigen"].ToString();
                            }
                            break;
                        case "DQA":
                            finS = row["Antigen"].ToString();
                            break;
                        case "DPB":
                            if (data["AlleleDPSerology"].ToString().Length != 0)
                            {
                                //is serology all positive?
                                var seroPOS = (from i in reportDS.Tables[0].AsEnumerable()
                                               where i["AlleleDPSerology"].ToString() == data["AlleleDPSerology"].ToString()
                                                && i["assignment"].ToString() == "Negative"
                                               select i).Any();
                                if (seroPOS)
                                    finS = row["Antigen"].ToString();
                                else
                                    finS = data["AlleleDPSerology"].ToString().Length != 0 ? data["AlleleDPSerology"].ToString() : row["Antigen"].ToString();
                            }
                            finS = finS.Replace("w", "");
                            List<string> seroArray = new List<string>() { "DP1", "DP2", "DP3", "DP5", "DP6" };
                            if (!(seroArray.Contains(finS)))
                            {
                                finS = row["Antigen"].ToString();
                            }
                            break;
                        case "DPA":
                            finS = row["Antigen"].ToString();
                            break;
                    }               
                    tmp.Add(decimal.Parse(data["rawValue"].ToString()));
                }
                avgRawValue = tmp.Average();
                string finalSero = finS.Contains("(") ? finS.Split('(')[0] : finS;
                if (avgRawValue > high)
                {
                    if(!reportdb.StrongSerology.Contains(finalSero))
                        reportdb.StrongSerology.Add(finalSero);
                    if(!reportdb.StrongAlleles.Contains(row["Antigen"].ToString()))
                        reportdb.StrongAlleles.Add(row["Antigen"].ToString());
                }
                else if (avgRawValue > midlow && avgRawValue <= midhigh)
                {
                    if(!reportdb.ModSerology.Contains(finalSero))
                        reportdb.ModSerology.Add(finalSero);
                    if(!reportdb.ModAlleles.Contains(row["Antigen"].ToString()))
                        reportdb.ModAlleles.Add(row["Antigen"].ToString());
                }
                else if (avgRawValue > lowlow && avgRawValue <= lowhight)
                {
                    if(!reportdb.WeakSerology.Contains(finalSero))
                        reportdb.WeakSerology.Add(finalSero);
                    if(!reportdb.WeakAlleles.Contains(row["Antigen"].ToString()))
                        reportdb.WeakAlleles.Add(row["Antigen"].ToString());
                }
                if (avgRawValue >= unacceptablelow)
                {
                    if (finalSero.StartsWith("DR"))
                    {
                        if(!reportdb.UnacceptableSerology.Contains(finalSero))
                            reportdb.UnacceptableSerology.Add(finalSero);
                    }
                }
            }

            //foreach (DataRow dr in reportDS.Tables[0].Rows)
            //{
            //   // if(pos[""])
            //    decimal raw = decimal.Parse(dr["rawValue"].ToString());
            //    string call = dr["assignment"].ToString();
            //    string serology = getSerologyFromRow(dr);
            //    string alleles = GetAlleleFromRow(dr);

            //    if (serology == "DPw4")
            //    {
            //        serology = alleles.Split(',')[1];
            //    }
            //    List<string> addedSero = new List<string>();

            //    if (call.ToLower() == "positive")
            //    {

            //        List<string> negList = new List<string>();
            //        List<string> alleleList = alleles.Split(',').ToList<string>();
            //        foreach(string item in alleleList)
            //        {
            //            var allelesNEG = (from neg in reportDS.Tables[0].AsEnumerable()
            //                              where (neg["AlleleDRCol"].ToString() == item
            //                             || neg["AlleleDPACol"].ToString() == item
            //                             || neg["AlleleDPBCol"].ToString() == item
            //                             || neg["AlleleDQACol"].ToString() == item
            //                             || neg["AlleleDQBCol"].ToString() == item)
            //                             && neg["Assignment"].ToString() == "Negative"
            //                              select neg).Any();


            //            if(allelesNEG) { negList.Add(item); }
            //            else
            //            {
            //                //check to see if allele has serology
            //                switch (item.Substring(0, 3))
            //                {
            //                    case "DRB":
            //                        var dritem = (from dri in reportDS.Tables[0].AsEnumerable()
            //                                     where dri["AlleleDRCol"].ToString() == item
            //                                     select dri).FirstOrDefault();
            //                        if(dritem["AlleleDRSerology"].ToString().Length == 0)
            //                        {
            //                            addedSero.Add(item);
            //                        }
            //                        break;
            //                    case "DPA":
            //                        addedSero.Add(item);
            //                        break;
            //                    case "DPB":
            //                        var dritemdpb = (from dri in reportDS.Tables[0].AsEnumerable()
            //                                        where dri["AlleleDPBCol"].ToString() == item
            //                                        select dri).FirstOrDefault();
            //                        if (dritemdpb["AlleleDPSerology"].ToString().Length == 0)
            //                        {
            //                            addedSero.Add(item);
            //                        }
            //                        break;
            //                    case "DQB":
            //                        var dritemdqb = (from dri in reportDS.Tables[0].AsEnumerable()
            //                                         where dri["AlleleDQBCol"].ToString() == item
            //                                         select dri).FirstOrDefault();
            //                        if (dritemdqb["AlleleDQSerology"].ToString().Length == 0)
            //                        {
            //                            addedSero.Add(item);
            //                        }
            //                        break;
            //                    case "DQA":
            //                        addedSero.Add(item);
            //                        break;
            //                }
            //            }
            //        }

            //        List<string> negSero = new List<string>();
            //        List<string> serolist = serology.Split(',').ToList<string>();
            //        foreach(string item in serolist)
            //        {
            //            var allelesNeg = (from neg in reportDS.Tables[0].AsEnumerable()
            //                    where (neg["AlleleDRSerology"].ToString() == item
            //                    || neg["AlleleDPSerology"].ToString() == item
            //                    || neg["AlleleDQSerology"].ToString() == item)
            //                    && neg["Assignment"].ToString() == "Negative"
            //                     select neg).Any();
            //            if(allelesNeg) { negSero.Add(item); }
            //        }

            //        foreach (string s in serolist)
            //        {
            //            if (!negSero.Contains(s))
            //            {
            //                if (seroValuePairs.ContainsKey(serology))
            //                {
            //                    seroValuePairs[serology].Add(raw);
            //                }
            //                else
            //                {
            //                    List<decimal> elem = new List<decimal>();
            //                    elem.Add(raw);
            //                    seroValuePairs.Add(serology, elem);
            //                }
            //            }
            //        }
                    
            //        foreach (string i in alleleList)
            //        {
            //            if (!negList.Contains(i))
            //            {
            //                if (allValuePairs.ContainsKey(i))
            //                {
            //                    allValuePairs[i].Add(raw);
            //                }
            //                else
            //                {
            //                    List<decimal> elem = new List<decimal>();
            //                    elem.Add(raw);
            //                    allValuePairs.Add(i, elem);
            //                }
            //            }
            //            if (addedSero.Contains(i))
            //            {
            //                List<decimal> elem = new List<decimal>();
            //                elem.Add(raw);
            //                string oser = i;
            //                if (i.StartsWith("DPB"))
            //                {
            //                    oser = i.Split(':')[0].Replace("B1*", "");
            //                }
            //                if (seroValuePairs.ContainsKey(oser))
            //                {
            //                    seroValuePairs[oser].Add(raw);
            //                }
            //                else
            //                {
            //                    seroValuePairs.Add(oser, elem);
            //                }
                            
            //            }
            //        }
            //    }
            //}
            //foreach (string s in seroValuePairs.Keys)
            //{
            //    decimal finalsero;
            //    if (seroValuePairs[s].Count > 1)
            //    {
            //        //take the average of the values
            //        finalsero = seroValuePairs[s].Average();
            //    }
            //    else
            //    {
            //        finalsero = seroValuePairs[s][0];
            //    }
            //    string finS = s.Contains("(") ? s.Split('(')[0] : s;
            //    if (finalsero > high)
            //    {                       
            //        reportdb.StrongSerology.Add(finS);
            //    }
            //    else if (finalsero > midlow && finalsero <= midhigh)
            //    {
            //        reportdb.ModSerology.Add(finS);
            //    }
            //    else if (finalsero > lowlow && finalsero <= lowhight)
            //    {
            //        reportdb.WeakSerology.Add(finS);
            //    }
            //    if (finalsero >= unacceptablelow)
            //    {
            //        if (finS.StartsWith("DR"))
            //        {
            //            reportdb.UnacceptableSerology.Add(finS);
            //        }
            //    }
            //}

            //foreach (string a in allValuePairs.Keys)
            //{
            //    decimal finalallele;
            //    if (a.Length > 0)
            //    {
            //        if (allValuePairs[a].Count > 1)
            //        {
            //            finalallele = allValuePairs[a].Average();
            //        }
            //        else
            //        {
            //            finalallele = allValuePairs[a][0];
            //        }

            //        if (finalallele > high)
            //        {
            //            reportdb.StrongAlleles.Add(a);
            //        }
            //        else if (finalallele > midlow && finalallele <= midhigh)
            //        {
            //            reportdb.ModAlleles.Add(a);
            //        }
            //        else if (finalallele > lowlow && finalallele <= lowhight)
            //        {
            //            reportdb.WeakAlleles.Add(a);
            //        }
            //    }
            //}
            reportdb.StrongAlleleFinal = SortAlleles(reportdb.StrongAlleles);
            reportdb.ModAllelesFinal = SortAlleles(reportdb.ModAlleles);
            reportdb.WeakAllelesFinal = SortAlleles(reportdb.WeakAlleles);
            reportdb.WeakSerologyFinal = SortSerology(reportdb.WeakSerology);
            reportdb.ModSerologyFinal = SortSerology(reportdb.ModSerology);
            reportdb.StrongSerologyFinal = SortSerology(reportdb.StrongSerology);
            reportdb.UnacceptableSeroFinal = SortSerology(reportdb.UnacceptableSerology);

            reportdb.WeakSerologyFinal= reportdb.WeakSerologyFinal.Replace("w", "");
            reportdb.ModSerologyFinal = reportdb.ModSerologyFinal.Replace("w", "");
            reportdb.StrongSerologyFinal = reportdb.StrongSerologyFinal.Replace("w", "");
            reportdb.UnacceptableSeroFinal = reportdb.UnacceptableSeroFinal.Replace("w", "");
        }
        public string SortAlleles(List<string> alleles)
        {
            Dictionary<string, int> sorted = new Dictionary<string, int>();

            //sort order DR1,3,4,5 DQB, DQA, DPB, DPA
            var listDR = (from dr in alleles where dr.StartsWith("DR") select dr).ToList();
            var listDQB = (from dqb in alleles where dqb.StartsWith("DQB") select dqb).ToList();
            var listDQA = (from dqa in alleles where dqa.StartsWith("DQA") select dqa).ToList();
            var listDPB = (from dpb in alleles where dpb.StartsWith("DPB") select dpb).ToList();
            var listDPA = (from dpa in alleles where dpa.StartsWith("DPA") select dpa).ToList();
            string ret = $"{string.Join(" ", listDR)} {string.Join(" ", ReportDAL.GetSortedAntigens(listDQB))} {string.Join(" ", ReportDAL.GetSortedAntigens(listDQA))} {string.Join(" ", ReportDAL.GetSortedAntigens(listDPB))} {string.Join(" ", ReportDAL.GetSortedAntigens(listDPA))}";
            return ret;
        }

        public string SortSerology(List<string> alleles)
        {
            Dictionary<string, int> sorted = new Dictionary<string, int>();

            //sort order DR1,3,4,5 DQB, DQA, DPB, DPA
            var listDR = (from dr in alleles where dr.StartsWith("DR") select dr).ToList();
            var listDQB = (from dqb in alleles where dqb.StartsWith("DQ") && !dqb.StartsWith("DQA") select dqb).ToList();
            var listDQA = (from dqa in alleles where dqa.StartsWith("DQA") select dqa).ToList();
            var listDPB = (from dpb in alleles where dpb.StartsWith("DP") && !dpb.StartsWith("DPA") select dpb).ToList();
            var listDPA = (from dpa in alleles where dpa.StartsWith("DPA") select dpa).ToList();
            string ret = $"{string.Join(" ", listDR)} {string.Join(" ", listDQB)} {string.Join(" ", ReportDAL.GetSortedAntigens(listDQA))} {string.Join(" ", listDPB)} {string.Join(" ", ReportDAL.GetSortedAntigens(listDPA))}";
            return ret;
        }
    }

    public class ReportData 
    {
        public string SampleID { get; set; }
        public string SerumDate { get; set; }
        public List<string> StrongAlleles { get; set; }
        public List<string> ModAlleles { get; set; }
        public List<string> WeakAlleles { get; set; }
        public List<string> StrongSerology { get; set; }
        public List<string> ModSerology { get; set; }
        public List<string>  WeakSerology { get; set; }
        public List<string> UnacceptableSerology { get; set; }
        public string StrongAlleleFinal { get; set; }
        public string ModAllelesFinal { get; set; }
        public string WeakAllelesFinal { get; set; }
        public string StrongSerologyFinal { get; set; }
        public string ModSerologyFinal { get; set; }
        public string WeakSerologyFinal { get; set; }
        public string UnacceptableSeroFinal { get; set; }
    }

    public class BeadDataCI
    {
        public string antigen { get; set; }
        public string serology { get; set; }
        public decimal mfi { get; set; }
        public bool positive { get; set; }
    }
    public class BeadDataCII
    {
        public string antigenI { get; set; }
        public string antigenII { get; set; }
        public string serologyI { get; set; }
        public decimal mfi { get; set; }
        public bool positive { get; set; }
    }

}
