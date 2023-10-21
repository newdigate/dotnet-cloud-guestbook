using System;
using System.Net.Http;
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
namespace backend
{
    public class Startup
    {
        public const string defaultServiceName = "MyCompany.MyProduct.Backend";
        public const string serviceVersion = "1.0.0";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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
                            c.Endpoint = new Uri("http://dotnet-guestbook-tempo:4317"); 
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
            
            services.AddLogging(
                loggingBuilder =>
                    loggingBuilder
                        .AddSerilog(
                            new LoggerConfiguration()
                                .Enrich.FromLogContext()
                                .WriteTo.GrafanaLoki("http://dotnet-guestbook-loki:3100", 
                                    labels:
                                        new LokiLabel[] {
                                            new () { Key = "app", Value="dotnet-guestbook-backend"},
                                        }, 
                                    propertiesAsLabels : new [] {"app"})
                                .CreateLogger(), 
                            dispose: true)
                        .AddConsole());

            // GUESTBOOK_DB_ADDR environment variable is provided in guestbook-backend.deployment.yaml.
            var databaseAddr = Environment.GetEnvironmentVariable("GUESTBOOK_DB_ADDR");
            if (string.IsNullOrEmpty(databaseAddr))
            {
                throw new ArgumentException("GUESTBOOK_DB_ADDR environment variable is not set");
            }

            services.AddControllers();

            // Pass the configuration for connecting to MongoDB to Dependency Injection container
            services.Configure<MongoConfig>(c => c.DatabaseAddress = $"mongodb://{databaseAddr}");

            services.AddScoped<IMessageRepository, MessageRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
            //app.UseHttpLogging();
        }
    }
}