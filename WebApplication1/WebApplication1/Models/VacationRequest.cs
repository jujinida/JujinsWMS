using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Vacation_Requests")]
    public class VacationRequest
    {
        [Key]
        [Column("request_id")]
        public int RequestId { get; set; }

        [Column("employee_idx")]
        public int EmployeeId { get; set; }

        [Column("reason")]
        public string? Reason { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("vacation_days")]
        public decimal? VacationDays { get; set; }

        [Column("is_half_day")]
        public bool? IsHalfDay { get; set; }

        [Column("approval_status")]
        public string? ApprovalStatus { get; set; }

        // Navigation Properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;
    }
}
