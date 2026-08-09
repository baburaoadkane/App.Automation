using App.Automation.Core.Interfaces;
using App.Automation.Core.Utilities;
using OpenQA.Selenium;

namespace App.Automation.Core.Base
{
    public abstract class BaseExecutor<TDocument> : IExecutor<TDocument>
    {
        // ── Core dependencies ──────────────────────────────────────────────────
        protected readonly IWebDriver Driver;
        protected readonly WaitHelper Wait;
        protected readonly ConfigReader Config = ConfigReader.Instance;
        protected readonly ReportHelper Report;

        // ── Constructor ────────────────────────────────────────────────────────
        protected BaseExecutor(IWebDriver driver, WaitHelper wait, ReportHelper report)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            Wait = wait ?? throw new ArgumentNullException(nameof(wait));
            Report = report ?? throw new ArgumentNullException(nameof(report));
        }

        // ── Abstract contract — every executor MUST implement this ─────────────

        /// <summary>
        /// Execute the full document creation / edit / action flow.
        /// Called directly by TestCase methods.
        /// </summary>
        public abstract void Execute(TDocument document);

        // ── Shared navigation methods ──────────────────────────────────────────

        /// <summary>
        /// Navigate to a full URL path from the base URL.
        /// Example: Navigate("sales/invoices/new")
        /// </summary>
        protected void Navigate(string route)
        {
            string url = $"{Config.BaseUrl.TrimEnd('/')}/{route.TrimStart('/')}";
            Driver.Navigate().GoToUrl(url);
            WaitForPageLoad();
            Report.Info($"Navigated to: {url}");
        }

        protected void NavigateToModule(string moduleName)
        {
            By appModule = By.Id("AppModuleButton");
            By moduleText = By.XPath($"//a[contains(@class,'AppModuleButtonPopoverItem')][.//span[normalize-space()='{moduleName}']]");

            Wait.UntilClickable(appModule, 5).Click();
            Wait.WaitForSeconds(1);
            WaitForLoader();

            Wait.UntilClickable(moduleText, 5).Click();
            Wait.WaitForSeconds(1);
            WaitForLoader();
        }

        protected void NavigateToListing(string entityName)
        {
            By entityTitle = By.XPath($"//a[@title='{entityName}']");
            Wait.UntilClickable(entityTitle, 5).Click();
            WaitForLoader();
        }

        protected void OpenFormMode(string formMode)
        {
            By formState = By.XPath($"//li[@title='{formMode}']");

            Wait.UntilClickable(formState, 5).Click();
            WaitForLoader();
            Wait.WaitForSeconds(2);
        }

        /// <summary>
        /// Click a menu item by its visible text to navigate within ERP.
        /// Example: NavigateViaMenu("Sales", "Invoices", "New Invoice")
        /// </summary>
        protected void NavigateViaMenu(params string[] menuPath)
        {
            foreach (string menuItem in menuPath)
            {
                By locator = By.XPath($"//a[normalize-space()='{menuItem}'] | //span[normalize-space()='{menuItem}']");
                IWebElement element = Wait.UntilClickable(locator);
                element.Click();
                Thread.Sleep(300); // Brief pause for menu animation
            }
            WaitForPageLoad();
            Report.Info($"Navigated via menu: {string.Join(" → ", menuPath)}");
        }

        // ── Shared document actions ────────────────────────────────────────────

        /// <summary>
        /// Click the Save button and wait for the success confirmation.
        /// Override if your ERP uses a different save button locator.
        /// </summary>
        protected virtual void ClickOnForm(string? buttonText = "Save")
        {
            Report.Info($"{buttonText} Details...");

            By button = By.XPath($"//span[contains(@class, 'dx-vam') and text()='{buttonText}']");

            Wait.UntilClickable(button).Click();
            WaitForLoader();
            WaitForSuccessToast();

            Report.Info($"{buttonText} Successful.");
        }

        /// <summary>
        /// Submit the document for approval workflow.
        /// Override with your ERP's specific submit/approve button.
        /// </summary>
        protected virtual void Submit()
        {
            Report.Info("Submitting document for approval.");

            By submitButton = By.XPath(
                "//button[normalize-space()='Submit'] | " +
                "//button[normalize-space()='Send for Approval']"
            );

            Wait.UntilClickable(submitButton).Click();
            WaitForLoader();
            WaitForSuccessToast();

            Report.Info("Document submitted for approval.");
        }

        /// <summary>
        /// Approve the document (called from an approver context).
        /// Override with your ERP's specific approve button.
        /// </summary>
        protected virtual void Approve()
        {
            Report.Info("Approving document...");

            By approveButton = By.XPath(
                "//button[normalize-space()='Approve'] | " +
                "//button[normalize-space()='Approve Document']"
            );

            Wait.UntilClickable(approveButton).Click();
            WaitForLoader();
            WaitForSuccessToast();

            Report.Info("Document approved successfully.");
        }

