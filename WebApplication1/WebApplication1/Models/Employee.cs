using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        [Column("employee_idx")]
        public int EmployeeId { get; set; }

        [Column("employee_name")]
        public string EmployeeName { get; set; } = string.Empty;

        [Column("birth_date")]
        public DateTime? BirthDate { get; set; }

        [Column("hire_date")]
        public DateTime? HireDate { get; set; }

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("position")]
        public string? Position { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("department_id")]
        public int DepartmentId { get; set; }

        [Column("salary")]
        public int Salary { get; set; }

        [Column("profile_url")]
        public string? ProfileUrl { get; set; }

        [Column("remaining_vacation_days")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? RemainingVacationDays { get; set; }

        [Column("total_vacation_days")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? TotalVacationDays { get; set; }

        // Navigation Properties
        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public virtual ICollection<VacationRequest> VacationRequests { get; set; } = new List<VacationRequest>();
        public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
    }
}
