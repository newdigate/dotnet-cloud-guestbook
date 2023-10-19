using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dotnet_guestbook.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using frontend;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace dotnet_guestbook.Controllers
{
    public class HomeController : Controller
    {
        private static readonly Meter MyMeter = new("MyCompany.MyProduct.MyLibrary", "1.0");
        private static readonly Counter<long> _metric_index_get = MyMeter.CreateCounter<long>($"{nameof(HomeController)} {nameof(Index)} GET");
        private static readonly Counter<long> _metric_index_get_error = MyMeter.CreateCounter<long>($"{nameof(HomeController)} {nameof(Index)} GET ERROR");
        private static readonly Counter<long> _metric_index_post = MyMeter.CreateCounter<long>($"{nameof(HomeController)} {nameof(Index)} POST");
        private static readonly Counter<long> _metric_index_post_error = MyMeter.CreateCounter<long>($"{nameof(HomeController)} {nameof(Index)} POST ERROR");

        private ILogger _logger;
        private IEnvironmentConfiguration _envConfig;
        private IHttpClientFactory _factory;

        private static ActivitySource MyActivitySource = new ActivitySource(Startup.defaultServiceName, Startup.serviceVersion);


        public HomeController(
            IHttpClientFactory httpFactory,
            ILoggerFactory loggerFactory,
            IEnvironmentConfiguration environmentConfiguration)
        {
            _factory = httpFactory;
            _logger = loggerFactory.CreateLogger<HomeController>();
            _envConfig = environmentConfiguration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation($"Getting all messages");

            using Activity? activity = MyActivitySource.StartActivity("IndexGet");
            activity?.SetTag("foo", 1);
            activity?.SetTag("bar", "Hello, World!");
            activity?.SetTag("baz", new int[] { 1, 2, 3 });

            _metric_index_get.Add(1);
            // Get the entries from the backend
            try
            {
                var httpClient = _factory.CreateClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // TODO - Good spot for adding a logpoint to get the backend address
                _logger.LogInformation($"Making a request to {_envConfig.BackendAddress}");
                var response = await httpClient.GetAsync(_envConfig.BackendAddress);
                response.EnsureSuccessStatusCode();
                var entries = await response.Content.ReadAsAsync<IEnumerable<GuestbookEntry>>();

                return View(entries);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                _metric_index_get_error.Add(1);
                return View();
            }
        }

        [HttpPost("post")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post([FromForm] GuestbookEntry entry)
        {
            _logger.LogInformation($"Calling backend at {_envConfig.BackendAddress} for message authored by {entry.Name}");
            _metric_index_post.Add(1);
            using Activity? activity = MyActivitySource.StartActivity("IndexPost");
            try
            {
                var httpClient = _factory.CreateClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync<GuestbookEntry>(_envConfig.BackendAddress, entry);
                string responseContent = await responseMessage.Content.ReadAsStringAsync();
                _logger.LogInformation($"RESPONSE: {_envConfig.BackendAddress}: {responseContent}");
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                _metric_index_post_error.Add(1);
                return View();
            }
        }
 
        [HttpGet]
        public async Task<IActionResult> Test() {
            using Activity? activity = MyActivitySource.StartActivity("SomeWork");
            
            await Task.Delay(1000);
            return new JsonResult("sfdd");
        }
    }
}
