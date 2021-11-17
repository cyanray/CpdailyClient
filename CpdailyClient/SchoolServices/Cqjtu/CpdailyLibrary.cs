﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cpdaily.SchoolServices.Cqjtu
{
    public class CpdailyLibrary
    {
        const string UserAgent = "Mozilla/5.0 (Linux; Android 11; Redmi K30 5G Build/RKQ1.200826.002; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/86.0.4240.185 Mobile Safari/537.36  cpdaily/8.2.17 wisedu/8.2.17";

        public async Task ReserveAsync(string cookie, string libraryName, DateTime date)
        {
            string url = $"http://libopp.cqjtu.edu.cn/tsg.asp?riqi={date:yyyy-MM-dd}&quyu={libraryName}&vehicle=同意";
            RestClient client = new RestClient(url)
            {
                UserAgent = UserAgent
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            request.AddHeader("Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            request.AddHeader("Upgrade-Insecure-Requests", "1");
            request.AddHeader("Referer", "http://libopp.cqjtu.edu.cn/tsg.asp");
            request.AddHeader("X-Requested-With", "com.wisedu.cpdaily");
            request.AddHeader("Proxy-Connection", "keep-alive");
            request.AddHeader("Cookie", cookie);
            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            // TODO: parse error message.
        }

        public async Task CancelReservationsAsync(string cookies, string Id)
        {
            string url = $"http://libopp.cqjtu.edu.cn/myorder.asp?cz=del&id={Id}";
            RestClient client = new RestClient(url)
            {
                UserAgent = UserAgent
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("Cookie", cookies);
            request.AddHeader("X-Requested-With", "com.wisedu.cpdaily");
            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            // TODO: parse error message.
        }


        public async Task<List<LibraryReservationLog>> GetReservationsAsync(string cookie)
        {
            string url = "http://libopp.cqjtu.edu.cn/myorder.asp";
            RestClient client = new RestClient(url)
            {
                UserAgent = UserAgent
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("Cookie", cookie);
            request.AddHeader("X-Requested-With", "com.wisedu.cpdaily");
            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            Regex regex = new Regex(@"(\d\d\d\d-\d\d-\d\d)[\s\S]+?(南岸馆|双福馆)[^=]+(=myorder\.asp\?cz=del&id=(\d+))?");
            var matches = regex.Matches(response.Content);
            List<LibraryReservationLog> result = new List<LibraryReservationLog>();
            foreach (Match match in matches)
            {
                var log = new LibraryReservationLog()
                {
                    Date = DateTime.ParseExact(match.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    LibraryName = match.Groups[2].Value,
                    Id = match.Groups.Count >= 5 ? match.Groups[4].Value : null
                };
                result.Add(log);
            }
            return result;
        }
    }
}
