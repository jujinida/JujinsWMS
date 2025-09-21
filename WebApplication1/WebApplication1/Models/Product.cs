using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("product_name")]
        public string? ProductName { get; set; }

        [Column("category")]
        public string? Category { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        [Column("stock_quantity")]
        public int? StockQuantity { get; set; }

        [Column("safety_stock")]
        public int? SafetyStock { get; set; }

        [Column("pd_url")]
        public string? ImageUrl { get; set; }

        // Navigation Properties
        public virtual ICollection<ProductLocation> ProductLocations { get; set; } = new List<ProductLocation>();
        public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    }
}
