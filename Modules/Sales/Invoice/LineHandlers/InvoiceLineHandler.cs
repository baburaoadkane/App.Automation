using App.Automation.Core.Base;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Sales.Invoice.DataModels;
using OpenQA.Selenium;

namespace App.Automation.Modules.Sales.Invoice.LineHandlers;

public class InvoiceLineHandler : BaseLineHandler<InvoiceLineDM>
{
    public InvoiceLineHandler(IWebDriver driver, WaitHelper wait, ReportHelper report)
        : base(driver, wait, report) { }


    protected override Dictionary<string, FieldConfig> FieldMap => new()
    {
        ["Barcode"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_ItemBarcodeId_B-1')]"),
            ColumnIndex = 1
        },
        ["Item"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_ItemId_B-1')]"),
            ColumnIndex = 2
        },
        ["Description"] = new()
        {
            ColumnIndex = 3
        },
        ["Size"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_ItemSizeId_B-1')]"),
            ColumnIndex = 4
        },
        ["Color"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_ItemColorId_B-1')]"),
            ColumnIndex = 5
        },
        ["Warehouse"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_WarehouseId_B-1')]"),
            ColumnIndex = 6
        },
        ["Quantity"] = new()
        {
            ColumnIndex = 8
        },
        ["UnitPrice"] = new()
        {
            ColumnIndex = 9
        },
        ["GrossAmount"] = new()
        {
            ColumnIndex = 12
        },
        ["BonusQty"] = new()
        {
            ColumnIndex = 13
        },
        ["UOM"] = new()
        {
            Dropdown = By.XPath("//td[contains(@id, '_UnitOfMeasureId_B-1')]"),
            ColumnIndex = 29
        },
        ["DiscountPercent"] = new()
        {
            ColumnIndex = 30
        },
        ["DiscountValue"] = new()
        {
            ColumnIndex = 31
        },
        ["Remarks"] = new()
        {
            ColumnIndex = 32
        }
    };

    public override void Fill(IEnumerable<InvoiceLineDM> lines)
    {
        if (lines == null || !lines.Any()) return;

        DeleteExistingLine();

        foreach (var line in lines)
        {
            AddNewLine();
            FillLine(line);
            //WaitForLoader();
        }
    }

    // ── Core Line Fill ────────────────────────────────────────────────────
    private void FillLine(InvoiceLineDM line)
    {
        if (!string.IsNullOrWhiteSpace(line.Barcode))
        {
            Lookup("Barcode", line.Barcode);
        }
        else
        {
            LookupCell("Item", line.Item);
        }

        SetCell("Description", line.Description);
        LookupCell("Color", line.Color);
        LookupCell("Size", line.Size);
        LookupCell("Warehouse", line.Warehouse);

        SetCell("Quantity", line.Quantity);
        SetCell("UnitPrice", line.UnitPrice);
        SetCell("GrossAmount", line.GrossAmount);
        SetCell("BonusQty", line.BonusQty);

        if (!string.IsNullOrWhiteSpace(line.UOM) ||
            !string.IsNullOrWhiteSpace(line.Remarks) ||
            line.DiscountInPercent > 0 ||
            line.DiscountValue > 0)
        {
            ClickToShowExtraFields();
        }

        LookupCell("UOM", line.UOM);
        SetCell("DiscountPercent", line.DiscountInPercent);
        SetCell("DiscountValue", line.DiscountValue);
        SetCell("Remarks", line.Remarks);
    }
}