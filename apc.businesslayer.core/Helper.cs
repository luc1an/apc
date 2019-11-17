using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
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

        public static string Detect(string link)
        {
            string encodedLink = HttpUtility.UrlDecode(link);

            var originalRegex = "/([a-zA-Z0-9]{10})(?:[/?]|$)";
            var reg2 = new Regex(originalRegex);
            var m1 = reg2.Match(encodedLink ?? throw new InvalidOperationException());
            if (m1.Success)
            {
                return m1.Groups[1].Value;
            }
            var regex = new Regex(@"(?:^[(http)(https)]://(?:www\.){0,1}amazon.(?:/.*|es|com|it|fr|de|co.uk|com.au|at|co.jp|ca|cn){0,1}(?: /dp/|/product/dp/|/gp/product/|gp/aw/d/))(.*?)(?:/.*|$)");

            var m = regex.Match(encodedLink);
            if (m.Success)
                return m.Groups[1].Value.Substring(0, 10);
            regex = new Regex("^[(http)(https)]://www.amazon.(?:/.*|es|com|it|fr|de|co.uk|at|co.jp|com.au|ca|cn){0,1}/([\\w-]+/)?(dp|gp/product|gp/aw/d)/(\\w+/)?(\\w{10})");
            m = regex.Match(encodedLink);
            return m.Groups[^1].Value.Substring(0, 10);
        }
    }
}
