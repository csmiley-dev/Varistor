using System;

namespace VAR
{
    public class ProjectInfo
    {
        public string ProjectName { get; set; } = "";
        public string ProjectNumber { get; set; } = "";
        public string ClientName { get; set; } = "";
    }

    public class Variation
    {
        public int Id { get; set; }
        public string VariationNumber { get; set; } = "";
        public string VariationName { get; set; } = "";
        public string VariationDate { get; set; } = "";
        public string? ClientContact { get; set; }
        public bool IsApproved { get; set; }
        public string? ApprovedBy { get; set; }
        public string? ApprovedDate { get; set; }
        public decimal TotalValue { get; set; }

        public string VariationType
        {
            get
            {
                if (TotalValue < 0) return "Credit";
                if (TotalValue == 0) return "Nil-Cost";
                return "Addition";
            }
        }
    }

    public class LineItem
    {
        public int Id { get; set; }
        public int VariationId { get; set; }
        public int ItemNumber { get; set; }
        public string ItemDescription { get; set; } = "";
        public string ItemType { get; set; } = "Cost";  // "Cost" or "Refund"
        public decimal MaterialQty { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal MaterialTotal => MaterialQty * MaterialCost * (ItemType == "Refund" ? -1 : 1);
        public decimal HourlyQty { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal LabourTotal => HourlyQty * HourlyRate * (ItemType == "Refund" ? -1 : 1);
        public decimal LineTotal => MaterialTotal + LabourTotal;
    }

    public class HourlyRate
    {
        public int Id { get; set; }
        public string RateName { get; set; } = "";
        public decimal RateValue { get; set; }
    }

    public class VariationSummary
    {
        public decimal TotalAdditions { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal NetValue => TotalAdditions + TotalCredits;

        public decimal ApprovedAdditions { get; set; }
        public decimal ApprovedCredits { get; set; }
        public decimal ApprovedNetValue => ApprovedAdditions + ApprovedCredits;
    }
}
