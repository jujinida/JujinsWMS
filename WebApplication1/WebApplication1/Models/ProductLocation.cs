using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Product_Locations")]
    public class ProductLocation
    {
        [Key]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Key]
        [Column("location_id")]
        public int LocationId { get; set; }

        [Column("stock_quantity")]
        public int StockQuantity { get; set; }

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
