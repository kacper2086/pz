namespace pz.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }

        // Jeśli potrzebne są klucze obce, można dodać właściwości dla nich
        // Na przykład:
        // public Customer Customer { get; set; }
        // public Product Product { get; set; }
    }
}
