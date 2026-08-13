namespace App.Automation.Core.DataModels.Shared;

public class ExpectedResultDM
{
    /// <summary>Expected document status after the flow completes.</summary>
    /// <example>Draft, Submitted, Approved, Cancelled</example>
    public string? Status { get; set; }

    public MessageDM Messages { get; set; } = new();

    public string? PaymentStatus { get; set; }

    /// <summary>Expected success message text to appear in toast/notification.</summary>
    public string? SuccessMessage { get; set; }

    /// <summary>Expected error/validation message for negative test cases.</summary>
    public string? ErrorMessage { get; set; }

    public string? ValidationMessage { get; set; } = null;

    /// <summary>Expected totals — validated by TotalsValidator.</summary>
    public ExpectedTotalsDM? Totals { get; set; }
}

public class MessageDM
{
    public string? OnSave { get; set; }
    public string? OnView { get; set; }
    public string? OnEdit { get; set; }
    public string? OnRevise { get; set; }
    public string? OnReject { get; set; }
    public string? OnApprove { get; set; }
    public string? OnDelete { get; set; }
    public string? OnSubmit { get; set; }
}