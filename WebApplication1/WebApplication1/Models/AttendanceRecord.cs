using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Attendance_Records")]
    public class AttendanceRecord
    {
        [Key]
        [Column("record_id")]
        public int RecordId { get; set; }

        [Column("employee_idx")]
        public int EmployeeId { get; set; }

        [Column("record_date")]
        public DateTime RecordDate { get; set; }

        [Column("check_in_time")]
        public DateTime? CheckInTime { get; set; }

        [Column("check_out_time")]
        public DateTime? CheckOutTime { get; set; }

        [Column("work_hours")]
        public int? WorkHours { get; set; }

        [Column("work_minutes")]
        public int? WorkMinutes { get; set; }

        // Navigation Properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;
    }
}
