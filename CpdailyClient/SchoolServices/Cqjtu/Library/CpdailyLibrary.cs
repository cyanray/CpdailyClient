using Cpdaily.Exceptions;
using RestSharp;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace Cpdaily.SchoolServices.Cqjtu.Library
{
    public class CpdailyLibrary
    {
        public async Task<string> LoginAsync(string cookie)
        {
            string? url = "http://libopp.cqjtu.edu.cn/";
            string host = url;
            var cookieContainer = new CookieContainer();
            MatchCollection matchCollection = Regex.Matches(cookie, @"([^=]+)=([^;]+);?\s*");
            foreach (Match item in matchCollection)
            {
                if (item.Groups.Count != 3) continue;
                cookieContainer.Add(new Uri(url), new Cookie(item.Groups[1].Value, item.Groups[2].Value));
                cookieContainer.Add(new Uri("http://ids.cqjtu.edu.cn/"), new Cookie(item.Groups[1].Value, item.Groups[2].Value));
            }
            do
            {
                if (!url.StartsWith("http"))
                {
                    url = $"{host}{url}";
                }
                RestClient client = new RestClient(url)
                {
                    CookieContainer = cookieContainer,
                    FollowRedirects = false,
                    UserAgent = Constants.UserAgent
                };
                var request = new RestRequest(Method.GET);
                request.AddHeader("Upgrade-Insecure-Requests", "1");
                request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                request.AddHeader("X-Requested-With", "com.wisedu.cpdaily");
                var response = await client.ExecuteGetAsync(request);
                url = response.Headers.Where(x => x.Name == "Location").Select(x => x.Value.ToString()).FirstOrDefault();
            } while (!string.IsNullOrEmpty(url));
            string result = string.Join(" ", cookieContainer.GetAllCookies().Select(x => $"{x.Name}={x.Value};"));
            return result;
        }

        public async Task ReserveAsync(string cookie, string libraryName, DateTime date)
        {
            string url = $"http://libopp.cqjtu.edu.cn/tsg.asp?riqi={date:yyyy-MM-dd}&quyu={libraryName}&vehicle=同意";
            RestClient client = new RestClient(url)
            {
                UserAgent = Constants.UserAgent
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
                throw new HttpRequestFailedException(response.StatusCode);
        }

        public async Task CancelReservationsAsync(string cookie, string Id)
        {
            string url = $"http://libopp.cqjtu.edu.cn/myorder.asp?cz=del&id={Id}";
            RestClient client = new RestClient(url)
            {
                UserAgent = Constants.UserAgent
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("Cookie", cookie);
            request.AddHeader("X-Requested-With", "com.wisedu.cpdaily");
            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestFailedException(response.StatusCode);
        }

        public async Task<List<LibraryReservationLog>> GetReservationsAsync(string cookie)
        {
            string url = "http://libopp.cqjtu.edu.cn/myorder.asp";
            RestClient client = new RestClient(url)
            {
                UserAgent = Constants.UserAgent
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("Cookie", cookie);
            request.AddHeader("X-Requested-With", "com.wisedu.cpdaily");
            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestFailedException(response.StatusCode);
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
