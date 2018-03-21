using System;
using System.Collections.Generic;
using System.Text;
using ZofX.Library.Network;
using System.Text.RegularExpressions;
using ZofX.Library.Strings;

namespace ZofX.HtmlCollector.Core
{
    public static class HtmlHandler
    {
        public static string GetHtml(string url, string queryString, string postString, string pageParamName, int pageParamValue, bool pageInPostString)
        {
            if (pageInPostString)
            {
                return HttpHelper.Post(url + "?" + queryString, postString + "&" + pageParamName + "=" + pageParamValue);
            }
            else
            {
                return HttpHelper.Post(url + "?" + queryString + "&" + pageParamName + "=" + pageParamValue, postString);
            }
        }

        public static List<string> ParseHeader(string html, string headerRegEx, string maxPageRegEx, out int minPage, out int maxPage)
        {
            minPage = 1;
            maxPage = 0;
            List<string> list = new List<string>();

            Regex reg = new Regex(headerRegEx);
            Match m = reg.Match(html);
            int index = 0;
            foreach (Group g in m.Groups)
            {
                if (index++ == 0) continue;
                list.Add(g.Value.Trim());
            }

            Match mMinMax = new Regex(@"(\d+)/(\d+)").Match(maxPageRegEx);
            if (mMinMax.Groups.Count == 3)
            {
                int.TryParse(mMinMax.Groups[1].Value, out minPage);
                int.TryParse(mMinMax.Groups[2].Value, out maxPage);
            }
            else
            {
                reg = new Regex(maxPageRegEx);
                m = reg.Match(html);
                int.TryParse(m.Groups[1].Value, out maxPage);
            }

            return list;
        }

        public static List<List<string>> ParseBody(string detailUrl, string html, string regEx, string appendRegEx, int detailUrlIndex, bool ignoreFirst = false)
        {
            List<List<string>> list = new List<List<string>>();

            Regex reg = new Regex(regEx);
            MatchCollection mc = reg.Matches(html);
            foreach (Match m in mc)
            {
                List<string> item = new List<string>();
                int index = 0;
                foreach (Group g in m.Groups)
                {
                    if (index++ == 0) continue;
                    if (index - 1 == detailUrlIndex) continue;
                    item.Add(g.Value.Trim());
                }
                list.Add(item);
                if (detailUrlIndex <= 0) continue;
                item.AddRange(ParseDetailBody(detailUrl + m.Groups[detailUrlIndex].Value.Trim(), appendRegEx));
            }

            return list;
        }

        public static List<string> ParseDetailBody(string url, string regEx)
        {
            List<string> item = new List<string>();
            string html = HttpHelper.Post(url, "").Replace("\r", "").Replace("\n", "");
            Regex reg = new Regex(regEx);
            Match m = reg.Match(html);
            int index = 0;
            foreach (Group g in m.Groups)
            {
                if (index++ == 0) continue;
                item.Add(g.Value.Trim());
            }
            return item;
        }
    }
}