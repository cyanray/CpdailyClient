using Cpdaily;
using Cpdaily.CpdailyModels;
using Cpdaily.SchoolServices.Cqjtu.NetPay;
using Newtonsoft.Json;
using System.Text;
using Cqjtu = Cpdaily.SchoolServices.Cqjtu;

namespace GetUserInfo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("使用方法：dotnet GetUserInfo.dll \"base64 of the login result json\"");
                return;
            }
            LoginResult? loginResult = null;
            try
            {
                string json = Encoding.UTF8.GetString(Convert.FromBase64String(args[0]));
                loginResult = JsonConvert.DeserializeObject<LoginResult>(json);
                if (loginResult is null)
                {
                    throw new Exception("无法反序列化 LoginResult");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            try
            {
                var cpdaily = new CpdailyClient();
                var secretKey = await cpdaily.GetSecretKeyAsync();
                var info = await cpdaily.GetUserInfoAsync(loginResult);
                if (info is not null)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(info));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}