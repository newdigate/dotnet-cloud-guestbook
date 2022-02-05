using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // PORT environment variable is provided in guestbook-backend.deployment.yaml.
                    var port = Environment.GetEnvironmentVariable("PORT");
                    if (string.IsNullOrEmpty(port))
                    {
                        throw new ArgumentException("PORT environment variable is not set");
                    }

                    webBuilder
                        .UseUrls($"http://*:{port}")
                        .UseStartup<Startup>();
                });
    }
}
