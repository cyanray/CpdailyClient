using Cpdaily;
using Cpdaily.SchoolServices.Cqjtu.NetPay;
using Newtonsoft.Json;
using Cqjtu = Cpdaily.SchoolServices.Cqjtu;

namespace MobileLogin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("使用方法：dotnet PayInfo.dll \"Cookie\"");
                return;
            }
            string cookie = args[0];
            Console.WriteLine($"Cookie: {cookie}");
            try
            {
                var payClient = new NetPay();
                var pay_cookie = await payClient.LoginAsync(cookie);
                var info = await payClient.GetAccountInfoAsync(pay_cookie);
                Console.WriteLine($"{info.SchoolId}: ￥{info.RemainingAmount}(待充值: ￥{info.UnaccountedAmount})");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}