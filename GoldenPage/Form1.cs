using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;

namespace GoldenPage
{
    public partial class Form1 : Form
    {
        private const string goldenPageUrl = "https://trangvangvietnam.com/";
        private string filePath = string.Empty;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnOpenFile.Enabled = false;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    lblFileLocation.Text = fbd.SelectedPath;
                }
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblFileLocation.Text))
            {
                MessageBox.Show("Please choose a folder to save file");
                return;
            }
            btnOpenFile.Enabled = false;
            var dataFetcher = new DataFetcher();
            var searchUrl = txtUrl.Text.Trim();
            if (!string.IsNullOrWhiteSpace(searchUrl))
            {
                txtResultStatus.Text = string.Empty;
                var filename = $"{DateTime.Now.ToString("yyMMddHHmmss")}.xlsx";
                filePath = Path.Combine(lblFileLocation.Text, filename);
                List<Data> infoData = new List<Data>();

                var url = searchUrl;
                txtResultStatus.AppendText($"[{DateTime.Now}] Fetching page {url}{Environment.NewLine}");
                var numberOfPage = await Task<int>.Run(() => dataFetcher.GetNumberOfPages(url));
                txtResultStatus.AppendText($"[{DateTime.Now}] Number of page [{numberOfPage}]{Environment.NewLine}");
                for (int i = 1; i <= numberOfPage; i++)
                {
                    var pageUrl = $"{url}?page={i}";
                    txtResultStatus.AppendText($"[{DateTime.Now}] Start Fetching page {pageUrl}{Environment.NewLine}");
                    var data = await Task<List<Data>>.Run(() => dataFetcher.GetInfo(pageUrl));
                    infoData.AddRange(data);
                    txtResultStatus.AppendText($"[{DateTime.Now}] Fetching page done {pageUrl}{Environment.NewLine}");
                }
                txtResultStatus.AppendText($"[{DateTime.Now}] Fetching page done {Environment.NewLine}");
                if (infoData.Count > 0)
                {
                    txtResultStatus.AppendText($"[{DateTime.Now}] Export to excel {Environment.NewLine}");
                    dataFetcher.WriteToExcel(infoData, filePath);
                    txtResultStatus.AppendText($"[{DateTime.Now}] Export to excel done {filePath} {Environment.NewLine}");
                    btnOpenFile.Enabled = true;
                    MessageBox.Show("Export data done");
                }
                else
                {
                    txtResultStatus.AppendText($"[{DateTime.Now}] Data empty {Environment.NewLine}");
                }
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                System.Diagnostics.Process.Start(filePath);
            }
        }
    }
}
