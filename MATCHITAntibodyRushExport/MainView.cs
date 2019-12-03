using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using RushViewModel;
using System.Windows.Forms;

namespace MATCHITAntibodyRushExport
{
    public partial class MainView : DevExpress.XtraEditors.XtraForm
    {
        Dictionary<string, List<string>> batches;
        Dictionary<string, List<string>> Selectedbatches;
        Dictionary<string, List<string>> MasterBatchList;
        List<string> availableSamples;
        List<string> selectedSamples;
        List<string> successfulBatches;
        List<string> unsuccessfuleBatches;
        
        string pathtosave;

        public void RestoreSettings(String fileName)
        {
            if (!(File.Exists(fileName)))
            {
                File.Copy(String.Format("{0}\\RUSHSettings.xml", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)), fileName);
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNodeList nodes;// = new XmlNodeList();
            nodes = doc.SelectNodes("/Settings");
            try
            {
                if (nodes.Item(0).SelectSingleNode("reportPath").InnerText.Length == 0)
                {
                    pathtosave = "c:\\";
                }
                else
                {
                    pathtosave = nodes.Item(0).SelectSingleNode("reportPath").InnerText;
                }
               
            }
            catch
            {
               
                //nodes does not exist.
                //create node and add value
                XmlNode node = doc.CreateNode(XmlNodeType.Element, "Settings", null);
                //create title node
                XmlNode nodeTitle = doc.CreateElement("reportPath");
                //add value for it
                node.AppendChild(nodeTitle);
                nodeTitle.InnerText = "c:\\";
                doc.DocumentElement.AppendChild(node);
                //save back
                doc.Save(fileName);
            }
        }
        public void UpdatePath(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            XmlNodeList nodes = doc.SelectNodes("/Settings");
            XmlNode myNode = nodes.Item(0).SelectSingleNode("reportPath");
            myNode.InnerText = pathtosave;
            doc.Save(filename);

        }
        public MainView()
        {
            Selectedbatches = new Dictionary<string, List<string>>();
            MasterBatchList = new Dictionary<string, List<string>>();
            selectedSamples = new List<string>();
            availableSamples = new List<string>();
            successfulBatches = new List<string>();
            unsuccessfuleBatches = new List<string>();
            RestoreSettings(String.Format("{0}\\LIFECODES\\RUSHSettings.xml", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)));
            InitializeComponent();
        }

        private void ExitAppliction(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            this.Close();
        }

        private void simpleButton10_Click(object sender, EventArgs e)
        {
            if(dtFrom.SelectedText.Length == 0 || dtTo.SelectedText.Length == 0)
            {
                MessageBox.Show("Please select a valid date range", "Error", MessageBoxButtons.OK);
            }
            else
            {
                BatchListVM batchListVM = new BatchListVM(dtFrom.DateTime, dtTo.DateTime);
                batchListVM.Load();
                MasterBatchList = batchListVM.batches.ToDictionary(kvp=>kvp.Key,kvp=>kvp.Value);
                batches = new Dictionary<string, List<string>>(MasterBatchList);
                lstAvailable.DataSource = batches.Keys;
            }
        }

        private void btnBatchAll_Click(object sender, EventArgs e)
        {
            if (batches != null)
            {
                lstAvailable.DataSource = null;
                Selectedbatches = new Dictionary<string, List<string>>(MasterBatchList);
                lstSelected.DataSource = Selectedbatches.Keys;
                lstSelected.SelectedIndex = -1;
                batches.Clear();
            }
        }

        private void btnBatchOne_Click(object sender, EventArgs e)
        {
            if(lstAvailable.SelectedItem != null)
            {
                Selectedbatches.Add(lstAvailable.SelectedItem.ToString(), batches[lstAvailable.SelectedItem.ToString()]);
                lstSelected.DataSource = null;
                lstSelected.DataSource = Selectedbatches.Keys;
                batches.Remove(lstAvailable.SelectedItem.ToString());
                lstAvailable.DataSource = null;
                lstAvailable.DataSource = batches.Keys;
            }
        }

        private void btnBatchremoveone_Click(object sender, EventArgs e)
        {
            if (lstSelected.SelectedItem != null)
            {
                batches.Add(lstSelected.SelectedItem.ToString(), Selectedbatches[lstSelected.SelectedItem.ToString()]);
                lstAvailable.DataSource = null;
                lstAvailable.DataSource = batches.Keys;
                Selectedbatches.Remove(lstSelected.SelectedItem.ToString());            
                lstSelected.DataSource = null;
                lstSelected.DataSource = Selectedbatches.Keys;
            }
        }

