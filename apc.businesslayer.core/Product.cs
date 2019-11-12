using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace apc.bussinesslayer.core
{
    public class Product
    {

        #region Fields
        public Site AmazonSite { get; set; }
        public string Asin { get; set; }
        public bool Valid { get; set; }
        public string ProductName { get; set; }
        public string ProductLink { get; set; }
        public decimal Netto { get; set; }
        public decimal VatAmount { get; set; }
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
                Netto = Math.Round(_moneyValue / (1 + (AmazonSite.Vat / 100)), 2); // with 2 digits!
                VatAmount = _moneyValue - Netto;
            }
        }
        public string Currency { get; set; }

        #endregion

        /// <summary>
        /// Creates the price structure 
        /// </summary>
        /// <param name="price"></param>
        private void BreakPriceText(string price)
        {
            price = price.Trim();
            //EUR 36,99 - EUR 98,40
            if (price.IndexOf("-", StringComparison.Ordinal) > 0)
            {
                price = price.Substring(0, price.IndexOf("-", StringComparison.Ordinal)).Trim();
            }
            string[] t = price.Split(' ');
            if (t.Length == 1)
            {

                if (price.StartsWith("$"))
                {
                    t = new[] { "$", price.Replace("$", string.Empty) };
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
                        t = new[] { "£", price.Substring(price.IndexOf("£", StringComparison.Ordinal) + 1) };
                    }
                    else
                        t = new[] { "XX", price };
                }
            }

            var altCurrency = ">>";
            if (AmazonSite.Name == "Japan")
            {
                MoneyValue = Convert.ToDecimal(t[1]);
                Currency = "JPY";
            }
            else
            {
                string cultureName;
                switch (AmazonSite.Name)
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
                    var pp = price.Replace(altCurrency, string.Empty).Replace(" ", string.Empty).Trim();
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
                    if (AmazonSite.Name == "Australia")
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

        /// <summary>
        /// Fills the price data for a given ASIN
        /// </summary>
        /// <returns></returns>
        public void FetchPrice()
        {
            var cl = new WebClient();
            if (AmazonSite.Url == null) return;
            string url = string.Format(AmazonSite.Url, Asin);
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

            Valid = true;
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
                var price = actualPriceValue.Descendants("b").FirstOrDefault();
                BreakPriceText(price != null ? price.InnerText : actualPriceValue.InnerText);
            }
            //ret.Add(site);
        }

        public Product(string asin, Site amazonSite)
        {
            Asin = asin;
            AmazonSite = amazonSite;
            // go get it!
            FetchPrice();
        }
    }
}
