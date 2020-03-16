using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Forms;

namespace MATCHITAntibodyRushExport
{
    public partial class MFIGroups : Form
    {
        public MFIGroups()
        {
            InitializeComponent();
        }

        private void MFIGroups_Load(object sender, EventArgs e)
        {
            string fileName = String.Format("{0}\\LIFECODES\\RUSHSettings.xml", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            LoadData(fileName);
        }
        private void LoadData(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            XmlNodeList nodes;// = new XmlNodeList();
            nodes = doc.SelectNodes("/Settings");
            try
            {
                string strong = nodes.Item(0).SelectSingleNode("highrange").InnerText;
                string mid = nodes.Item(0).SelectSingleNode("midrange").InnerText;
                string low = nodes.Item(0).SelectSingleNode("lowrange").InnerText;
                string unaccept = nodes.Item(0).SelectSingleNode("unacceptable").InnerText;

                txtStrong.Text = strong;
                txtmodlow.Text = mid.Split('-')[0];
                txtmodhigh.Text = mid.Split('-')[1];
                txtweaklow.Text = low.Split('-')[0];
                txtweakhigh.Text = low.Split('-')[1];
                if (unaccept.Contains("-"))
                {
                    txtunacceptablelow.Text = unaccept.Split('-')[0];
                    txtunacceptablehigh.Text = unaccept.Split('-')[1];
                }
                else
                {
                    txtunacceptablelow.Text = unaccept;
                    txtunacceptablehigh.Text = unaccept;
                }
               
            }
            catch
            {

            }
        }
        public void SaveData(string filename)
        {
            Cursor = Cursors.WaitCursor;
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            XmlNodeList nodes;// = new XmlNodeList();
            nodes = doc.SelectNodes("/Settings");
            try
            {
                nodes.Item(0).SelectSingleNode("highrange").InnerText = txtStrong.Text;
                nodes.Item(0).SelectSingleNode("midrange").InnerText = $"{txtmodlow.Text}-{txtmodhigh.Text}";
                nodes.Item(0).SelectSingleNode("lowrange").InnerText = $"{txtweaklow.Text}-{txtweakhigh.Text}";
                nodes.Item(0).SelectSingleNode("unacceptable").InnerText = $"{txtunacceptablelow.Text}-{txtunacceptablehigh.Text}";
                doc.Save(filename);
            }
            catch
            {
                XmlNode node;
                if (nodes == null)
                {
                    node = doc.CreateNode(XmlNodeType.Element, "Settings", null);
                    doc.DocumentElement.AppendChild(node);
                }
                else
                    node = nodes.Item(0);
                //create title node
                XmlNode highrange = doc.CreateElement("highrange");
                XmlNode midrange = doc.CreateElement("midrange");
                XmlNode lowrange = doc.CreateElement("lowrange");
                XmlNode unacceptable = doc.CreateElement("unacceptable");
                //add value for it
                node.AppendChild(highrange);
                highrange.InnerText = "5000";
                node.AppendChild(midrange);
                midrange.InnerText = "2000-5000";
                node.AppendChild(lowrange);
                lowrange.InnerText = "700-2000";
                node.AppendChild(unacceptable);
                unacceptable.InnerText = "3500-5000";
                //save back
                doc.Save(filename);
            }
            Cursor = Cursors.Default;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            string fileName = String.Format("{0}\\LIFECODES\\RUSHSettings.xml", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            SaveData(fileName);
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
