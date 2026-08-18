using App.Automation.Core.DataModels.Shared;
using App.Automation.Core.Enums;
using App.Automation.Core.Utilities;
using App.Automation.Modules.Sales.Invoice.DataModels;

namespace App.Automation.Modules.Sales.Invoice.Builders;

public class InvoiceBuilder
{
    private InvoiceDM _model;

    // ── Private constructor — use static entry points ──────────────────────
    private InvoiceBuilder()
    {
        _model = new InvoiceDM
        {
            Header = new InvoiceHeaderDM(),
            Discount = new InvoiceDiscountDM(),
            Lines = new List<InvoiceLineDM>(),
            Charges = new InvoiceChargesDM(),
            Payments = new InvoicePaymentsDM(),
            Others = new InvoiceOthersDM()
        };
    }

    // ── Static entry points ────────────────────────────────────────────────

    /// <summary>
    /// Load a SalesInvoiceDM from a JSON file.
    /// This is the PRIMARY pattern for data-driven tests.
    /// </summary>
    public static InvoiceBuilder FromJson(string jsonPath)
    {
        var builder = new InvoiceBuilder();
        builder._model = JsonLoader.Load<InvoiceDM>(jsonPath);
        return builder;
    }

    /// <summary>
    /// Start a blank model and build programmatically.
    /// Use for edge cases that don't warrant a separate JSON file.
    /// </summary>
    public static InvoiceBuilder New()
        => new InvoiceBuilder();

    // ── Header overrides ───────────────────────────────────────────────────

    /// <summary>Override the invoice date.</summary>
    public InvoiceBuilder WithInvoiceDate(string date)
    {
        _model.Header.InvoiceDate = date;
        return this;
    }

    /// <summary>Override the customer.</summary>
    public InvoiceBuilder WithCustomer(string customer)
    {
        _model.Header.Customer = customer;
        return this;
    }

    /// <summary>Override the currency.</summary>
    public InvoiceBuilder WithCurrency(string currency)
    {
        _model.Header.Currency = currency;
        return this;
    }

    /// <summary>Override the payment term.</summary>
    public InvoiceBuilder WithPriceList(string priceList)
    {
        _model.Header.PriceList = priceList;
        return this;
    }

    /// <summary>Override the location/warehouse.</summary>
    public InvoiceBuilder WithWarehouse(string warehouse)
    {
        _model.Header.Warehouse = warehouse;
        return this;
    }

    /// <summary>Override the salesman.</summary>
    public InvoiceBuilder WithSalesman(string salesman)
    {
        _model.Header.Salesman = salesman;
        return this;
    }

    /// <summary>Override the payment method.</summary>
    public InvoiceBuilder WithPaymentMethod(string paymentMethod)
    {
        _model.Header.PaymentMethod = paymentMethod;
        return this;
    }

    /// <summary>Override the reference number.</summary>
    public InvoiceBuilder WithReferenceNum(string referenceNo)
    {
        _model.Header.ReferenceNum = referenceNo;
        return this;
    }

    /// <summary>Override the customer PO number.</summary>
    public InvoiceBuilder WithCustomerPONum(string customerPONum)
    {
        _model.Header.CustomerPONum = customerPONum;
        return this;
    }

    /// <summary>Override the customer mobile number.</summary>
    public InvoiceBuilder WithCustomerMobileNum(string customerMobileNum)
    {
        _model.Header.MobileNum = customerMobileNum;
        return this;
    }

    // ── Line item fluent methods ───────────────────────────────────────────

    /// <summary>
    /// Add a line item programmatically.
    /// Calculates ExpectedLineTotal automatically.
    /// </summary>
    public InvoiceBuilder AddLine(
        string barcode,
        string item,
        decimal qty = 1,
        decimal price = 0,
        string warehouse = "",
        string taxType = "Tax Exempt",
        decimal taxPercent = 0,
        decimal discountInPercent = 0,
        decimal discountValue = 0,
        string remarks = "",
        string uom = "")
    {
        decimal lineTotal = (qty * price) - discountValue;

        _model.Lines.Add(new InvoiceLineDM
        {
            Barcode = barcode,
            Item = item,
            Quantity = qty,
            UOM = uom,
            UnitPrice = price,
            Warehouse = warehouse,
            DiscountInPercent = discountInPercent,
            DiscountValue = discountValue,
            TaxType = taxType,
            TaxPercent = taxPercent,
            Remarks = remarks,
            ExpectedLineTotal = Math.Round(lineTotal, 2)
        });

        return this;
    }

