namespace AIMoneyRecordLineBot.Models
{
    public class ExpenseItem
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public DateTime? ConsumptionTime { get; set; }
    }

}
