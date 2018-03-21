using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZofX.Library.File;
using ZofX.DAL;
using ZofX.HtmlCollector.Core;
using System.Threading;
using System.Windows.Threading;
using ZofX.Library.Strings;

namespace ZofX.HtmlCollector.WpfForm
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Database db;
        private List<string> lstHeader;
        private string strHeader = "";
        private int minPage;
        private int maxPage;

        private string url;
        private string detailUrl;
        private string get;
        private string post;
        private string pageParamName;
        private int firstPageIndex = 1;
        private string pageParamPos;
        private int detailUrlIndex;
        private string bodyRegEx;
        private string appendBodyRegEx;
        private string tableName;
        private string key;
        private int keyIndex;
        private string headerRegEx;
        private string maxPageRegEx;
        private string appendHeader;
        private string connType;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbbConnType.Items.Add("OleDb");
            cbbConnType.Items.Add("MsSql");
            cbbConnType.Items.Add("MySql");
            cbbConnType.Items.Add("PgSql");
            cbbConnType.Items.Add("SQLite");
            cbbConnType.SelectedIndex = 0;

            cbbPageParamPos.Items.Add("POST");
            cbbPageParamPos.Items.Add("GET");

            IniHelper ini = new IniHelper(AppDomain.CurrentDomain.BaseDirectory + "config.ini");
            string connType = ini.Read("Database", "ConnType");
            cbbConnType.SelectedItem = connType;
            txtConnStr.Text = ini.Read("Database", connType + "ConnStr");
            txtTableName.Text = ini.Read("Database", "TableName");

            txtUrl.Text = ini.Read("Connector", "Url");
            txtPageParamName.Text = ini.Read("Connector", "PageParamName");
            cbbPageParamPos.SelectedItem = ini.Read("Connector", "PageParamPos");
            txtFirstPageIndex.Text = ini.Read("Connector", "FirstPageIndex");
            txtDetailUrl.Text = ini.Read("Connector", "DetailUrl");
            txtGET.Text = ini.Read("Connector", "GET");
            txtPOST.Text = ini.Read("Connector", "POST");
            txtKey.Text = ini.Read("Connector", "Key");
            txtHeaderRegEx.Text = ini.Read("Connector", "HeaderRegEx");
            txtAppendHeader.Text = ini.Read("Connector", "AppendHeader");
            txtBodyRegEx.Text = ini.Read("Connector", "BodyRegEx");
            txtAppendBodyRegEx.Text = ini.Read("Connector", "AppendBodyRegEx");
            txtMaxPageRegEx.Text = ini.Read("Connector", "MaxPageRegEx");
        }

        private void btnConn_Click(object sender, RoutedEventArgs e)
        {
            connType = cbbConnType.SelectedItem.ToString();
            btnConn.IsEnabled = false;
            if (btnConn.Content.ToString() == "断开")
            {
                btnConn.Content = "连接";
                btnHeader.IsEnabled = false;
                btnBody.IsEnabled = true;
                btnConn.IsEnabled = true;
                return;
            }
            try
            {
                db = new Database(connType, txtConnStr.Text);
                lstHeader = null;
                strHeader = "";
                maxPage = 0;
                btnConn.Content = "断开";
                btnHeader.IsEnabled = true;
                btnBody.IsEnabled = true;
                btnConn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                lblTip.Content = null == ex.InnerException ? ex.Message : ex.InnerException.Message;
            }
        }

        private void btnHeader_Click(object sender, RoutedEventArgs e)
        {
            btnHeader.IsEnabled = false;
            btnConn.IsEnabled = false;

            url = txtUrl.Text.Trim();
            detailUrl = txtDetailUrl.Text.Trim();
            get = txtGET.Text.Trim();
            post = txtPOST.Text.Trim();
            pageParamName = txtPageParamName.Text.Trim();
            pageParamPos = cbbPageParamPos.SelectedItem.ToString();
            if (!int.TryParse(txtFirstPageIndex.Text.Trim(), out firstPageIndex)) firstPageIndex = 1;
            bodyRegEx = txtBodyRegEx.Text.Trim();
            headerRegEx = txtHeaderRegEx.Text.Trim();
            maxPageRegEx = txtMaxPageRegEx.Text.Trim();
            appendHeader = txtAppendHeader.Text.Trim();
            appendBodyRegEx = txtAppendBodyRegEx.Text.Trim();
            key = txtKey.Text.Trim();
            tableName = txtTableName.Text.Trim();            

            string html = HtmlHandler.GetHtml(url, get, post, pageParamName, 1, pageParamPos == "POST");
            lstHeader = HtmlHandler.ParseHeader(html, headerRegEx, maxPageRegEx, out minPage, out maxPage);
            minPage -= (1 - firstPageIndex);
            maxPage -= (1 - firstPageIndex);
            if (!string.IsNullOrEmpty(appendHeader))
            {
                int index = appendHeader.IndexOf('-');
                if (index > 0)
                {
                    int.TryParse(appendHeader.Substring(0, index), out detailUrlIndex);
                }
                if (index + 1 < appendHeader.Length)
                {
                    lstHeader.AddRange(appendHeader.Substring(index + 1).Split(','));
                }
            }
            if (lstHeader.Count <= 0)
            {
                lblTip.Content = "未获取到表头。";
                btnHeader.IsEnabled = true;
                btnConn.IsEnabled = true;
                return;
            }

            try
            {
                db.Execute("drop table " + tableName + "");
            }
            catch { }
            string dataType = connType == "OleDb" ? "text" : "nvarchar(MAX)";
            strHeader = "";
            StringBuilder sbFields = new StringBuilder();
            keyIndex = 0;
            for (int index = 0; index < lstHeader.Count; index++)
            {
                sbFields.Append(string.Format("{0} {1},", lstHeader[index].Trim(), dataType));
                strHeader += "" + lstHeader[index].Trim() + ",";
                if (lstHeader[index].Trim() == key)
                    keyIndex = index;
            }
            string sql = string.Format(@"create table {0}(
                                            {1}
                                         )", tableName, sbFields.ToString().TrimEnd(','));
            try
            {
                db.Execute(sql);
                lblTip.Content = "表头获取成功，表创建成功，共" + (maxPage + (1 - firstPageIndex)) + "页。";
            }
            catch (Exception ex)
            {
                lblTip.Content = null == ex.InnerException ? ex.Message : ex.InnerException.Message;
            }
            btnBody.IsEnabled = true;
            btnHeader.IsEnabled = true;
            btnConn.IsEnabled = true;
        }

        private void btnBody_Click(object sender, RoutedEventArgs e)
        {
            btnBody.IsEnabled = false;
            btnHeader.IsEnabled = false;
            btnConn.IsEnabled = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(GetData), null);
        }

        private void GetData(object satte)
        {
            try
            {
                db.Execute("delete from " + tableName + "");
            }
            catch { }
            string html = null;
            string strField = null;
            List<List<string>> list = null;
            StringBuilder sbSql = new StringBuilder();            
            for (int page = minPage; page <= maxPage; page++)
            {
                lblTip.Dispatcher.Invoke(new Action<string>(ShowTip), "当前页码：" + (page + (1 - firstPageIndex)) + "/" + (maxPage + (1 - firstPageIndex)));
                html = HtmlHandler.GetHtml(url, get, post, pageParamName, page, pageParamPos == "POST");
                list = HtmlHandler.ParseBody(detailUrl, html, bodyRegEx, appendBodyRegEx, detailUrlIndex);

                //sbSql.Clear();
                foreach (List<string> item in list)
                {
                    strField = "";
                    foreach (string value in item)
                    {
                        strField += "'" + FilterHelper.HtmlFilter(value) + "',";
                    }
                    if (item.Count != lstHeader.Count)
                    {
                        for (var i = item.Count + 1; i <= lstHeader.Count; i++)
                        {
                            strField += "'',";
                        }
                    }
                    //sbSql.Append(string.Format("insert into {0}({1}) values({2});", tableName, strHeader.TrimEnd(','), strField.TrimEnd(',')));
                    if ((int)db.GetScalar(string.Format("select count(1) from {0} where {1}='{2}'", tableName, key, item[keyIndex])) > 0)
                        continue;
                    db.Execute(string.Format("insert into {0}({1}) values({2});", tableName, strHeader.TrimEnd(','), strField.TrimEnd(',')));
                }
                //db.Execute(sbSql.ToString());
            }
            lblTip.Dispatcher.Invoke(new Action<string>(ShowTip), "操作完成");
            btnBody.Dispatcher.Invoke(new Action(() => { btnBody.IsEnabled = true; }));
            btnConn.Dispatcher.Invoke(new Action(() => { btnConn.IsEnabled = true; }));
            btnHeader.Dispatcher.Invoke(new Action(() => { btnHeader.IsEnabled = true; }));
        }

        private void ShowTip(string tip)
        {
            lblTip.Content = tip;
        }
    }
}
