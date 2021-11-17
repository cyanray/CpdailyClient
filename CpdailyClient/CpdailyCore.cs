using Cpdaily.CpdailyModels;
using Cpdaily.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cpdaily
{
    /// <summary>
    /// Cpdaily Client
    /// </summary>
    public partial class CpdailyClient
    {
        /// <summary>
        /// 设备信息
        /// </summary>
        public DeviceInfo DeviceInfo { get; set; } = new DeviceInfo()
        {
            AppVersion = "8.2.16",
            SystemName = "android",
            SystemVersion = "5.1.1",
            DeviceId = "vmosserivmosvmos",
            Model = "vmos",
            Longitude = 0,
            Latitude = 0,
            UserId = ""
        };

        private const string ApiUserAgent = "Mozilla/5.0 (Linux; Android 5.1.1; vmos Build/LMY48G; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/52.0.2743.100 Mobile Safari/537.36 okhttp/3.12.4";
        private const string WebUserAgent = "Mozilla/5.0 (Linux; Android 5.1.1; vmos Build/LMY48G; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/52.0.2743.100 Mobile Safari/537.36  cpdaily/8.2.16 wisedu/8.2.16";
        private const string SaltForGetSecretKey = "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824";
        private readonly CookieContainer CookieContainer = new CookieContainer();

        private RestRequest CreateApiRequest(Method method, string SessionToken = "szFn6zAbjjU=")
        {
            var request = new RestRequest(method);
            request.AddHeader("sessionTokenKey", SessionToken);
            request.AddHeader("SessionToken", SessionToken);
            request.AddHeader("clientType", "cpdaily_student");
            request.AddHeader("deviceType", "1");
            request.AddHeader("CpdailyClientType", "CPDAILY");
            request.AddHeader("CpdailyStandAlone", "0");
            request.AddHeader("CpdailyInfo", DeviceInfo.EncryptCache);
            request.AddHeader("User-Agent", ApiUserAgent);
            return request;
        }

        /// <summary>
        /// 解析响应并返回有效的 ApiResponse
        /// </summary>
        /// <param name="response">响应</param>
        /// <returns>有效的 ApiResponse</returns>
        /// <exception cref="HttpRequestFailedException"></exception>
        /// <exception cref="DeserializeResponseException"></exception>
        /// <exception cref="CpdailyApiException"></exception>
        private static ApiResponse<T> ParseOrThrowException<T>(IRestResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestFailedException(response.StatusCode);
            }
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(response.Content);
            if (apiResponse is null)
            {
                throw new DeserializeResponseException(response.Content);
            }
            if (apiResponse.ErrorCode != 0 || apiResponse.Data is null)
            {
                throw new CpdailyApiException(apiResponse.ErrorCode, apiResponse.ErrorMessage);
            }

            return apiResponse;
        }

        /// <summary>
        /// 获取后续加密用的密钥
        /// </summary>
        /// <param name="guid">随机的GUID</param>
        /// <returns>SecretKey</returns>
        public async Task<SecretKey> GetSecretKeyAsync(Guid guid)
        {
            string p = $"{guid}|firstv";
            p = Convert.ToBase64String(CpdailyCrypto.RSAEncrpty(Encoding.ASCII.GetBytes(p), CpdailyCrypto.PublicKey));
            string s = $"p={p}&{SaltForGetSecretKey}";
            s = CpdailyCrypto.MD5(s).ToLower();
            string url = "https://mobile.campushoy.com/app/auth/dynamic/secret/getSecretKey/v-8213";
            RestClient client = new RestClient(url)
            {
                CookieContainer = CookieContainer
            };
            var request = CreateApiRequest(Method.POST);
            request.AddJsonBody(new { p, s });
            var response = await client.ExecutePostAsync(request);
            ApiResponse<string> apiResponse = ParseOrThrowException<string>(response);
            string decretedData = Encoding.ASCII.GetString(CpdailyCrypto.RSADecrpty(Convert.FromBase64String(apiResponse.Data), CpdailyCrypto.PrivateKey));
            string[] data = decretedData.Split("|");
            return new SecretKey { Guid = Guid.Parse(data[0]), Chk = data[1], Fhk = data[2] };
        }

        /// <summary>
        /// 获取后续加密用的密钥
        /// </summary>
        /// <returns>SecretKey</returns>
        public Task<SecretKey> GetSecretKeyAsync()
        {
            return GetSecretKeyAsync(Guid.NewGuid());
        }

        /// <summary>
        /// 获取所有入驻的学校
        /// </summary>
        /// <returns>School[]</returns>
        public async Task<List<School>> GetSchoolsAsync()
        {
            string url = $"https://static.campushoy.com/apicache/tenantListSort?v={DateTimeOffset.Now.ToUnixTimeMilliseconds()}&oick={CpdailyCrypto.GetOick()}";
            RestClient client = new RestClient(url)
            {
                CookieContainer = CookieContainer
            };
            var request = CreateApiRequest(Method.GET);
            var response = await client.ExecuteGetAsync(request);
            ApiResponse<object> apiResponse= ParseOrThrowException<object>(response);

            List<School> schools = new List<School>();
            var lists = JArray.FromObject(apiResponse.Data);
            foreach (JObject item in lists)
            {
                JToken? datas = item["datas"];
                if (datas is null) continue;
                var list = datas.ToObject<List<School>>();
                if (list is not null)
                {
                    schools.AddRange(list);
                }
            }
            return schools;
        }

        /// <summary>
        /// 获取学校的详细信息(用于登录或其他)
        /// </summary>
        /// <param name="schoolId">学校ID</param>
        /// <param name="chk">通过GetSecretKeyAsync()获取到的Chk密钥</param>
        /// <returns>SchoolDetails</returns>
        public async Task<SchoolDetails> GetSchoolDetailsAsync(string schoolId, string chk)
        {
            var encryptedId = CpdailyCrypto.DESEncrypt(schoolId, CpdailyCrypto.GetDESKey(chk), CpdailyCrypto.IV);
            string url = $"https://mobile.campushoy.com/v6/config/guest/tenant/info/v-8213";
            RestClient client = new RestClient(url)
            {
                CookieContainer = CookieContainer
            };
            var request = CreateApiRequest(Method.GET);
            request.AddParameter("a", encryptedId);
            request.AddParameter("b", "firstv");
            request.AddParameter("oick", CpdailyCrypto.GetOick());
            var response = await client.ExecuteGetAsync(request);
            ApiResponse<string> apiResponse = ParseOrThrowException<string>(response);
            string decretedData = CpdailyCrypto.DESDecrypt(apiResponse.Data, CpdailyCrypto.GetDESKey(chk), CpdailyCrypto.IV);
            JArray list = JArray.Parse(decretedData);
            if (list.Count != 0)
            {
                return list[0].ToObject<SchoolDetails>() ?? throw new UnknownException();
            }
            throw new UnknownException();
        }

        /// <summary>
        /// 获取学校的详细信息(用于登录或其他)
        /// </summary>
        /// <param name="school">School Object</param>
        /// <param name="secretKey">SecretKey Object</param>
        /// <returns>SchoolDetails</returns>
        public Task<SchoolDetails> GetSchoolDetailsAsync(School school, SecretKey secretKey)
        {
            if (school.Id is null)
            {
                throw new ArgumentNullException(nameof(school.Id));
            }
            if (secretKey.Chk is null)
            {
                throw new ArgumentNullException(nameof(secretKey.Chk));
            }
            return GetSchoolDetailsAsync(school.Id, secretKey.Chk);
        }

        /// <summary>
        /// 通用登录，今日校园的Auth阶段(未完成)
        /// </summary>
        /// <param name="encryptedToken">通过LoginWorker得到的Token</param>
        /// <param name="schoolId">School Id</param>
        /// <param name="chk">通过GetSecretKeyAsync()获取到的Chk密钥</param>
        /// <returns>CookieContainer(以后将会修改)</returns>
        public async Task<CookieContainer> CpdailyAuth(string encryptedToken, string schoolId, string chk)
        {
            string encryptedData = JsonConvert.SerializeObject(new { c = schoolId, d = encryptedToken });
            encryptedData = CpdailyCrypto.DESEncrypt(encryptedData, CpdailyCrypto.GetDESKey(chk), CpdailyCrypto.IV);
            var result = new CookieContainer();
            string url = "https://mobile.campushoy.com/app/auth/authentication/notcloud/login/v-8213";
            RestClient client = new RestClient(url)
            {
                CookieContainer = result
            };
            var request = CreateApiRequest(Method.POST);
            request.AddJsonBody(new { a = encryptedData, b = "firstv" });
            var response = await client.ExecutePostAsync(request);
            ApiResponse<string> apiResponse = ParseOrThrowException<string>(response);
            JObject json = JObject.Parse(CpdailyCrypto.DESDecrypt(apiResponse.Data, CpdailyCrypto.GetDESKey(chk), CpdailyCrypto.IV));
            string? tgc = json["tgc"]?.Value<string>();
            if (string.IsNullOrEmpty(tgc))   // TODO: 
            {
                string? mobile = json["mobile"]?.Value<string>();
                await SendVerifyMessage(result, mobile);
                string? code = Console.ReadLine();
                await VerifyCode(result, encryptedToken, mobile, code);
            }
            return result;
        }

        /// <summary>
        /// 通用登录，设备异常需验证，发送验证码(未完成)
        /// </summary>
        /// <param name="cookieContainer">通过CpdailyAuth()得到的CookieContainer</param>
        /// <param name="phoneNumber">手机号(需要修改:可在Auth得到)</param>
        /// <returns></returns>
        public async Task SendVerifyMessage(CookieContainer cookieContainer, string phoneNumber)
        {
            string mobile = CpdailyCrypto.DESEncrypt(phoneNumber, "QTZ&A@54", CpdailyCrypto.IV);
            string url = "https://mobile.campushoy.com/v6/auth/deviceChange/mobile/messageCode/v2";
            RestClient client = new RestClient(url)
            {
                CookieContainer = cookieContainer
            };
            var request = CreateApiRequest(Method.POST);
            request.AddJsonBody(new { mobile });
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            //var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
            //if (apiResponse.ErrorCode != 0)
            //{
            //    throw new Exception($"出现错误: {apiResponse.ErrorMessage}, 错误代码: {apiResponse.ErrorCode}。");
            //}

        }

        /// <summary>
        /// 通用登录，设备异常需验证，提交验证码(未完成)
        /// </summary>
        /// <param name="cookieContainer">通过CpdailyAuth()得到的CookieContainer</param>
        /// <param name="encryptedToken">通过LoginWorker得到的Token</param>
        /// <param name="phoneNumber">手机号</param>
        /// <param name="messageCode">验证码</param>
        /// <returns></returns>
        public async Task VerifyCode(CookieContainer cookieContainer, string encryptedToken, string phoneNumber, string messageCode)
        {
            string url = "https://mobile.campushoy.com/v6/auth/deviceChange/validateMessageCode";
            RestClient client = new RestClient(url)
            {
                CookieContainer = cookieContainer
            };
            var request = CreateApiRequest(Method.POST);
            request.AddJsonBody(new { ticket = encryptedToken, mobile = phoneNumber, messageCode });
            var response = await client.ExecutePostAsync(request);
            _ = ParseOrThrowException<object>(response);
        }

        /// <summary>
        /// 手机验证码登录(发送验证码)
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <param name="chk">通过GetSecretKeyAsync()获取到的Chk密钥</param>
        /// <returns>void</returns>
        public async Task MobileLoginAsync(string phoneNumber, string chk)
        {
            string mobile = CpdailyCrypto.DESEncrypt(phoneNumber, CpdailyCrypto.GetDESKey(chk), CpdailyCrypto.IV);
            string url = "https://mobile.campushoy.com/app/auth/authentication/mobile/messageCode/v-8213";
            RestClient client = new RestClient(url)
            {
                CookieContainer = CookieContainer
            };
            var request = CreateApiRequest(Method.POST);
            request.AddJsonBody(new { a = mobile, b = "firstv" });
            var response = await client.ExecutePostAsync(request);
            _ = ParseOrThrowException<object>(response);
        }

        /// <summary>
        /// 手机验证码登录(发送验证码)
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <param name="secretKey">通过GetSecretKeyAsync()获取到的SecretKey</param>
        /// <returns>void</returns>
        public Task MobileLoginAsync(string phoneNumber, SecretKey secretKey)
        {
            if (secretKey.Chk is null)
            {
                throw new ArgumentNullException(nameof(secretKey.Chk));
            }
            return MobileLoginAsync(phoneNumber, secretKey.Chk);
        }

        /// <summary>
        /// 手机验证码登录(提交验证码)
        /// </summary>
        /// <param name="phoneNumber">手机号码</param>
        /// <param name="code">验证码</param>
        /// <param name="chk">通过GetSecretKeyAsync()获取到的Chk密钥</param>
        /// <returns>LoginResult</returns>
        public async Task<LoginResult> MobileLoginAsync(string phoneNumber, string code, string chk)
        {
            string data = CpdailyCrypto.DESEncrypt(
                JsonConvert.SerializeObject(new { d = code, c = phoneNumber }),
                CpdailyCrypto.GetDESKey(chk),
                CpdailyCrypto.IV);
            string url = "https://mobile.campushoy.com/app/auth/authentication/mobileLogin/v-8213";
            RestClient client = new RestClient(url)
            {
                CookieContainer = CookieContainer
            };
            var request = CreateApiRequest(Method.POST);
            request.AddJsonBody(new { a = data, b = "firstv" });
            var response = await client.ExecutePostAsync(request);
            ApiResponse<string> apiResponse = ParseOrThrowException<string>(response);
            JObject json = JObject.Parse(CpdailyCrypto.DESDecrypt(apiResponse.Data, CpdailyCrypto.GetDESKey(chk), CpdailyCrypto.IV));
            return json.ToObject<LoginResult>() ?? throw new UnknownException();
        }

        /// <summary>
        /// 手机验证码登录(提交验证码)
        /// </summary>
        /// <param name="phoneNumber">手机号码</param>
        /// <param name="code">验证码</param>
        /// <param name="secretKey">通过GetSecretKeyAsync()获取到的SecretKey</param>
        /// <returns></returns>
        public Task<LoginResult> MobileLoginAsync(string phoneNumber, string code, SecretKey secretKey)
        {
            if (secretKey.Chk is null)
            {
                throw new ArgumentNullException(nameof(secretKey.Chk));
            }
            return MobileLoginAsync(phoneNumber, code, secretKey.Chk);
        }

        /// <summary>
        /// 获取用于访问校内应用的 Cookie
        /// </summary>
        /// <param name="loginResult">LoginResult Object</param>
        /// <returns>Cookie string</returns>
        public async Task<string> UserStoreAppListAsync(LoginResult loginResult)
        {
            string? sessionTokenRaw = loginResult.SessionToken;
            string? tgcRaw = loginResult.Tgc;
            if (sessionTokenRaw is null || tgcRaw is null)
            {
                throw new Exception("SessionToken 或 Tgc 不可为 null.");
            }
            if (loginResult.TenantId is null)
            {
                throw new Exception("TenantId 不可为 null.");
            }
            string sessionToken = CpdailyCrypto.DESEncrypt(sessionTokenRaw, "XCE927==", CpdailyCrypto.IV);
            string tgc = CpdailyCrypto.DESEncrypt(tgcRaw, "XCE927==", CpdailyCrypto.IV);

            var amp = new
            {
                AMP1 = new[]
                {
                    new {name="sessionToken", value= sessionTokenRaw},
                    new {name="sessionToken", value= sessionTokenRaw}
                },
                AMP2 = new[]
                {
                    new {name="CASTGC", value= tgcRaw},
                    new {name="AUTHTGC", value= tgcRaw},
                    new {name="sessionToken", value= sessionTokenRaw},
                    new {name="sessionToken", value= sessionTokenRaw}
                }
            };

            string ampCookies = CpdailyCrypto.DESEncrypt(JsonConvert.SerializeObject(amp), "XCE927==", CpdailyCrypto.IV);

            string? url = $"https://cqjtu.campusphere.net/wec-portal-mobile/client/userStoreAppList?oick={CpdailyCrypto.GetOick(loginResult.UserId)}";
            IRestResponse? response = null;
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("http://ids.cqjtu.edu.cn/"), new Cookie("CASTGC", tgcRaw));
            cookieContainer.Add(new Uri("http://ids.cqjtu.edu.cn/"), new Cookie("AUTHTGC", tgcRaw));
            // RestSharp 不会处理复杂的跳转，因此自行循环处理
            do
            {
                RestClient client = new RestClient(url)
                {
                    CookieContainer = cookieContainer,
                    FollowRedirects = false
                };
                var request = CreateApiRequest(Method.GET, sessionToken);
                request.AddHeader("tenantId", loginResult.TenantId);
                request.AddHeader("TGC", tgc);
                request.AddHeader("AmpCookies", ampCookies);
                response = await client.ExecuteGetAsync(request);

                url = response.Headers.Where(x => x.Name == "Location").Select(x => x.Value.ToString()).FirstOrDefault();
            } while (!string.IsNullOrEmpty(url));

            var result = string.Join(" ", cookieContainer.GetAllCookies().Select(x => $"{x.Name}={x.Value};"));
            return result;
        }

    }
}
