using System;
using frontend;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace dotnet_guestbook
{
    public class Startup
    {

        // Define some important constants and the activity source
        public const string defaultServiceName = "MyCompany.MyProduct.MyService";
        public const string serviceVersion = "1.0.0";
        private EnvironmentConfiguration envConfig;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            envConfig = new EnvironmentConfiguration();
            /*
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.GrafanaLoki("http://dotnet-guestbook-loki:3100")
                .WriteTo.Console()
                .CreateLogger(); */
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services
                .AddHttpClient(Options.DefaultName);
            services
                .AddOpenTelemetry()
                .ConfigureResource(b =>
                {
                    b.AddService(defaultServiceName);
                })
                .WithTracing(b => b
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        o.Filter = ctx => ctx.Request.Path != "/metrics";
                    })
                    .AddHttpClientInstrumentation()
                    .AddSource("TraceActivityName")
                    .AddOtlpExporter( 
                        c => { 
                            c.Endpoint = new System.Uri("http://dotnet-guestbook-tempo:4317"); 
                            c.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                        })
                    .AddConsoleExporter())
                .WithMetrics(b => b
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        o.Filter = (_, ctx) => ctx.Request.Path != "/metrics";
                    })
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddPrometheusExporter());

            services.AddSingleton<IEnvironmentConfiguration>(envConfig);
            services.AddLogging(
                loggingBuilder =>
                    loggingBuilder
                        .AddSerilog(
                            new LoggerConfiguration()
                                .Enrich.FromLogContext()
                                .WriteTo.GrafanaLoki("http://dotnet-guestbook-loki:3100", 
                                    labels:
                                        new LokiLabel[] {
                                            new LokiLabel() { Key = "app", Value="dotnet-guestbook-frontend"},
                                        }, 
                                    propertiesAsLabels : new [] {"app"})
                                .CreateLogger(), 
                            dispose: true)
                        .AddConsole());
            services.AddControllersWithViews();
        }
    
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,  IWebHostEnvironment env, ILogger<Startup> logger)
        {
            // GUESTBOOK_API_ADDR environment variable is provided in guestbook-frontend.deployment.yaml.
            var backendAddr = Environment.GetEnvironmentVariable("GUESTBOOK_API_ADDR");
            logger.LogInformation($"Backend address is set to {backendAddr}");
            if (string.IsNullOrEmpty(backendAddr))
            {
                throw new ArgumentException("GUESTBOOK_API_ADDR environment variable is not set");
            }

            // PORT environment variable is provided in guestbook-frontend.deployment.yaml.
            var port = Environment.GetEnvironmentVariable("PORT");
            logger.LogInformation($"Port env var is set to {port}");
            if (string.IsNullOrEmpty(port))
            {
                throw new ArgumentException("PORT environment variable is not set");
            }

            // Set the address of the backend microservice
            envConfig.BackendAddress = $"http://{backendAddr}:{port}/messages";

            var jaegerUrl = Environment.GetEnvironmentVariable("JAEGER_URI");
            logger.LogInformation($"JAEGER_URI env var is set to {jaegerUrl}");
            if (string.IsNullOrEmpty(jaegerUrl))
            {
                throw new ArgumentException("JAEGER_URI environment variable is not set");
            }

            Uri? jaegerUri = null;
            if (Uri.TryCreate(jaegerUrl, UriKind.Absolute, out jaegerUri)) {
                envConfig.OtlpTraceSyncUri = jaegerUri;
            } else {
                throw new ArgumentException("not able to parse JAEGER_URI {jaegerUrl}");
            }
            
            var serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME");
            logger.LogInformation($"SERVICE_NAME env var is set to {serviceName}");
            if (string.IsNullOrEmpty(serviceName))
            {
                serviceName = defaultServiceName;
            }
            envConfig.ServiceName = serviceName;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                // endpoints.MapControllerRoute(
                //     name: "postRoute",
                //     pattern: "{controller=Home}/{action=Post}/");

                endpoints.MapControllerRoute(
                    name: "postRoute",
                    pattern: "{controller}/post",
                    defaults: new { controller = "Home", action = "Post" });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
            app.UseHttpLogging();

        }
    }
}
