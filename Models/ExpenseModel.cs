namespace AIMoneyRecordLineBot.Models
{
    public class ExpenseModel
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public DateTime? ConsumptionTime { get; set; }
    }

}
