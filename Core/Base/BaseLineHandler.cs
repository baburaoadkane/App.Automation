using App.Automation.Core.Interfaces;
using App.Automation.Core.Utilities;
using OpenQA.Selenium;

namespace App.Automation.Core.Base;

public abstract class BaseLineHandler<TLine> : BaseHandler, ILineHandler<TLine>
{
    protected BaseLineHandler(IWebDriver driver, WaitHelper wait, ReportHelper report)
        : base(driver, wait, report) { }


    // ── Common ────────────────────────────────────────────────────────────
    private static readonly By LookupText = By.XPath("//div[contains(@class,'lookup-text')]");
    private static readonly By DeleteLineButton = By.XPath("//div[@class='dx-button-content' and .//span[text()='Delete Line']]");
    private static readonly By AddLineButton = By.XPath("//div[contains(@id,'LineNewButton')]");
    private static readonly By NextButton = By.XPath("//a[contains(@class,'dxp-button')]//img[@alt='Next']");
    private static readonly By ExtraFieldButton = By.XPath("//img[contains(@id, '_DXCBtn-1Img')]");

    public abstract void Fill(IEnumerable<TLine> lines);

    protected class FieldConfig
    {
        public By? Dropdown { get; set; }
        public int? ColumnIndex { get; set; }
    }

    protected abstract Dictionary<string, FieldConfig> FieldMap { get; }

    protected By GetDropdown(string field)
    {
        if (!FieldMap.ContainsKey(field) || FieldMap[field].Dropdown == null)
            throw new Exception($"Dropdown not defined for {field}");

        return FieldMap[field].Dropdown!;
    }

    protected int GetColIndex(string field)
    {
        if (!FieldMap.ContainsKey(field) || FieldMap[field].ColumnIndex == null)
            throw new Exception($"Column index not defined for {field}");

        return FieldMap[field].ColumnIndex!.Value;
    }

    protected By GetCell(string field)
    {
        int colIndex = GetColIndex(field);
        return By.XPath($"(//div[@class='dxgBCTC dx-ellipsis'])[{colIndex}]");
    }

    // ── Set Cell Value ────────────────────────────────────────────────────
    protected void SetCell(string field, object? value)
    {
        if (value == null || !IsValidValue(value)) return;

        string finalValue = value switch
        {
            decimal d => d.ToString("G29"),
            double d => d.ToString("G29"),
            float f => f.ToString("G29"),
            int i => i.ToString("G29"),
            long l => l.ToString("G29"),
            _ => value.ToString() ?? string.Empty
        };

        var cell = GetCell(field);
        SetClipboardValue(cell, finalValue);
    }

    // ── Validation ────────────────────────────────────────────────────────
    protected bool IsValidValue(object value)
    {
        return value switch
        {
            string s => !string.IsNullOrWhiteSpace(s),
            decimal d => d > 0,
            double d => d > 0,
            float f => f > 0,
            int i => i > 0,
            long l => l > 0,
            _ => true
        };
    }

    // ── 🔥 Generic Lookup ────────────────────────────────────────────────
    protected void Lookup(string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var dropdown = GetDropdown(field);

        OpenDropdown(dropdown);
        WaitForLoader();

        SelectOption(LookupText, NextButton, value);
    }

    // ── 🔥 Lookup inside Grid Cell ────────────────────────────────────────
    protected void LookupCell(string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var cell = GetCell(field);
        Click(cell);

        var dropdown = GetDropdown(field);

        OpenDropdown(dropdown);
        WaitForLoader();

        SelectOption(LookupText, NextButton, value);
    }

    protected void DeleteExistingLine()
    {
        if (IsVisible(DeleteLineButton))
        {
            Click(DeleteLineButton);
            WaitForLoader();
        }
    }

    protected void AddNewLine()
    {
        Click(AddLineButton);
        WaitForLoader();
    }

    protected void ClickToShowExtraFields()
    {
        Click(ExtraFieldButton);
        WaitForLoader();
    }
}
