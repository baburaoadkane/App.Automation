using App.Automation.Core.Enums;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace App.Automation.Core.Utilities
{
    public sealed class ConfigReader
    {
        // ── Singleton ──────────────────────────────────────────────────────────
        private static readonly Lazy<ConfigReader> _instance =
            new(() => new ConfigReader());

        public static ConfigReader Instance => _instance.Value;

        // ── Raw config dictionary ──────────────────────────────────────────────
        private readonly JsonDocument _config;

        // ── Constructor ────────────────────────────────────────────────────────
        private ConfigReader()
        {
            string basePath = Path.Combine(AppContext.BaseDirectory, "Config", "appsettings.json");
            string environment = Environment.GetEnvironmentVariable("ERP_ENV") ?? "Test";
            string envPath = Path.Combine(AppContext.BaseDirectory, "Config", $"appsettings.{environment}.json");

            // Load base config
            string baseJson = File.ReadAllText(basePath);
            _config = JsonDocument.Parse(baseJson);

            // Merge environment-specific overrides if file exists
            if (File.Exists(envPath))
            {
                string envJson = File.ReadAllText(envPath);
                _config = MergeJson(baseJson, envJson);
            }
        }

        // ── Public Properties ──────────────────────────────────────────────────

        /// <summary>Active Base URL based on the current environment.</summary>
        public string BaseUrl
        {
            get
            {
                string env = Get("Application:Environment");
                return Get($"Application_URLs:{env}");
            }
        }

        /// <summary>Browser type to use for WebDriver creation.</summary>
        public BrowserType BrowserType =>
            Enum.Parse<BrowserType>(Get("Browser:Type"), ignoreCase: true);

        /// <summary>Run browser in headless mode (no UI window).</summary>
        public bool Headless => GetBool("Browser:Headless");

        /// <summary>Browser window size string e.g. "1920,1080".</summary>
        public string WindowSize => Get("Browser:WindowSize");

        /// <summary>Implicit wait timeout in seconds.</summary>
        public int ImplicitWait => GetInt("Browser:ImplicitWait");

        /// <summary>Explicit wait timeout in seconds.</summary>
        public int ExplicitWait => GetInt("Browser:ExplicitWait");

        /// <summary>Page load timeout in seconds.</summary>
        public int PageLoadTimeout => GetInt("Browser:PageLoadTimeout");

        /// <summary>Admin username from config or environment variable.</summary>
        public string AdminUsername => Get("Credentials:AdminUser:Username");

        /// <summary>Admin password — resolves ${ENV_VAR} tokens from environment.</summary>
        public string AdminPassword => ResolveEnvToken(Get("Credentials:AdminUser:Password"));

        /// <summary>Approver username from config.</summary>
        public string ApproverUsername =>
            Get("Credentials:ApproverUser:Username");

        /// <summary>
        /// Approver password — resolves ${ENV_VAR} tokens from environment.
        /// </summary>
        public string ApproverPassword =>
            ResolveEnvToken(Get("Credentials:ApproverUser:Password"));

        /// <summary>Root path for all test data JSON files.</summary>
        public string TestDataRoot => Get("Paths:TestDataRoot");

        /// <summary>Output directory for HTML test reports.</summary>
        public string ReportsOutput => Get("Paths:ReportsOutput");

        /// <summary>Output directory for failure screenshots.</summary>
        public string ScreenshotsOutput => Get("Paths:ScreenshotsOutput");

        /// <summary>Max retry attempts for flaky interactions.</summary>
        public int MaxRetryAttempts => GetInt("Retry:MaxAttempts");

        /// <summary>Delay in ms between retry attempts.</summary>
        public int RetryDelayMs => GetInt("Retry:DelayBetweenAttemptsMs");

        /// <summary>Output folder for log files.</summary>
        public string LogsOutput => Get("Paths:LogsOutput");

        /// <summary>Whether to attach screenshots on test failure.</summary>
        public bool AttachScreenshotOnFailure => GetBool("Reporting:AttachScreenshotOnFailure");

        /// <summary>Whether to capture and attach screenshots on test pass.</summary>
        public bool AttachScreenshotOnPass => GetBool("Reporting:AttachScreenshotOnPass");

        // ── Module helpers ─────────────────────────────────────────────────────

        /// <summary>Check if a specific ERP module is enabled in config.</summary>
        public bool IsModuleEnabled(string moduleName) =>
            GetBool($"Modules:{moduleName}:Enabled");

        /// <summary>Get the base URL route for a module (e.g. "sales").</summary>
        public string GetModuleRoute(string moduleName) =>
            Get($"Modules:{moduleName}:BaseRoute");

        // ── Core get helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Get any config value by colon-delimited key path.
        /// Example: Get("Browser:Type") → "Chrome"
        /// </summary>
        public string Get(string keyPath)
        {
            string[] parts = keyPath.Split(':');
            JsonElement current = _config.RootElement;

            foreach (string part in parts)
            {
                if (!current.TryGetProperty(part, out current))
                    throw new KeyNotFoundException(
                        $"[ConfigReader] Key not found: '{keyPath}'. " +
                        $"Failed at segment '{part}'. Check appsettings.json.");
            }

            // Return a string representation for primitives (string, number, bool)
            return current.ValueKind switch
            {
                JsonValueKind.String => current.GetString()!,
                JsonValueKind.Number => current.GetRawText(),
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                JsonValueKind.Null => throw new InvalidOperationException($"[ConfigReader] Value for '{keyPath}' is null."),
                _ => throw new InvalidOperationException($"[ConfigReader] Unsupported JSON value kind for '{keyPath}': {current.ValueKind}.")
            };
        }

        private int GetInt(string key) => int.Parse(Get(key));
        private bool GetBool(string key) => bool.Parse(Get(key));

        // ── Private helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Resolves ${ENV_VAR_NAME} tokens from actual OS environment variables.
        /// If token is not found, returns the raw string (for local dev use).
        /// </summary>
        private static string ResolveEnvToken(string value)
        {
            if (value.StartsWith("${") && value.EndsWith("}"))
            {
                string varName = value[2..^1];
                return Environment.GetEnvironmentVariable(varName) ?? value;
            }
            return value;
        }

        /// <summary>
        /// Simple JSON merge: environment config values override base config.
        /// Performs a deep merge so nested objects are merged rather than replaced.
        /// </summary>
        private static JsonDocument MergeJson(string baseJson, string overrideJson)
        {
            // Use JsonNode (System.Text.Json.Nodes) to perform a recursive/deep merge
            var baseNode = JsonNode.Parse(baseJson)?.AsObject() ?? new JsonObject();
            var overrideNode = JsonNode.Parse(overrideJson)?.AsObject() ?? new JsonObject();

            JsonObject DeepMerge(JsonObject baseObj, JsonObject overrideObj)
            {
                var result = new JsonObject();

                // Start with base properties
                foreach (var kv in baseObj)
                    result[kv.Key] = kv.Value?.DeepClone();

                // Apply overrides (merge objects recursively)
                foreach (var kv in overrideObj)
                {
                    if (kv.Value is JsonObject ovObj && result[kv.Key] is JsonObject baseSubObj)
                    {
                        result[kv.Key] = DeepMerge(baseSubObj, ovObj);
                    }
                    else
                    {
                        result[kv.Key] = kv.Value?.DeepClone();
                    }
                }

                return result;
            }

            var merged = DeepMerge(baseNode, overrideNode);
            string mergedJson = merged.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
            return JsonDocument.Parse(mergedJson);
        }

    }
}
