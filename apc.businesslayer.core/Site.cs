using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace apc.bussinesslayer.core
{
    /// <summary>
    /// The main class used to get the price for a product.
    /// </summary>
    public class Site
    {
        //at some point i used random user agents while parsing the website(s). not anymore.
    
        #region Fields

        public string Name { get; set; }
        public decimal Vat { get; set; }
        public string Url { get; set; }
        #endregion


        #region Methods


        #endregion

        #region Static methods


        /// <summary>
        /// Returns the list of amazon sites used to check the prices.
        /// </summary>
        /// <param name="rootPath">The xml file root path where the amazon sites info is kept. Can be null (in this case, the root folder of the app is used)</param>
        /// <returns></returns>
        private static List<Site> BuildSitesList(string rootPath=null)
        {
            var appDataPath = rootPath;
            
            if (appDataPath == null) return null;
            string file = Path.Combine(appDataPath, "AmazonSites.xml");
            const string xpath = "//site/*";
            var documentItem = XDocument.Load(file);

            var itemz = documentItem.XPathSelectElements(xpath);

            return
                itemz.Select(
                    item =>
                        new Site
                        {
                            Name = item.Attribute("name")?.Value,
                            Url = item.Attribute("value")?.Value,
                            Vat = Convert.ToDecimal(item.Attribute("vat")?.Value)
                        }).ToList();
        }



        /// <summary>
        /// Returns the price list for a given <param name="asin">Amazon Standard Identification Number</param>
        /// </summary>
        /// <param name="asin"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        public static List<Product> GetPrices(string asin, string rootPath)
        // ReSharper restore CSharpWarnings::CS1998
        {
            var sites = BuildSitesList(rootPath);
            var ret = new List<Product>();
            //ret = sites.Select(site => site.FetchProduct(asin)).ToList();
            Parallel.ForEach(sites, site => { ret.Add(site.FetchProduct(asin)); });
            return ret.Where(c => (c.Valid && c.MoneyValue != 0)).ToList();
        }

        private Product FetchProduct(string asin)
        {
            return new Product(asin, this);
        }
        #endregion
    }


}
