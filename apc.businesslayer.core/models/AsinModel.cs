using System;
using System.Text.RegularExpressions;
using System.Web;

namespace apc.businesslayer.core.models
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
            // http://www.amazon.com/dp/B000N0SQKI/ref=pe_139600_26796080_pe_vfe_dt1
            // http://www.amazon.com/Bakugan-BKG043-Black-Character-Strap/dp/B002M78NBW/ref=sr_1_2?s=watches&ie=UTF8&qid=1352728137&sr=1-2
            // http://www.amazon.com/gp/product/B000BYVKD6/ref=s9_simh_gw_p200_d13_i6?pf_rd_m=ATVPDKIKX0DER&pf_rd_s=center-4&pf_rd_r=07EYA5QESAR7M194MECF&pf_rd_t=101&pf_rd_p=470939031&pf_rd_i=507846
            var regex = new Regex(@"(?:^[(http)(https)]://(?:www\.){0,1}amazon.(?:/.*|es|com|it|fr|de|co.uk|com.au|at|co.jp|ca|cn){0,1}(?: /dp/|/product/dp/|/gp/product/|gp/aw/d/))(.*?)(?:/.*|$)");

            //Regex("http://www.amazon.*/([\\w-]+/)?(dp|gp/product)/(\\w+/)?(\\w{10})");
            var m = regex.Match(encodedLink);
            if (m.Success)
                return m.Groups[1].Value.Substring(0, 10);
            //http://www.amazon.de/gp/product/B008B8UBCA?psc=1&redirect=true&ref_=oh_aui_detailpage_o00_s00
            regex = new Regex("^[(http)(https)]://www.amazon.(?:/.*|es|com|it|fr|de|co.uk|at|co.jp|com.au|ca|cn){0,1}/([\\w-]+/)?(dp|gp/product|gp/aw/d)/(\\w+/)?(\\w{10})");
            m = regex.Match(encodedLink);
            return m.Groups[^1].Value.Substring(0, 10);
        }
    }
}