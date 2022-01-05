using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace dotnet_guestbook
{
    public class Program
    {
        private static readonly Counter TickTock =
            Metrics.CreateCounter("sampleapp_ticks_total", "Just keeps on ticking");
        public static void Main(string[] args)
        {
            while (!Debugger.IsAttached) {
                Thread.Sleep(1000);
            }
            var server = new MetricServer(hostname: "127.0.0.1", port: 1234);
            server.RequestPredicate = request => 
                {
                    Console.WriteLine(request.Url.ToString());
                    return true;
                };
            server.Start();

            CancellationTokenSource cancelSource = new CancellationTokenSource();
            Task background = Task.Factory.StartNew ( () => {
                    while(!cancelSource.IsCancellationRequested) {
                        Console.WriteLine("Tick/Tock");
                        TickTock.Inc();
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }, cancelSource.Token);

            CreateHostBuilder(args).Build().Run();
            cancelSource.Cancel();
            background.Wait();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddDebug();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