        private void btnbatchremoveAll_Click(object sender, EventArgs e)
        {
            batches = new Dictionary<string, List<string>>(MasterBatchList);
            Selectedbatches.Clear();
            lstAvailable.DataSource = batches.Keys;
            lstSelected.DataSource = null;
        }

        private void btnsampleAll_Click(object sender, EventArgs e)
        {
            lstAvailableSamples.DataSource = null;
            selectedSamples = new List<string>(availableSamples);
            availableSamples.Clear();
            lstSelectedSamples.DataSource = selectedSamples;            
        }

        private void btnSampleOne_Click(object sender, EventArgs e)
        {
            if(lstAvailableSamples.SelectedItem != null)
            {
                selectedSamples.Add(lstAvailableSamples.SelectedItem.ToString());
                availableSamples.Remove(lstAvailableSamples.SelectedItem.ToString());
                lstAvailableSamples.DataSource = null;
                lstSelectedSamples.DataSource = null;
                lstSelectedSamples.DataSource = selectedSamples;
                lstAvailableSamples.DataSource = availableSamples;
            }            
        }

        private void btnsampleremoveOne_Click(object sender, EventArgs e)
        {
            if(lstSelectedSamples.SelectedItem != null)
            {
                availableSamples.Add(lstSelectedSamples.SelectedItem.ToString());
                selectedSamples.Remove(lstSelectedSamples.SelectedItem.ToString());
                lstAvailableSamples.DataSource = null;
                lstSelectedSamples.DataSource = null;
                lstSelectedSamples.DataSource = selectedSamples;
                lstAvailableSamples.DataSource = availableSamples;
            }
        }

        private void btnsampleremoveAll_Click(object sender, EventArgs e)
        {
            lstSelectedSamples.DataSource = null;
            availableSamples = new List<string>(selectedSamples);
            selectedSamples.Clear();
            lstAvailableSamples.DataSource = availableSamples;
        }

