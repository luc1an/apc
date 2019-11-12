using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace apc.bussinesslayer.core
{
    public class Helper
    {
        private const string UaUrl = "http://" + "www.user-agents.org/index.shtml?n_s";
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

                    if (ua == null) return _userAgents;
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

                return _userAgents;
            }
        }
    }
}
