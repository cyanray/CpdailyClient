using HtmlAgilityPack;
using RestSharp;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cpdaily.LoginWorkers
{
    /// <summary>
    /// 通用 LoginWorker，适用于大多数学校。
    /// </summary>
    public class DefaultLoginWorker : ILoginWorker
    {
        private async Task<bool> NeedCaptcha(string urlRoot, CookieContainer cookieContainer, string username)
        {
            string url = $"{urlRoot}/authserver/needCaptcha.html?username={username}&pwdEncrypt2=pwdEncryptSalt&v={Random.Shared.NextDouble()}";
            RestClient LoginClient = new RestClient(url)
            {
                CookieContainer = cookieContainer
            };
            var request = new RestRequest(Method.GET);
            request.AddHeader("User-Agent", WebUserAgent);
            var response = await LoginClient.ExecuteGetAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            return response.Content.Contains("true");
        }

        public override async Task<LoginParameter> GetLoginParameter(string username, string password, string idsUrl)
        {
            LoginParameter loginParameter = new LoginParameter()
            {
                Username = username,
                Password = password,
                IdsUrl = idsUrl,
                NeedCaptcha = false
            };
            RestClient LoginClient = new RestClient(idsUrl)
            {
                CookieContainer = loginParameter.CookieContainer
            };
            var request = new RestRequest(Method.POST);
            request.AddHeader("User-Agent", WebUserAgent);
            var response = await LoginClient.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            string urlRoot = response.ResponseUri.GetLeftPart(UriPartial.Authority);

            var needCaptchaTask = NeedCaptcha(urlRoot, loginParameter.CookieContainer, username);
            loginParameter.CaptchaImageUrl = $"{urlRoot}/authserver/captcha.html";

            //** 解析 pwdDefaultEncryptSalt **//
            var matches = Regex.Matches(response.Content, "var pwdDefaultEncryptSalt *= *\"(.*?)\";");
            if (matches.Count <= 0) throw new Exception("没有匹配到 pwdDefaultEncryptSalt。");
            string pwdDefaultEncryptSalt = matches[0].Groups[1].Value;

            // update encryptedPwd
            byte[] iv = Encoding.ASCII.GetBytes(CpdailyCrypto.RandomString(16));
            string tPassword = CpdailyCrypto.RandomString(64) + password;
            string encryptedPwd = CpdailyCrypto.AESEncrypt(tPassword, pwdDefaultEncryptSalt, iv);
            loginParameter.EncryptedPassword = encryptedPwd;

            //** 解析 action、lt、execution **//
            var doc = new HtmlDocument();
            doc.LoadHtml(response.Content);
            string action = doc.DocumentNode
                .SelectNodes(@"//*[@id=""casLoginForm""]")[0]
                .GetAttributeValue("action", string.Empty);
            string lt = doc.DocumentNode
                .SelectNodes(@"//*[@id=""casLoginForm""]/input[1]")[0]
                .GetAttributeValue("value", string.Empty);
            string execution = doc.DocumentNode
                .SelectNodes(@"//*[@id=""casLoginForm""]/input[3]")[0]
                .GetAttributeValue("value", string.Empty);

            loginParameter.Parameters.Add("lt", lt);
            loginParameter.Parameters.Add("execution", execution);
            loginParameter.ActionUrl = urlRoot + action;

            loginParameter.NeedCaptcha = await needCaptchaTask;

            return loginParameter;
        }

        public override async Task<string> IdsLogin(LoginParameter loginParameter)
        {
            if (loginParameter.ActionUrl is null)
            {
                throw new ArgumentNullException(nameof(loginParameter.ActionUrl));
            }
            if (loginParameter.Username is null)
            {
                throw new ArgumentNullException(nameof(loginParameter.Username));
            }
            if (loginParameter.EncryptedPassword is null)
            {
                throw new ArgumentNullException(nameof(loginParameter.EncryptedPassword));
            }
            string? loginUrl = loginParameter.ActionUrl;
            IRestResponse? response = null;
            do
            {
                RestClient client = new RestClient(loginUrl)
                {
                    CookieContainer = loginParameter.CookieContainer,
                    FollowRedirects = false
                };
                var request = new RestRequest(Method.POST);
                request.AddHeader("User-Agent", WebUserAgent);
                request.AddParameter("username", loginParameter.Username);
                request.AddParameter("password", loginParameter.EncryptedPassword);
                request.AddParameter("lt", loginParameter.Parameters["lt"]);
                request.AddParameter("dllt", "mobileLogin");
                request.AddParameter("execution", loginParameter.Parameters["execution"]);
                request.AddParameter("_eventId", "submit");
                request.AddParameter("rmShown", "1");
                if (loginParameter.CaptchaValue is not null)
                {
                    request.AddParameter("captchaResponse", loginParameter.CaptchaValue);
                }
                response = await client.ExecutePostAsync(request);

                loginUrl = response.Headers.Where(x => x.Name == "Location").Select(x => x.Value.ToString()).FirstOrDefault();
            } while (!string.IsNullOrEmpty(loginUrl));
            var result = string.Join(" ", loginParameter.CookieContainer.GetAllCookies().Select(x => $"{x.Name}={x.Value};"));
            return result;
        }

        public override Task<string> GetEncrypedToken(LoginParameter loginParameter)
        {
            throw new NotImplementedException();
        }
    }
}
