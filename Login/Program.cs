using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Cpdaily;
using Cpdaily.CpdailyModels;
using Newtonsoft.Json;
using RestSharp;
using Serilog;

namespace Login
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

        }

        public static async Task<IEnumerable<LibraryReservation>> GetLibraryReservationsAsync(string cookies)
        {
            var WebUserAgent = "Mozilla/5.0 (Linux; Android 5.1.1; vmos Build/LMY48G; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/52.0.2743.100 Mobile Safari/537.36  cpdaily/8.2.16 wisedu/8.2.16";
            string url = "http://libopp.cqjtu.edu.cn/myorder.asp";
            RestClient client = new RestClient(url)
            {
                UserAgent = WebUserAgent
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("Cookie", cookies);
            request.AddHeader("X-Requested-With", "com.wisedu.cpdaily");
            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            Regex regex = new Regex(@"(\d\d\d\d-\d\d-\d\d)[\s\S]+?(南岸馆|双福馆)[^=]+(=myorder\.asp\?cz=del&id=(\d+))?");
            var matches = regex.Matches(response.Content);
            List<LibraryReservation> result = new List<LibraryReservation>();
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var lr = new LibraryReservation()
                    {
                        Date = DateTime.ParseExact(match.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        LibraryName = match.Groups[2].Value,
                        Id = match.Groups.Count >= 5 ? match.Groups[4].Value : ""
                    };
                    result.Add(lr);
                }
            }
            return result;
        }

    }
    public class LibraryReservation
    {
        public string Id { get; set; }
        public string LibraryName { get; set; }
        public DateTime Date { get; set; }
    }
}