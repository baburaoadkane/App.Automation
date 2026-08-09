using App.Automation.Core.Base;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Sales.Invoice.DataModels;
using OpenQA.Selenium;

namespace App.Automation.Modules.Sales.Invoice.HeaderHandlers;

public class InvoiceHeaderHandler : BaseHeaderHandler<InvoiceHeaderDM>
{
    // ── Common Lookup Locator ─────────────────────────────────────────
    private static readonly By LookupText = By.XPath("//div[@class='lookup-text']");

    // ── Static Fields ──────────────────────────────────────────────────
    private static readonly By TxnDateDropdown = By.XPath("//td[contains(@id, '.TxnDate_B-1')]");
    private static readonly By TxnDateInput = By.XPath("//input[contains(@id, '.TxnDate_I')]");

    private static readonly By ReferenceNumInput = By.XPath("//input[contains(@id, '.ReferenceNum_I')]");
    private static readonly By CustomerPONumInput = By.XPath("//input[contains(@id, '.CustomerPONum_I')]");
    private static readonly By DisplayNameInput = By.XPath("//input[contains(@id, '.DisplayCustomerName_I')]");
    private static readonly By MobileNumInput = By.XPath("//input[contains(@id, '.MobileNum_I')]");

    public InvoiceHeaderHandler(IWebDriver driver, WaitHelper wait, ReportHelper report) : base(driver, wait, report) { }

    // ── PUBLIC ENTRY ──────────────────────────────────────────────────
    public override void Fill(InvoiceHeaderDM header)
    {
        FillInvoiceDate(header.InvoiceDate);

        // 🔥 Fully Dynamic (NO FieldMap)
        Lookup("CustomerId", header.Customer);
        Lookup("CurrencyId", header.Currency);
        Lookup("PriceListId", header.PriceList);
        Lookup("WarehouseId", header.Warehouse);
        Lookup("SalesmanId", header.Salesman);
        Lookup("PaymentMethodId", header.PaymentMethod);
        Lookup("PaymentTermId", header.PaymentTerm);

        Type(ReferenceNumInput, header.ReferenceNum);
        Type(CustomerPONumInput, header.CustomerPONum);
        Type(DisplayNameInput, header.DisplayName);
        Type(MobileNumInput, header.MobileNum);

        //WaitForLoader();
    }

    // ── 🔥 FULLY DYNAMIC LOOKUP ───────────────────────────────────────
    private void Lookup(string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var (dropdown, input, nextPage) = BuildLookupLocators(fieldName);

        OpenDropdown(dropdown);
        //Type(input, value);   

        WaitForLoader();

        SelectOption(LookupText, nextPage, value);
    }

    // ── 🔥 SMART LOCATOR BUILDER ──────────────────────────────────────
    private (By dropdown, By input, By nextPage) BuildLookupLocators(string fieldName)
    {
        // Find exact input for this field
        var inputElement = Wait.UntilVisible(
            By.XPath($"//input[contains(@id, '.{fieldName}Lookup_I')]")
        );

        string id = inputElement.GetAttribute("id");

        if (string.IsNullOrWhiteSpace(id) || !id.Contains('.'))
            throw new Exception($"Invalid ID format: {id}");

        // Example:
        // SalesInvoice.CustomerIdLookup_I

        string modulePrefix = id.Split('.')[0];

        string baseId = $"{modulePrefix}.{fieldName}";

        var dropdown = By.Id($"{baseId}Lookup_B-1");
        var input = By.Id($"{baseId}Lookup_I");
        var nextPage = By.Id($"{baseId}_NextPage");

        return (dropdown, input, nextPage);
    }

    // ── NON-LOOKUP ────────────────────────────────────────────────────
    private void FillInvoiceDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date)) return;

        OpenDropdown(TxnDateDropdown);
        ClearAndType(TxnDateInput, date);
    }
}
