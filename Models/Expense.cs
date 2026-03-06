using System.ComponentModel.DataAnnotations;

namespace TripTracker.Models;

public class Expense
{
    public int Id { get; set; }
    public int TripId { get; set; }

    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;

    public ExpenseCategory Category { get; set; } = ExpenseCategory.Other;

    [Range(0, 100000000)]
    public decimal OriginalAmount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string OriginalCurrency { get; set; } = "USD";

    [Range(0, 100000000)]
    public decimal ConvertedAmount { get; set; }

    [StringLength(100)]
    public string? PaidBy { get; set; }

    [StringLength(200)]
    public string? SplitWith { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public Trip? Trip { get; set; }
}

