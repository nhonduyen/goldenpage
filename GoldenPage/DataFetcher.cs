using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OfficeOpenXml;
using Supremes;

namespace GoldenPage
{
    public class DataFetcher
    {
        public async Task<string> GetTaxNumber(string url)
        {
            double flag;
            var result = string.Empty;
            var doc1 = Dcsoup.Parse(new Uri(url), 10000);
            foreach (var item in doc1.Select("div[class=hosocongty_text]"))
            {
                var taxString = item.Text;
                if (taxString.Length >= 7 && double.TryParse(taxString, out flag))
                {
                    result = taxString;
                    break;
                }
            }
            var t = await Task.FromResult<string>(result);
            return t;
        }

        public async Task<int> GetNumberOfPages(string url)
        {
            var numberOfPages = 0;
            try
            {
                var doc1 = Dcsoup.Parse(new Uri(url), 10000);
                var a = doc1.Select("div[id=paging]");

                foreach (var item in doc1.Select("div[id=paging]"))
                {
                    var pages = item.Select("a[href]");
                    if (pages != null)
                    {
                        foreach (var page in pages)
                        {
                            if (page.Attr("href").Contains("page"))
                            {
                                var resultString = Regex.Match(page.Attr("href"), @"\d+").Value;
                                var pageNumber = 0;
                                Int32.TryParse(resultString, out pageNumber);
                                if (pageNumber > numberOfPages)
                                {
                                    numberOfPages = pageNumber;
                                }
                            }
                        }
                    }
                }
            }
            catch (AggregateException ex)
            {
                foreach (var err in ex.InnerExceptions)
                {
                    Console.WriteLine(err.Message);
                }

            }
            numberOfPages = await Task.FromResult<int>(numberOfPages);
            return numberOfPages;
        }

        public async Task<List<Data>> GetInfo(string url)
        {
            var datas = new List<Data>();
            try
            {
                var doc1 = Dcsoup.Parse(new Uri(url), 10000);

                foreach (var item in doc1.Select("div[class=boxlistings]"))
                {
                    var txtName = item.Select("div[class=company_name]").Text;
                    var name = string.IsNullOrEmpty(txtName) ? item.Select("h2[class=company_name]").Text : txtName;
                    var address = item.Select("p[class=diachisection]").Last.Text;
                    var phone = item.Select("p[class=thoaisection]").Text;
                    var email = item.Select("div[class=email_text]").Select("a").Attr("title");
                    var website = item.Select("div[class=website_text]").Select("a").Attr("href");
                    var data = new Data()
                    {
                        Name = name,
                        DetailUrl = website,
                        Phone = phone,
                        Enail = email,
                        Address = address
                    };
                    datas.Add(data);
                }
            }
            catch (AggregateException ex)
            {
                foreach (var err in ex.InnerExceptions)
                {
                    Console.WriteLine(err.Message);
                }

            }
            return datas;
        }
        public void WriteToExcel(List<Data> data, string filename)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(filename)))
            {

                var ws = package.Workbook.Worksheets.Add("Sheet1");
                try
                {
                    ws.Cells["A1"].Value = "NAME";
                    ws.Cells["B1"].Value = "ADDRESS";
                    ws.Cells["C1"].Value = "WEBSITE";
                    ws.Cells["D1"].Value = "EMAIL";
                    ws.Cells["E1"].Value = "PHONE";
                    ws.Cells["A2"].LoadFromCollection(data);
                    ws.Column(1).Width = 50;
                    ws.Column(2).Width = 100;
                    ws.Column(3).Width = 20;
                    ws.Column(4).Width = 30;
                    ws.Column(5).Width = 30;
                    package.Save();
                }
                catch (AggregateException ex)
                {
                    foreach (var err in ex.InnerExceptions)
                    {
                        Console.WriteLine(err.Message);
                    }

                }
            }
        }
    }
}
