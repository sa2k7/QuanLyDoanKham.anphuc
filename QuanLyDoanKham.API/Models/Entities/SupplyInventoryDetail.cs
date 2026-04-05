using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class SupplyInventoryDetail
    {
        [Key]
        public int Id { get; set; }

        public int VoucherId { get; set; }
        [ForeignKey("VoucherId")]
        public SupplyInventoryVoucher Voucher { get; set; }

        public int SupplyId { get; set; }
        [ForeignKey("SupplyId")]
        public Supply Supply { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
    }
}
