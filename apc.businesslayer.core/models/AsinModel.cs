using System;
using System.Text.RegularExpressions;
using System.Web;

namespace apc.bussinesslayer.core.models
{
    public class AsinModel
    {
        public string Code { get; set; }

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