        /// <summary>
        /// Cancel the document.
        /// Override with your ERP's specific cancel button and confirmation dialog.
        /// </summary>
        protected virtual void Cancel(string confirmOption)
        {
            Report.Info("Cancelling document...");

            By cancelButton = By.XPath("//button[normalize-space()='Cancel']");
            Wait.UntilClickable(cancelButton).Click();

            // Handle confirmation dialog if it appears
            ConfirmDialog(confirmOption);
            WaitForLoader();

            Report.Info("Document cancelled.");
        }

        /// <summary>
        /// Delete the document.
        /// Override with your ERP's specific delete button and confirmation dialog.
        /// </summary>
        protected virtual void Delete(string confirmOption)
        {
            Report.Info("Deleting document...");

            By deleteButton = By.XPath("//button[normalize-space()='Delete']");
            Wait.UntilClickable(deleteButton).Click();

            ConfirmDialog(confirmOption);
            WaitForLoader();

            Report.Info("Document deleted.");
        }

        // ── Shared UI helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Handle a Yes/Confirm/OK dialog button.
        /// Override for ERP-specific confirmation dialogs.
        /// </summary>
        protected virtual void ConfirmDialog(string confirmOption)
        {
            By confirmBtn = By.XPath($"//div[@aria-label='{confirmOption}']");

            try
            {
                Wait.UntilClickable(confirmBtn, timeoutSeconds: 5).Click();
                Report.Info("Confirmation dialog accepted.");
            }
            catch
            {
                // Dialog may not appear — that's fine
            }
        }

        /// <summary>
        /// Wait for a success toast/notification to appear and disappear.
        /// Override the locator for your ERP's specific toast element.
        /// </summary>
        protected virtual void WaitForSuccessToast()
        {
            By toast = By.CssSelector(
                ".dx-toast-success, .dx-toast-message"
            );

            try
            {
                Wait.UntilVisible(toast, timeoutSeconds: 2);
                Report.Info("Success notification received.");
            }
            catch
            {
                // Toast may be too fast to catch — not a failure
            }
        }

        /// <summary>
        /// Wait for any loading overlay/spinner to disappear.
        /// Override the locator for your ERP's specific loader element.
        /// </summary>
        protected virtual void WaitForLoader()
        {
            By loader = By.Id("LoadingPanel");
            try { Wait.UntilInvisible(loader, timeoutSeconds: 1); }
            catch { /* Loader may not appear — continue */ }
        }

        /// <summary>
        /// Wait for the page to fully load (document.readyState = 'complete').
        /// </summary>
        protected void WaitForPageLoad()
        {
            Wait.UntilPageLoaded();
        }

        /// <summary>
        /// Get the current document number from the page after saving.
        /// Override with your ERP's specific document number field locator.
        /// </summary>
        protected virtual string GetDocumentNumber()
        {
            By docNumLocator = By.CssSelector(
                "[data-field='docno'], .document-number, #documentNumber"
            );

            try
            {
                return Wait.UntilVisible(docNumLocator).Text.Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        public void SwitchToOldInterface()
        {
            try
            {
                var switchLocator = Driver.FindElement(By.XPath("//div[contains(@id,'MainMenu_DXI25_T')]//span[contains(@class,'dx-vam')]"));

                var switchText = switchLocator.Text.Trim();

                bool canSwitchToOld = switchText.Contains("old interface", StringComparison.OrdinalIgnoreCase);

                if (canSwitchToOld)
                {
                    Report.Info("Switching to OLD interface...");

                    var switchButton = By.XPath("//span[contains(@class, 'dx-vam') and text()='Switch to old interface']");

                    Wait.UntilClickable(switchButton).Click();
                    WaitForLoader();

                    Report.Info("Switched to OLD interface.");
                }
                else
                {
                    Report.Info("Already in OLD interface.");
                }
            }
            catch (Exception ex)
            {
                Report.Fail($"Failed to switch to OLD interface: {ex.Message}");
                throw;
            }
        }

        public void SwitchToNewInterface()
        {
            try
            {
                var switchLocator = Driver.FindElement(By.XPath("//div[contains(@id,'MainMenu_DXI25_T')]//span[contains(@class,'dx-vam')]"));

                var switchText = switchLocator.Text.Trim();

                bool canSwitchToOld = switchText.Contains("old interface", StringComparison.OrdinalIgnoreCase);

                if (!canSwitchToOld)
                {
                    Report.Info("Switching to NEW interface...");

                    var switchButton = By.XPath("//span[contains(@class, 'dx-vam') and text()='Switch to new interface']");

                    Wait.UntilClickable(switchButton).Click();
                    WaitForLoader();

                    Report.Info("Switched to NEW interface.");
                }
                else
                {
                    Report.Info("Already in NEW interface.");
                }
            }
            catch (Exception ex)
            {
                Report.Fail($"Failed to switch to NEW interface: {ex.Message}");
                throw;
            }
        }
    }
}
