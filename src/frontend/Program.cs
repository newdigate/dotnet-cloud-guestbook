using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace dotnet_guestbook
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //while (!Debugger.IsAttached) {
            //    Thread.Sleep(1000);
            //}
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
