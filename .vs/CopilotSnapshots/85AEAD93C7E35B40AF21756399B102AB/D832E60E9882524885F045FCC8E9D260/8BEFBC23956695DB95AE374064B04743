using OpenQA.Selenium;

namespace App.Automation.Core.Utilities
{
    public static class ScreenshotHelper
    {
        /// <summary>
        /// Capture a screenshot and save it as a PNG file.
        /// File name format: {TestName}_{Timestamp}.png
        /// Returns the full file path of the saved screenshot.
        /// </summary>
        public static string Capture(IWebDriver driver, string testName, string outputPath)
        {
            try
            {
                // Ensure output directory exists
                Directory.CreateDirectory(outputPath);

                // Clean test name for valid filename
                string safeName = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{safeName}_{timestamp}.png";
                string fullPath = Path.Combine(outputPath, fileName);

                // Take and save screenshot
                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile(fullPath);

                return fullPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ScreenshotHelper] Failed to capture screenshot: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