    /// <summary>Clear all existing lines — useful when overriding JSON lines.</summary>
    public InvoiceBuilder ClearLines()
    {
        _model.Lines.Clear();
        return this;
    }

    // ── Charges fluent methods ─────────────────────────────────────────────

    /// <summary>Add a charge entry programmatically.</summary>
    public InvoiceBuilder AddCharge(
        string chargeType,
        decimal amount,
        string? taxType = null,
        bool isTaxable = false)
    {
        _model.Charges.Items.Add(new ChargeDM
        {
            ChargeType = chargeType,
            AmountFC = amount,
            TaxType = taxType,
            IsTaxable = isTaxable
        });

        return this;
    }

    // ── Payment fluent methods ─────────────────────────────────────────────

    /// <summary>Add a payment entry programmatically.</summary>
    public InvoiceBuilder AddPayment(
        string paymentMode,
        decimal amount,
        string? referenceNo = null,
        string? paymentDate = null,
        string? account = null)
    {
        _model.Payments.Entries.Add(new PaymentEntryDM
        {
            PaymentMode = paymentMode,
            AmountFC = amount,
            ReferenceNo = referenceNo,
            PaymentDate = paymentDate,
            Account = account
        });

        return this;
    }

    // ── Remarks fluent methods ─────────────────────────────────────────────

    /// <summary>Set the remarks field.</summary>
    public InvoiceBuilder WithRemarks(string remarks)
    {
        _model.Others.Remarks = remarks;
        return this;
    }

    // ── Workflow fluent methods ────────────────────────────────────────────

    /// <summary>
    /// Mark this invoice for the Save → Submit → Approve flow.
    /// Sets RequiresApproval = true and ScenarioType = "Approval".
    /// Initializes the Approval property with default ApprovalAction.Approve.
    /// </summary>
    //public InvoiceBuilder WithApproval()
    //{
    //    _model.RequiresApproval = true;
    //    _model.ScenarioType = "Approval";
    //    _model.Approval = new ApprovalDM
    //    {
    //        ApprovalLevel = 1,
    //        Action = ApprovalAction.Approve,
    //        Approver = null
    //    };
    //    return this;
    //}

    /// <summary>Override the scenario type directly.</summary>
    public InvoiceBuilder AsScenario(string scenarioType)
    {
        _model.ScenarioType = scenarioType;
        return this;
    }

    /// <summary>Set the document number — required for Edit and Validation scenarios.</summary>
    public InvoiceBuilder ForDocument(string documentNo)
    {
        _model.DocumentNo = documentNo;
        return this;
    }

    // ── Build ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Finalize and return the fully constructed SalesInvoiceDM.
    /// Validates that required fields are present before returning.
    /// </summary>
    public InvoiceDM Build()
    {
        ValidateModel();
        return _model;
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private void ValidateModel()
    {
        string scenario = _model.ScenarioType?.ToUpperInvariant() ?? "CREATE";

        // Edit and Validation scenarios require a DocumentNo
        if ((scenario == "EDIT")
            && string.IsNullOrWhiteSpace(_model.DocumentNo))
        {
            throw new InvalidOperationException(
                $"[InvoiceBuilder] ScenarioType '{scenario}' requires DocumentNo. " +
                $"Call .ForDocument(\"SI-2025-0001\") before .Build().");
        }

        // Non-negative scenarios require at least a customer
        //if (scenario != "NEGATIVE"
        //    && string.IsNullOrWhiteSpace(_model.Header?.Customer))
        //{
        //    throw new InvalidOperationException(
        //        "[InvoiceBuilder] Header.Customer is required for non-negative scenarios. " +
        //        "Set it in the JSON file or call .WithCustomer(\"...\").");
        //}
    }
}