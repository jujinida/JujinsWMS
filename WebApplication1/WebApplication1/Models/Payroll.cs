using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Payroll")]
    public class Payroll
    {
        [Key]
        [Column("payment_id")]
        public int PayrollId { get; set; }

        [Column("employee_idx")]
        public int EmployeeId { get; set; }

        [Column("gross_pay")]
        public decimal? GrossPay { get; set; }

        [Column("allowance")]
        public decimal? Allowance { get; set; }

        [Column("deductions")]
        public decimal? Deductions { get; set; }

        [Column("net_pay")]
        public decimal? NetPay { get; set; }

        [Column("payment_month")]
        public string? PaymentMonth { get; set; }

        [Column("payment_date")]
        public DateTime? PaymentDate { get; set; }

        [Column("payment_status")]
        public string? PaymentStatus { get; set; }

        // Navigation Properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;
    }
}
