using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V150.Network;
using System.Text.Json;

namespace App.Automation.Core.Utilities
{
    public class NetworkHelper
    {
        private readonly IWebDriver _driver;
        private DevToolsSession _devTools;
        private string _responseBody = string.Empty;
        private string _urlFilter = string.Empty;

        public NetworkHelper(IWebDriver driver)
        {
            _driver = driver;
            _devTools = ((IDevTools)_driver).GetDevToolsSession();
        }

        /// <summary>
        /// Start capturing network calls that match the given URL
        /// </summary>
        public void StartCapture(string urlContains)
        {
            _urlFilter = urlContains;
            _responseBody = string.Empty;

            var domains = _devTools.GetVersionSpecificDomains<OpenQA.Selenium.DevTools.V150.DevToolsSessionDomains>();
            var network = domains.Network;

            network.Enable(new EnableCommandSettings());

            network.ResponseReceived += async (sender, e) =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(e.Response.Url) &&
                        e.Response.Url.Contains(_urlFilter))
                    {
                        var body = await network.GetResponseBody(new GetResponseBodyCommandSettings
                        {
                            RequestId = e.RequestId
                        });

                        _responseBody = body.Body;
                    }
                }
                catch
                {
                    // Ignore failures (sometimes body not available)
                }
            };
        }

        /// <summary>
        /// Wait and return API response (important to avoid 0 values)
        /// </summary>
        public T GetResponse<T>()
        {
            if (string.IsNullOrEmpty(_responseBody))
                throw new Exception("No network response captured.");

            return JsonSerializer.Deserialize<T>(_responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        /// <summary>
        /// Clear previous captured response
        /// </summary>
        public void Clear()
        {
            _responseBody = string.Empty;
        }
    }
}
