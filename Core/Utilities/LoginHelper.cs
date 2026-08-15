using OpenQA.Selenium;

namespace App.Automation.Core.Utilities
{
    public class LoginHelper
    {
        private readonly IWebDriver _driver;
        private readonly WaitHelper _wait;

        // ── Update these locators to match your ERP login page ────────────────
        private static readonly By UsernameField = By.Name("Username");
        private static readonly By PasswordField = By.Name("Password");
        private static readonly By LoginButton = By.ClassName("login-btn");
        public LoginHelper(IWebDriver driver, WaitHelper wait)
        {
            _driver = driver;
            _wait = wait;
        }

        /// <summary>
        /// Login to the ERP application with the provided credentials.
        /// Waits for the dashboard to confirm successful login.
        /// </summary>
        public void Login(string username, string password)
        {
            // Wait for login page to load
            _wait.UntilVisible(UsernameField);

            // Enter credentials
            _driver.FindElement(UsernameField).Clear();
            _driver.FindElement(UsernameField).SendKeys(username);

            _driver.FindElement(PasswordField).Clear();
            _driver.FindElement(PasswordField).SendKeys(password);

            // Click login
            _wait.UntilClickable(LoginButton).Click();
        }

        /// <summary>Logout from the ERP application.</summary>
        public void Logout()
        {
            By ProfileIcon = By.Id("UserProfileMenu");
            By logoutBtn = By.XPath("//span[normalize-space()='Log Off']");

            try
            {
                _wait.UntilClickable(ProfileIcon, timeoutSeconds: 5).Click();
                _wait.UntilClickable(logoutBtn, timeoutSeconds: 5).Click();
            }
            catch
            {
                // If logout fails, just quit the driver
            }
        }
    }
}
