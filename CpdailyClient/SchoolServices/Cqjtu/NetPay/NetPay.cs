using Cpdaily.Exceptions;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cpdaily.SchoolServices.Cqjtu.NetPay
{
    public class NetPay
    {
        public async Task<string> LoginAsync(string cookie)
        {
            string? url = "http://218.194.173.2/CmbNetPay/validateLogin.do";
            string host = url;
            var cookieContainer = new CookieContainer();
            do
            {
                RestClient client = new RestClient(url)
                {
                    CookieContainer = cookieContainer,
                    FollowRedirects = false,
                    UserAgent = Constants.UserAgent
                };
                var request = new RestRequest(Method.GET);
                request.AddHeader("Cookie", cookie);
                var response = await client.ExecuteGetAsync(request);
                url = response.Headers.Where(x => x.Name == "Location").Select(x => x.Value.ToString()).FirstOrDefault();
            } while (!string.IsNullOrEmpty(url));
            string result = string.Join(" ", cookieContainer.GetAllCookies().Select(x => $"{x.Name}={x.Value};"));
            return result;
        }

        public async Task<AccountInfo> GetAccountInfoAsync(string cookie)
        {
            string url = $"http://218.194.173.2/CmbNetPay/getPersonInfo.do";
            RestClient client = new RestClient(url)
            {
                UserAgent = Constants.UserAgent
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("Cookie", cookie);
            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestFailedException(response.StatusCode);
            var info = JsonConvert.DeserializeObject<AccountInfo>(response.Content);
            if (info is null)
            {
                throw new Exception("无法解析响应结果!");
            }
            return info;
        }

    }
}
