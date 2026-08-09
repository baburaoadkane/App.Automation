namespace App.Automation.Core.DataModels.Shared
{
    public abstract class BaseDocumentDM
    {
        /// <summary>Unique document number — populated after Save.</summary>
        public string? DocumentNo { get; set; }

        /// <summary>Which test scenario this data represents: Create / Edit / Negative etc.</summary>
        public string? ScenarioType { get; set; }

        /// <summary>Human-readable description of this test case.</summary>
        public string? TestDescription { get; set; }

        /// <summary>Whether this document should go through Submit → Approve flow.</summary>
        public bool RequiresApproval { get; set; } = false;

        /// <summary>Whether this document should be Posted after approval.</summary>
        public bool RequiresPost { get; set; } = false;

        /// <summary>Expected outcome — used by validators after execution.</summary>
        public ExpectedResultDM? Expected { get; set; }
    }
}
