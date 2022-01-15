using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dotnet_guestbook.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using frontend;
using System.Net.Http.Headers;
using Prometheus;

namespace dotnet_guestbook.Controllers
{
    public class HomeController : Controller
    {
        private ILogger _logger;
        private IEnvironmentConfiguration _envConfig;
        private IHttpClientFactory _factory;
        private static readonly Counter _metric_index_get = Metrics.CreateCounter("sampleapp_home_controller_index_get", "index_get");
        private static readonly Counter _metric_index_get_error = Metrics.CreateCounter("sampleapp_home_controller_index_get_error", "index_get_error");
        private static readonly Counter _metric_index_post = Metrics.CreateCounter("sampleapp_home_controller_index_post", "index_post");
        private static readonly Counter _metric_index_post_error = Metrics.CreateCounter("sampleapp_home_controller_index_post_error", "index_post_error");

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
            _metric_index_get.Inc();
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
                _metric_index_get_error.Inc();
                return View();
            }
        }

        [HttpPost("post")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post([FromForm] GuestbookEntry entry)
        {
            _logger.LogInformation($"Calling backend at {_envConfig.BackendAddress} for message authored by {entry.Name}");
            _metric_index_post.Inc();
            try
            {
                var httpClient = _factory.CreateClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                await httpClient.PostAsJsonAsync<GuestbookEntry>(_envConfig.BackendAddress, entry);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                _metric_index_post_error.Inc();
                return View();
            }
        }
    }
}
