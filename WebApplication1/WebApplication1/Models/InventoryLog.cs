using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("InventoryLogs")]
    public class InventoryLog
    {
        [Key]
        [Column("log_id")]
        public int LogId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("log_date")]
        public DateTime LogDate { get; set; }

        [Column("change_type")]
        public string ChangeType { get; set; } = string.Empty;

        [Column("quantity_changed")]
        public int QuantityChanged { get; set; }

        [Column("current_quantity")]
        public int CurrentQuantity { get; set; }

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
