namespace FinancialTracker.Model
{
    public class Transaction
    {
        public string? Id { get; set; }
        public string? Category { get; set; }
        public string? Type { get; set; } // Income / Expense
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public double Balance { get; set; }
    }
}
