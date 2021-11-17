using Cpdaily;
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
                Console.WriteLine("使用方法：dotnet CqjtuLibrary.dll \"Cookie\"");
                return;
            }
            string cookie = args[0];
            Console.WriteLine($"Cookie: {cookie}");
            try
            {
                var library = new Cqjtu.CpdailyLibrary();
                // 图书馆 App 需要额外登录一次
                cookie = await library.LoginAsync(cookie);
                await library.ReserveAsync(cookie, "南岸馆", DateTime.Parse("2021-11-18"));
                Console.WriteLine("已经尝试预约!");
                var logs = await library.GetReservationsAsync(cookie);
                foreach (var item in logs)
                {
                    Console.WriteLine($"{item.Id}. {item.LibraryName} {item.Date:D}");
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