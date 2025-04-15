namespace SV21T1020578.DomainModels
{
    public class Customer
    {
        public int CustomerId { get; set; } = 0;
        public string CustomerName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsLocked { get; set; } = false;

    }
}
