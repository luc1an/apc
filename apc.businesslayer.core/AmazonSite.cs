using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using HtmlAgilityPack;

namespace apc.businesslayer.core
{
    public class AmazonSite
    {
        //at some point i used random user agents while parsing the website(s). not anymore.
        private const string UaUrl = "http://" + "www.user-agents.org/index.shtml?n_s";

        #region Fields

        public bool Valid { get; set; }
        public string ProductName { get; set; }
        public string ProductLink { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal VAT { get; set; }
        public decimal Netto { get; set; }
        public decimal VATAmount { get; set; }
        public string MoneyAmt => MoneyValue.ToString(CultureInfo.InvariantCulture);
        private decimal _moneyValue;
        public decimal MoneyValue
        {
            get => _moneyValue;
            set
            {
                _moneyValue = value;
                // this is the final price
                // the VAT is... 
                Netto =  Math.Round(_moneyValue / (1 + (VAT / 100)), 2) ; // with 2 digits!
                VATAmount = _moneyValue - Netto;
            } 
        }
        public string Currency { get; set; }

        #endregion

        #region Methods

        public void BreakPriceText(string price)
        {
            price = price.Trim();
            //EUR 36,99 - EUR 98,40
            if (price.IndexOf("-") > 0)
            {
                price = price.Substring(0, price.IndexOf("-")).Trim();
            }
            string[] t = price.Split(' ');
            if (t.Length == 1)
            {

                if (price.StartsWith("$"))
                {
                    t = new[] {"$", price.Replace("$", string.Empty)};
                }
                else
                {
                    if (price.StartsWith("£"))
                    {
                        t = new[] { "£", price.Replace("£", string.Empty) };
                    }
                    else
                        if (price.Contains("£"))
                    {
                        t = new[] { "£", price.Substring(price.IndexOf("£") + 1) };
                    }
                    else
                        t = new[] { "XX", price };
                }
            }

            var altCurrency = ">>";
            if (Name == "Japan")
            {
                MoneyValue = Convert.ToDecimal(t[1]);
                Currency = "JPY";
            }
            else
            {
                string cultureName;
                switch (Name)
                {
                    case "Australia":
                        cultureName = "en-US";
                        altCurrency = "AUD";
                        break;
                    case "Japan":
                        cultureName = "jp";
                        break;
                    case "USA":
                        cultureName = "en-US";
                        altCurrency = "USD";
                        break;
                    case "Canada":
                        cultureName = "en-US";
                        altCurrency = "CAD";
                        break;
                    case "UK":
                        cultureName = "en-GB";
                        altCurrency = "GBP";
                        break;

                    case "Germany":
                    case "Italy":
                    case "Spain":
                    case "France":
                        cultureName = "it-IT";
                        altCurrency = "EUR";
                        break;
                    default:
                        cultureName = "zh-CHT";
                        break;

                }
                var cultureInfo = new CultureInfo(cultureName);
                if (t[0] == altCurrency)
                {
                    var pp = price.Replace(altCurrency, string.Empty).Replace(" ",string.Empty).Trim();
                    MoneyValue = Convert.ToDecimal(pp, cultureInfo.NumberFormat);
                }
                else
                    MoneyValue = Convert.ToDecimal(t[1], cultureInfo.NumberFormat);
            }
            switch (t[0])
            {
                case "£":
                    Currency = "GBP";
                    break;
                case "EUR":
                    Currency = "EUR";
                    break;
                case "$":
                    Currency = "USD";
                    if (Name == "Australia")
                        Currency = "AUD";                    
                    break;
                case "CDN$":
                    Currency = "CAD";
                    break;
                case "￥":
                    Currency = "JPY";
                    break;
                case "XX":
                    Currency = altCurrency;
                    break;
            }

        }

        #endregion

        #region Static methods

        private static List<string> _userAgents;

        public static List<string> UserAgents
        {
            get
            {
                if (_userAgents == null)
                {
                    _userAgents = new List<string>
                        {
                            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11"
                        };

                    HtmlDocument ua = new HtmlWeb().Load(UaUrl);

                    if (ua != null)
                    {
                        var coll = ua.DocumentNode.SelectNodes("//tr/td[@class='left']");

                        if (coll != null)
                            _userAgents =
                                coll.Select(
                                    node =>
                                    node.InnerText.Replace("&amp;", "&")
                                        .Replace("&nbsp;", " ")
                                        .Replace("\n", " ")
                                        .Trim()).ToList();
                    }
                }

                return _userAgents;
            }
        }

        private static Random _rnd;

        private static string RandomUserAgent
        {
            get
            {
                if (_rnd == null)
                {

                    _rnd = new Random(1);
                }
                int uac = UserAgents.Count;
                int rndr = _rnd.Next(uac);
                return UserAgents[rndr];
            }
        }


        public static List<AmazonSite> GetList(string rootPath=null)
        {
            string appDataPath = null;
            appDataPath = rootPath;
            
            if (appDataPath == null) return null;
            string file = Path.Combine(appDataPath, "AmazonSites.xml");
            const string xpath = "//site/*";
            var documentitem = XDocument.Load(file);

            var itemz = documentitem.XPathSelectElements(xpath);

            return
                itemz.Select(
                    item =>
                        new AmazonSite
                        {
                            Name = item.Attribute("name").Value,
                            Url = item.Attribute("value").Value,
                            VAT = Convert.ToDecimal(item.Attribute("vat").Value),
                            Valid = true
                        }).ToList();
        }

#pragma warning disable 1998
        public async Task FetchPrice(string asin)
#pragma warning restore 1998
        {
            var cl = new WebClient();
            if (Url != null)
            {
                string url = string.Format(Url, asin);
                cl.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                //cl.Headers.Add("user-agent", RandomUserAgent);
                string htmlDocument;
                try
                {
                    htmlDocument = cl.DownloadString(url);
                    htmlDocument = Regex.Replace(htmlDocument, "[^\u0000-\u007F]", " ");
                    htmlDocument = htmlDocument.Replace("\n", string.Empty);
                }
                catch 
                {
                    Valid = false;
                    MoneyValue = -1;
                    return;
                }
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlDocument);
                ProductLink = url;
                // product name
                try
                {
                    var pname = doc.DocumentNode.Descendants("title").FirstOrDefault();
                    if (pname != null) ProductName = pname.InnerText;
                    var link = doc.DocumentNode.Descendants("link").FirstOrDefault(l => l.Attributes["rel"].Value == "canonical");
                    if (link != null)
                    {
                        this.ProductLink = link.Attributes["href"].Value;
                    }                    
                }
                catch
                {
                    ProductName = "n/a";
                }
                
                // in happy cases, the price will be held in a span with id "actualPriceValue". That span is in a table row (tablerowid: actualPriceRow)
                // that table is in the main form . id of the form is handleBuy
                var actualPriceValue = doc.DocumentNode.Descendants("span").FirstOrDefault(sc => sc.Id == "actualPriceValue");
                var ourPrice = doc.DocumentNode.Descendants("span").FirstOrDefault(sc => sc.Id == "priceblock_ourprice");
                var dealPrice = doc.DocumentNode.Descendants("span").FirstOrDefault(sc => sc.Id == "priceblock_dealprice");
                var headerPrice = doc.DocumentNode.Descendants("span").FirstOrDefault(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("header-price") && d.Attributes["class"].Value.Contains("a-color-price"));
                if (headerPrice != null)
                {
                    BreakPriceText(headerPrice.InnerText);
                }
                else
                if (dealPrice != null)
                {
                    BreakPriceText(dealPrice.InnerText);
                }
                else if (ourPrice != null)
                {
                    BreakPriceText(ourPrice.InnerText);
                }
                else
                if (actualPriceValue == null)
                {
                    var priceBlock = doc.DocumentNode.Descendants("div").FirstOrDefault(sc => sc.Id == "priceBlock");
                    // try with the priceblock and find inside 
                    if (priceBlock == null)
                    {
                        return;
                    }
                    bool foundBlock = false;
                    var pric = priceBlock.Descendants("b").FirstOrDefault();
                    try
                    {
                        var q = priceBlock.Descendants("b").ToList();
                        foreach (HtmlNode node in q)
                        {
                            if (node.HasAttributes && node.Attributes["class"].Value.ToLower() == "pricelarge")
                            {
                                pric = node;
                                foundBlock = true;
                                break;
                            }
                        }
                    }
                    catch
                    {
                        pric = priceBlock.Descendants("b").FirstOrDefault();
                    }


                    if (foundBlock || pric == null)
                    {
                        if (!foundBlock)
                            pric =
                                priceBlock.Descendants("span")
                                          .FirstOrDefault(sc => sc.Attributes["class"].Value == "priceLarge");
                        if (pric == null)
                            BreakPriceText("0");
                        else
                        {
                            var s = pric.InnerText;
                            if (s.Contains("-"))
                            {
                                s = s.Split('-')[0].Trim();
                            }
                            BreakPriceText(s);
                        }
                    }
                    else
                    {
                        BreakPriceText(pric.InnerHtml);
                    }
                }
                else
                {
                    var pric = actualPriceValue.Descendants("b").FirstOrDefault();

                    BreakPriceText(pric != null ? pric.InnerText : actualPriceValue.InnerText);
                }
            }
            //ret.Add(site);
        }

        // ReSharper disable CSharpWarnings::CS1998
        public static List<AmazonSite> GetPrice(string asin, string rootPath)
        // ReSharper restore CSharpWarnings::CS1998
        {
            var sites = GetList(rootPath);
            var dtStart = DateTime.Now;
            // debug only
            const bool forceNonParallel = false;

            var options = new ParallelOptions { MaxDegreeOfParallelism = forceNonParallel ? Environment.ProcessorCount : -1 };
            Console.WriteLine("{options.MaxDegreeOfParallelism} number of parallel calls");
            //debug only
            //foreach (var site in sites)
            //{
            //    site.FetchPrice(asin);
            //}
            // trying to get the data five times and make a reunion
            Parallel.ForEach(sites, site => site.FetchPrice(asin).Wait());
            var ts = DateTime.Now - dtStart;
            Console.Write(ts.TotalSeconds);
            return sites.Where(c => (c.Valid && c.MoneyValue != 0)).ToList();
        }

        #endregion
    }


}
