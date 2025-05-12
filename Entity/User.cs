using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIMoneyRecordLineBot.Entity;

[Table("Users")]
public class User
{
    public int Id { get; set; }
    public string LineUserId { get; set; }
    public string LineDisplayName { get; set; }
    public DateTime CreateDateTime { get; set; }

    public virtual ICollection<ExpenseRecord> ExpenseRecords { get; set; }
}
