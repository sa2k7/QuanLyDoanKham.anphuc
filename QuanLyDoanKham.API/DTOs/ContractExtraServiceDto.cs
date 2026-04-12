namespace QuanLyDoanKham.API.DTOs
{
    public class ContractExtraServiceDto
    {
        public decimal ExtraServiceRevenue { get; set; }
        public string? ExtraServiceDetails { get; set; } // JSON list of items
        public decimal VATRate { get; set; }
    }
}
