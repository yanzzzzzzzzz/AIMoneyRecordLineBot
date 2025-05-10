using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AIMoneyRecordLineBot.Entity;

[Table("ExpenseRecords")]
public class ExpenseRecord
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    [MaxLength(50)]
    public string? Source { get; set; }

    [Required]
    public DateTime ConsumptionTime { get; set; }

    [Required]
    public DateTime CreateDateTime { get; set; }

    [Required]
    public int UserId { get; set; }
}