using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Cookie = System.Net.Cookie;
using System.IO;
using HtmlAgilityPack;
using System.Web;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Newtonsoft.Json;

namespace exam
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
       
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/login";
            string username = txtUser.Text;
            string password = txtPass.Text;

            HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(url);

            var token = doc.DocumentNode.SelectNodes("/html/body/div[3]/main/div/div[4]/form/input[1]");
            string token_val = token[0].Attributes[2].Value;
            using var client = new HttpClient();

            CookieContainer cookieContainer = new CookieContainer();
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.CookieContainer = cookieContainer;
            req.Method = "GET";

            WebResponse resp = req.GetResponse();

            HttpWebResponse response1 = resp as HttpWebResponse;

            CookieCollection cookies;
            cookies = response1.Cookies;

            var a = cookies;

            var baseAddress = new Uri(url);
            var cookieContainer1 = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client1 = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var content = new FormUrlEncodedContent(new[]
                {
                 new KeyValuePair<string, string>("login", HttpUtility.UrlEncode(username, Encoding.UTF8)),
                 new KeyValuePair<string, string>("password", HttpUtility.UrlEncode(password, Encoding.UTF8)),
                 new KeyValuePair<string, string>("authenticity_token", HttpUtility.UrlEncode(token_val, Encoding.UTF8)),
                 new KeyValuePair<string, string>("commit", HttpUtility.UrlEncode("Sign in", Encoding.UTF8))
                });
                cookieContainer1.Add(baseAddress, new Cookie("_device_id", a[0].Value ));
                cookieContainer1.Add(baseAddress, new Cookie("_gh_sess", a[1].Value));
                cookieContainer1.Add(baseAddress, new Cookie("_octo", a[2].Value));

                client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",token_val);
                var authValue = new System.Net.Http.Headers.ProductInfoHeaderValue(username,"");
                client1.DefaultRequestHeaders.UserAgent.Add(authValue);
                var result = await client1.PostAsync("/settings/security-log", content);
                string con = result.Content.ReadAsStringAsync().Result;
                result.EnsureSuccessStatusCode();


                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(con);

                var acc = htmlDoc.DocumentNode.SelectNodes("/html/body/div[4]/main/div/div[1]/div/div/h1/a");


                jsonData data = new jsonData();
                data.account = acc[0].InnerText.ToString().Trim();
              
                foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("/html/body/div[4]/main/div/div[2]/div[2]/div/div[2]"))
                {
                    logs logs = new logs();
                    logs.events = node.ChildNodes[3].ChildNodes[3].InnerText.ToString();
                    logs.time = node.ChildNodes[5].ChildNodes[3].InnerText.ToString();
                }

                string strResultJson = JsonConvert.SerializeObject(data);

                txtResult.Text = strResultJson;
            }
             
        }

    }

    class jsonData
    {
        public logs logs;
        public string account { get; set; }
        
    }

    class logs
    {
        public string events { get; set; }
        public string time { get; set; }
    }
}