        private void lstSelected_SelectedIndexChanged(object sender, EventArgs e)
        {
            ///load samples for selected item
            if(lstSelected.SelectedItem != null)
            {
                availableSamples = new List<string> (Selectedbatches[lstSelected.SelectedItem.ToString()]);
                lstAvailableSamples.DataSource = availableSamples;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if(pathtosave.Length == 0)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Please set the location to save the files before exporting.");
            }
            if(lstSelectedSamples.ItemCount > 0)
            {         
                //only selected samples for batch
                Parallel.ForEach((List<string>)lstSelectedSamples.DataSource,  samp => 
                {
                    ReportDataVM rpData = new ReportDataVM(lstSelected.SelectedItem.ToString(), samp, "I");
                    rpData.LoadData();
                    rpData.BuildReportData();
                    bool successful = CreateAndSaveFile(rpData);
                });
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Export is complete");
            }
            else if(lstSelected.ItemCount > 0)
            {
                ConcurrentDictionary<string, List<ReportDataVM>> batchRpt = new ConcurrentDictionary<string, List<ReportDataVM>>();
                //all selected batches with their samples.
                foreach(string batc in Selectedbatches.Keys)
                {
                    Parallel.ForEach(Selectedbatches[batc], item =>
                    {
                        ReportDataVM rpData = new ReportDataVM(batc, item, "I");
                        rpData.LoadData();
                        rpData.BuildReportData();
                        if (batchRpt.ContainsKey(batc))
                        {
                            batchRpt[batc].Add(rpData);
                        }
                        else
                        {
                            List<ReportDataVM> rpt = new List<ReportDataVM>();
                            rpt.Add(rpData);
                            batchRpt.TryAdd(batc, rpt);
                        }
                        //bool successful = CreateAndSaveFile(rpData);                  
                    });
                    bool successful = CreateAndSaveFile(batchRpt, batc);
                }
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Export is complete");
            }
            else
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("No Batches or Samples have been selected.");
            }
        }
        private bool CreateAndSaveFile(ConcurrentDictionary<string, List<ReportDataVM>> samples, string batch)
        {
            Excel.Application appExcel;
            Excel._Workbook wrkbook;
            Excel.Workbooks objBooks;
            Excel.Sheets objSheets;
            Excel._Worksheet objSheet;
            try
            {
                // Instantiate Excel and start a new workbook.
                appExcel = new Excel.Application();
                objBooks = appExcel.Workbooks;
                wrkbook = objBooks.Add();
                objSheets = wrkbook.Worksheets;

                objSheet = (Excel._Worksheet)objSheets.get_Item(1);

                objSheet.Range["A1:A9"].Style.Font.Name = "Calbri";
                objSheet.Range["A1:A9"].Style.Font.Size = 8;
                objSheet.Range["A1:A9"].Style.WrapText = true;
                objSheet.Range["A1:A9"].Style.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                objSheet.Range["A1:A9"].Style.VerticalAlignment = Excel.XlVAlign.xlVAlignTop;
                objSheet.Range["A1:I1"].Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
                objSheet.Range["A1:I1"].Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
                objSheet.Range["A1:I1"].Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                objSheet.Range["A1:I1"].Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                objSheet.Range["A1:I1"].Borders.Color = Color.Black;
                objSheet.Cells[1, 1] = $"Sample ID";
                objSheet.get_Range("B1", "B1").ColumnWidth = 15;
                objSheet.Cells[1, 2] = $"Serum Date";
                objSheet.get_Range("C1", "C1").ColumnWidth = 30;
                objSheet.Cells[1, 3] = $"Strong MFI (>5000) Alleles";
                objSheet.get_Range("D1", "D1").ColumnWidth = 30;
                objSheet.Cells[1, 4] = $"Mod MFI (2000-5000) Alleles";
                objSheet.get_Range("E1", "E1").ColumnWidth = 30;
                objSheet.Cells[1, 5] = $"Weak MFI (700-2000) Alleles";
                objSheet.get_Range("F1", "F1").ColumnWidth = 30;
                objSheet.Cells[1, 6] = $"Strong MFI (>5000) Serology";
                objSheet.get_Range("G1", "G1").ColumnWidth = 30;
                objSheet.Cells[1, 7] = $"Mod MFI (2000-5000) Serology";
                objSheet.get_Range("H1", "H1").ColumnWidth = 30;
                objSheet.Cells[1, 8] = $"Weak MFI (700-2000) Serology";
                objSheet.get_Range("I1", "I1").ColumnWidth = 30;
                objSheet.Cells[1, 9] = $"A, B, DR MFI (≥ 3500) Serology";

                int col = 2;
                foreach(ReportDataVM item in samples[batch])
                {
                    objSheet.Cells[col, 1] = item.reportdb.SampleID;
                    objSheet.Cells[col, 2] = item.reportdb.SerumDate;
                    objSheet.Cells[col, 3] = item.reportdb.StrongAlleleFinal;
                    objSheet.Cells[col, 4] = item.reportdb.ModAllelesFinal;
                    objSheet.Cells[col, 5] = item.reportdb.WeakAllelesFinal;
                    objSheet.Cells[col, 6] = item.reportdb.StrongSerologyFinal;
                    objSheet.Cells[col, 7] = item.reportdb.ModSerologyFinal;
                    objSheet.Cells[col, 8] = item.reportdb.WeakSerologyFinal;
                    objSheet.Cells[col, 9] = item.reportdb.UnacceptableSeroFinal;
                    string rangeval = $"A{col}:I9";
                    objSheet.Range[rangeval].Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
                    objSheet.Range[rangeval].Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
                    objSheet.Range[rangeval].Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                    objSheet.Range[rangeval].Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                    objSheet.Range[rangeval].Borders.Color = Color.Black;
                    col++;
                }

                wrkbook.SaveAs($"{pathtosave}\\{batch}.xlsx");

                appExcel.Quit();
                GC.Collect();

                Marshal.FinalReleaseComObject(objSheets);
                Marshal.FinalReleaseComObject(objSheet);

                Marshal.FinalReleaseComObject(objBooks);

                Marshal.FinalReleaseComObject(wrkbook);
                Marshal.FinalReleaseComObject(appExcel);
                //dispose of objects.
                appExcel = null;
                wrkbook = null;
                objBooks = null;
                objSheet = null;
                objSheets = null;


                return true;
            }
            catch (Exception ex)
            {
                //unsuccessfuleBatches.Add($"{reportDataVM.sessionid} : {reportDataVM.sampleid}");
                return false;
            }
        }
        private bool CreateAndSaveFile(ReportDataVM reportDataVM)
        {
            Excel.Application appExcel;
            Excel._Workbook wrkbook;
            Excel.Workbooks objBooks;
            Excel.Sheets objSheets;
            Excel._Worksheet objSheet;
            Excel.Range range;
            //Excel.Font font = null;
            try
            {
                // Instantiate Excel and start a new workbook.
                appExcel = new Excel.Application();
                objBooks = appExcel.Workbooks;
                wrkbook = objBooks.Add();
                objSheets = wrkbook.Worksheets;               
                
                objSheet = (Excel.Worksheet)objSheets.get_Item(1);
                
                
                objSheet.Cells[1, 1] = $"Sample ID";
                objSheet.get_Range("B1", "B1").ColumnWidth = 15;
                objSheet.Cells[1, 2] = $"Serum Date";
                objSheet.get_Range("C1", "C1").ColumnWidth = 30;
                objSheet.Cells[1, 3] = $"Strong MFI (>5000) Alleles";
                objSheet.get_Range("D1", "D1").ColumnWidth = 30;
                objSheet.Cells[1, 4] = $"Mod MFI (2000-5000) Alleles";
                objSheet.get_Range("E1", "E1").ColumnWidth = 30;
                objSheet.Cells[1, 5] = $"Weak MFI (700-2000) Alleles";
                objSheet.get_Range("F1", "F1").ColumnWidth = 30;
                objSheet.Cells[1, 6] = $"Strong MFI (>5000) Serology";
                objSheet.get_Range("G1", "G1").ColumnWidth = 30;
                objSheet.Cells[1, 7] = $"Mod MFI (2000-5000) Serology";
                objSheet.get_Range("H1", "H1").ColumnWidth = 30;
                objSheet.Cells[1, 8] = $"Weak MFI (700-2000) Serology";
                objSheet.get_Range("I1", "I1").ColumnWidth = 30;
                objSheet.Cells[1, 9] = $"A, B, DR MFI (≥ 3500) Serology";
                objSheet.Range["A1:A9"].Style.Font.Name = "Calbri";
                objSheet.Range["A1:A9"].Style.Font.Size = 8;
                objSheet.Range["A1:A9"].Style.WrapText = true;
                objSheet.Range["A1:A9"].Style.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                objSheet.Range["A1:A9"].Style.VerticalAlignment = Excel.XlVAlign.xlVAlignTop;
                objSheet.Range["A1:I2"].Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
                objSheet.Range["A1:I2"].Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
                objSheet.Range["A1:I2"].Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                objSheet.Range["A1:I2"].Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                objSheet.Range["A1:I2"].Borders.Color = Color.Black;
                objSheet.Cells[2, 1] = reportDataVM.reportdb.SampleID;
                objSheet.Cells[2, 2] = reportDataVM.reportdb.SerumDate;
                objSheet.Cells[2, 3] = reportDataVM.reportdb.StrongAlleleFinal;
                objSheet.Cells[2, 4] = reportDataVM.reportdb.ModAllelesFinal;
                objSheet.Cells[2, 5] = reportDataVM.reportdb.WeakAllelesFinal;
                objSheet.Cells[2, 6] = reportDataVM.reportdb.StrongSerologyFinal;
                objSheet.Cells[2, 7] = reportDataVM.reportdb.ModSerologyFinal;
                objSheet.Cells[2, 8] = reportDataVM.reportdb.WeakSerologyFinal;
                objSheet.Cells[2, 9] = reportDataVM.reportdb.UnacceptableSeroFinal;

                wrkbook.SaveAs($"{pathtosave}\\{reportDataVM.sessionid}_{reportDataVM.sampleid}.xlsx");
                

                appExcel.Quit();
                GC.Collect();

                Marshal.FinalReleaseComObject(objSheets);
                Marshal.FinalReleaseComObject(objSheet);

                Marshal.FinalReleaseComObject(objBooks);

                Marshal.FinalReleaseComObject(wrkbook);
                Marshal.FinalReleaseComObject(appExcel);
                //dispose of objects.
                appExcel = null;
                wrkbook = null;
                objBooks = null;
                objSheet = null;
                objSheets = null;
                range = null;
                

                return true;
            }
            catch (Exception ex)
            {
                unsuccessfuleBatches.Add($"{reportDataVM.sessionid} : {reportDataVM.sampleid}");
                return false;
            }                
        }
        private void barEditItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                barEditItem1.EditValue = folderBrowserDialog1.SelectedPath;
                pathtosave = barEditItem1.EditValue.ToString();
                UpdatePath(String.Format("{0}\\LIFECODES\\RUSHSettings.xml", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)));
            }
        }

        private void barSubItem2_Popup(object sender, EventArgs e)
        {
            barEditItem1.EditValue = pathtosave;
        }

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            MFIGroups mFI = new MFIGroups();
            mFI.ShowDialog();
        }
    }
}
