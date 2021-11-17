using Cpdaily;
using Newtonsoft.Json;

namespace MobileLogin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("使用方法：dotnet Login.dll \"手机号码\" \"学校名称\"");
                return;
            }
            string phoneNumber = args[0];
            string schoolName = args[1];
            Console.WriteLine($"手机号码: {phoneNumber}");
            Console.WriteLine($"学校名称: {schoolName}");
            try
            {
                Console.WriteLine("尝试进行手机登录...");
                // 创建 CpdailyClient 对象
                var cpdaily = new CpdailyClient();
                // 获取加密参数
                var secretKey = await cpdaily.GetSecretKeyAsync();
                // 发送短信验证码
                Console.WriteLine("发送短信验证码...");
                await cpdaily.MobileLoginAsync(phoneNumber, secretKey);
                // 输入短信验证码
                string? code = null;
                do
                {
                    Console.Write("请输入验证码:");
                    code = Console.ReadLine();
                } while (string.IsNullOrEmpty(code));
                // 短信验证码登录
                var loginResult = await cpdaily.MobileLoginAsync(phoneNumber, code, secretKey);
                Console.WriteLine("登录成功!");
                Console.WriteLine("请保留 LoginResult，可以用于恢复登录状态:");
                Console.WriteLine(JsonConvert.SerializeObject(loginResult));
                // 获取学校信息
                var schools = await cpdaily.GetSchoolsAsync();
                var school = schools.Where(x => x.Name == schoolName).FirstOrDefault();
                if (school is null)
                {
                    Console.WriteLine($"没有找到学校:{schoolName}!");
                    return;
                }
                // 获取学校详情
                var schoolDetails = await cpdaily.GetSchoolDetailsAsync(school, secretKey);
                // 获取访问校内应用的 Cookie
                var cookie = await cpdaily.UserStoreAppListAsync(loginResult, schoolDetails);
                Console.WriteLine(cookie);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}