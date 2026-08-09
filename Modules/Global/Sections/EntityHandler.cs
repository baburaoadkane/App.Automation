using App.Automation.Core.Base;
using App.Automation.Core.Utilities;
using OpenQA.Selenium;

namespace App.Automation.Modules.Global.Sections
{
    public class EntityHandler : BaseHandler
    {
        public EntityHandler(IWebDriver driver, WaitHelper wait, ReportHelper report)
        : base(driver, wait, report) { }


        private void NavigateToEntity(string moduleName, string entityName)
        {
            By moduleButton = By.Id("AppModuleButton");

            By moduleLocator = By.XPath(
                $"//a[normalize-space()='{moduleName}'] | " +
                $"//li[normalize-space()='{moduleName}'] | " +
                $"//button[normalize-space()='{moduleName}']"
            );

            By entityLocator = By.XPath(
                $"//a[normalize-space()='{entityName}'] | " +
                $"//li[normalize-space()='{entityName}']"
            );

            try
            {
                // Open module menu
                Wait.UntilClickable(moduleButton, 5).Click();
                WaitForLoader();

                // Click module (e.g., Sales)
                Wait.UntilClickable(moduleLocator, 5).Click();
                WaitForLoader();

                // Click entity (e.g., Invoice)
                Wait.UntilClickable(entityLocator, 5).Click();
                WaitForLoader();
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new Exception(
                    $"Navigation failed for Module: {moduleName}, Entity: {entityName}", ex);
            }
        }
    }
}
