using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
// ILogger를 사용하기 위해 추가
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HRController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<HRController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public HRController(IConfiguration configuration, ILogger<HRController> logger, ApplicationDbContext context, IMapper mapper)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var employeeDtos = await _context.Employees
                    .OrderBy(e => e.EmployeeId)
                    .ProjectTo<EmployeeDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(employeeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "직원 목록 조회 중 오류 발생");
                return StatusCode(500, new { message = $"직원 목록 조회 중 오류가 발생했습니다: {ex.Message}" });
            }
        }

        [HttpPut("employees/{employeeId}")]
        public async Task<IActionResult> UpdateEmployee(int employeeId, [FromBody] EmployeeRegistrationRequest request)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    return NotFound(new { message = "직원을 찾을 수 없습니다." });
                }

                // AutoMapper로 업데이트
                _mapper.Map(request, employee);

                await _context.SaveChangesAsync();

                return Ok(new { message = "직원 정보가 수정되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "직원 정보 수정 중 오류 발생");
                return StatusCode(500, new { message = $"직원 정보 수정 중 오류가 발생했습니다: {ex.Message}" });
            }
        }

        [HttpGet("attendance-records")]
        public async Task<IActionResult> GetAttendanceRecords([FromQuery] string? date = null)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // 날짜가 제공되지 않으면 오늘 날짜 사용
                string targetDate = string.IsNullOrEmpty(date) ? "CONVERT(DATE,GETDATE())" : $"'{date}'";

                string query = $@"
                    SELECT 
                        E.employee_idx, 
                        E.employee_name, 
                        E.department_id, 
                        E.position, 
                        A.check_in_time, 
                        A.check_out_time, 
                        A.work_hours, 
                        A.work_minutes, 
                        S.late_count, 
                        S.absent_without_leave_count
                    FROM Employees E 
                    LEFT JOIN Attendance_Records A ON E.employee_idx = A.employee_idx 
                    LEFT JOIN Attendance_Summary S ON A.employee_idx = S.employee_idx
                    WHERE A.record_date = {targetDate}
                    ORDER BY E.employee_idx";

                using var cmd = new SqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var attendanceRecords = new List<AttendanceRecordDto>();
                while (await reader.ReadAsync())
                {
                    var record = new AttendanceRecordDto();
                    
                    // 각 필드를 안전하게 읽기
                    record.EmployeeId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    record.EmployeeName = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    record.DepartmentId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    record.Position = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    record.CheckInTime = reader.IsDBNull(4) ? "" : reader.GetDateTime(4).ToString("HH:mm");
                    record.CheckOutTime = reader.IsDBNull(5) ? "" : reader.GetDateTime(5).ToString("HH:mm");
                    record.WorkHours = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                    record.WorkMinutes = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
                    record.LateCount = reader.IsDBNull(8) ? 0 : reader.GetInt32(8);
                    record.AbsentWithoutLeaveCount = reader.IsDBNull(9) ? 0 : reader.GetInt32(9);
                    
                    attendanceRecords.Add(record);
                }

                return Ok(attendanceRecords);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"출근 기록 조회 중 오류가 발생했습니다: {ex.Message}" });
            }
        }

        [HttpGet("attendance-records/{employeeId}/monthly")]
        public async Task<IActionResult> GetMonthlyAttendanceRecords(int employeeId, [FromQuery] string? yearMonth = null)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // 년월이 제공되지 않으면 현재 년월 사용
                DateTime targetDate = string.IsNullOrEmpty(yearMonth) ? DateTime.Now : DateTime.Parse($"{yearMonth}-01");
                string startDate = targetDate.ToString("yyyy-MM-01");
                string endDate = targetDate.AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");

                string query = $@"
                    SELECT 
                        A.record_date,
                        A.check_in_time, 
                        A.check_out_time, 
                        A.work_hours, 
                        A.work_minutes,
                        CASE 
                            WHEN A.check_in_time IS NULL AND A.check_out_time IS NULL THEN '결근'
                            WHEN CAST(A.check_in_time AS TIME) > CAST('09:01:00' AS TIME) THEN '지각'
                            ELSE '정상'
                        END as attendance_status,
                        CAST(A.check_in_time AS TIME) as check_in_time_only
                    FROM Attendance_Records A
                    WHERE A.employee_idx = @EmployeeId 
                    AND A.record_date BETWEEN '{startDate}' AND '{endDate}'
                    ORDER BY A.record_date";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                using var reader = await cmd.ExecuteReaderAsync();

                var monthlyRecords = new List<MonthlyAttendanceRecordDto>();
                while (await reader.ReadAsync())
                {
                    var record = new MonthlyAttendanceRecordDto();
                    
                    record.RecordDate = reader.IsDBNull(0) ? "" : reader.GetDateTime(0).ToString("yyyy-MM-dd");
                    record.CheckInTime = reader.IsDBNull(1) ? "" : reader.GetDateTime(1).ToString("HH:mm");
                    record.CheckOutTime = reader.IsDBNull(2) ? "" : reader.GetDateTime(2).ToString("HH:mm");
                    record.WorkHours = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                    record.WorkMinutes = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                    record.AttendanceStatus = reader.IsDBNull(5) ? "" : reader.GetString(5);
                    
                    // 디버깅을 위한 로그 - 원본 시간 값도 확인
                    var originalTime = reader.IsDBNull(6) ? "" : reader.GetTimeSpan(6).ToString(@"hh\:mm");
                    _logger.LogInformation($"출근 기록 - 날짜: {record.RecordDate}, 출근시간: {record.CheckInTime}, 원본시간: {originalTime}, 상태: {record.AttendanceStatus}");
                    
                    monthlyRecords.Add(record);
                }

                return Ok(monthlyRecords);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"월별 출근 기록 조회 중 오류가 발생했습니다: {ex.Message}" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterEmployee([FromBody] EmployeeRegistrationRequest request)
        {
            try
            {
                var employee = _mapper.Map<Employee>(request);

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                return Ok(new { message = "직원 등록이 완료되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "직원 등록 중 오류 발생");
                return StatusCode(500, new { message = $"직원 등록 중 오류가 발생했습니다: {ex.Message}" });
            }
        }

        [HttpGet("vacation-requests")]
        public async Task<IActionResult> GetVacationRequests()
        {
            try
            {
                var vacationRequestDtos = await _context.VacationRequests
                    .Include(v => v.Employee)
                    .OrderBy(v => v.EmployeeId)
                    .ProjectTo<VacationRequestDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(vacationRequestDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "휴가 신청 목록을 가져오는 중 오류가 발생했습니다.");
                return StatusCode(500, "서버 오류가 발생했습니다.");
            }
        }

        [HttpPut("vacation-requests/{requestId}/approve")]
        public async Task<IActionResult> ApproveVacationRequest(int requestId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // 트랜잭션 시작
                using var transaction = conn.BeginTransaction();

                try
                {
                    // 휴가 신청 정보 조회
                    string selectQuery = @"
                        SELECT employee_idx, vacation_days, approval_status 
                        FROM Vacation_Requests 
                        WHERE request_id = @RequestId";

                    using var selectCmd = new SqlCommand(selectQuery, conn, transaction);
                    selectCmd.Parameters.AddWithValue("@RequestId", requestId);

                    using var reader = await selectCmd.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                    {
                        return NotFound("휴가 신청을 찾을 수 없습니다.");
                    }

                    int employeeIdx = reader.GetInt32(0);
                    decimal vacationDays = reader.GetDecimal(1);
                    string currentStatus = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    
                    reader.Close();

                    // 승인 상태 업데이트
                    string updateVacationQuery = @"
                        UPDATE Vacation_Requests 
                        SET approval_status = '승인' 
                        WHERE request_id = @RequestId";

                    using var updateVacationCmd = new SqlCommand(updateVacationQuery, conn, transaction);
                    updateVacationCmd.Parameters.AddWithValue("@RequestId", requestId);
                    await updateVacationCmd.ExecuteNonQueryAsync();

                    // 잔여 휴가일수 차감
                    string updateEmployeeQuery = @"
                        UPDATE Employees 
                        SET remaining_vacation_days = remaining_vacation_days - @VacationDays 
                        WHERE employee_idx = @EmployeeIdx";

                    using var updateEmployeeCmd = new SqlCommand(updateEmployeeQuery, conn, transaction);
                    updateEmployeeCmd.Parameters.AddWithValue("@VacationDays", vacationDays);
                    updateEmployeeCmd.Parameters.AddWithValue("@EmployeeIdx", employeeIdx);
                    await updateEmployeeCmd.ExecuteNonQueryAsync();

                    // 트랜잭션 커밋
                    transaction.Commit();

                    return Ok(new { message = "휴가가 승인되었습니다." });
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "휴가 승인 중 오류가 발생했습니다.");
                return StatusCode(500, "휴가 승인 중 오류가 발생했습니다.");
            }
        }

        [HttpPut("vacation-requests/{requestId}/reject")]
        public async Task<IActionResult> RejectVacationRequest(int requestId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // 트랜잭션 시작
                using var transaction = conn.BeginTransaction();

                try
                {
                    // 휴가 신청 정보 조회
                    string selectQuery = @"
                        SELECT employee_idx, vacation_days, approval_status 
                        FROM Vacation_Requests 
                        WHERE request_id = @RequestId";

                    using var selectCmd = new SqlCommand(selectQuery, conn, transaction);
                    selectCmd.Parameters.AddWithValue("@RequestId", requestId);

                    using var reader = await selectCmd.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                    {
                        return NotFound("휴가 신청을 찾을 수 없습니다.");
                    }

                    int employeeIdx = reader.GetInt32(0);
                    decimal vacationDays = reader.GetDecimal(1);
                    string currentStatus = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    
                    reader.Close();

                    // 승인 상태를 거부로 업데이트
                    string updateVacationQuery = @"
                        UPDATE Vacation_Requests 
                        SET approval_status = '거부' 
                        WHERE request_id = @RequestId";

                    using var updateVacationCmd = new SqlCommand(updateVacationQuery, conn, transaction);
                    updateVacationCmd.Parameters.AddWithValue("@RequestId", requestId);
                    await updateVacationCmd.ExecuteNonQueryAsync();

                    // 이전 상태가 승인이었다면 잔여 휴가일수 복원
                    if (currentStatus == "승인")
                    {
                        string updateEmployeeQuery = @"
                            UPDATE Employees 
                            SET remaining_vacation_days = remaining_vacation_days + @VacationDays 
                            WHERE employee_idx = @EmployeeIdx";

                        using var updateEmployeeCmd = new SqlCommand(updateEmployeeQuery, conn, transaction);
                        updateEmployeeCmd.Parameters.AddWithValue("@VacationDays", vacationDays);
                        updateEmployeeCmd.Parameters.AddWithValue("@EmployeeIdx", employeeIdx);
                        await updateEmployeeCmd.ExecuteNonQueryAsync();
                    }

                    // 트랜잭션 커밋
                    transaction.Commit();

                    return Ok(new { message = "휴가가 거부되었습니다." });
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "휴가 거부 중 오류가 발생했습니다.");
                return StatusCode(500, "휴가 거부 중 오류가 발생했습니다.");
            }
        }

        [HttpGet("payroll")]
        public async Task<IActionResult> GetPayrollData([FromQuery] string paymentMonth = "2025-09")
        {
            try
            {
                var payrollDtos = await _context.Payrolls
                    .Include(p => p.Employee)
                    .Where(p => p.PaymentMonth == paymentMonth)
                    .OrderBy(p => p.EmployeeId)
                    .ProjectTo<PayrollDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(payrollDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "급여 데이터를 가져오는 중 오류가 발생했습니다.");
                return StatusCode(500, "급여 데이터를 가져오는 중 오류가 발생했습니다.");
            }
        }
    }

    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string BirthDate { get; set; }
        public string HireDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public int DepartmentId { get; set; }
        public int Salary { get; set; }
        public string ProfileUrl { get; set; }
    }

    public class AttendanceRecordDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int DepartmentId { get; set; }
        public string Position { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public int WorkHours { get; set; }
        public int WorkMinutes { get; set; }
        public int LateCount { get; set; }
        public int AbsentWithoutLeaveCount { get; set; }
    }

    public class MonthlyAttendanceRecordDto
    {
        public string RecordDate { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public int WorkHours { get; set; }
        public int WorkMinutes { get; set; }
        public string AttendanceStatus { get; set; }
    }

    public class EmployeeRegistrationRequest
    {
        public string EmployeeName { get; set; }
        public string BirthDate { get; set; }
        public string HireDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public int DepartmentId { get; set; }
        public int Salary { get; set; }
        public string ProfileUrl { get; set; }
    }

    public class PayrollDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int DepartmentId { get; set; }
        public string Position { get; set; }
        public decimal GrossPay { get; set; }
        public decimal Allowance { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }
        public string PaymentMonth { get; set; }
        public string PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
    }

    public class VacationRequestDto
    {
        public int RequestId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int DepartmentId { get; set; }
        public string Position { get; set; }
        public string Reason { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public decimal VacationDays { get; set; }
        public bool IsHalfDay { get; set; }
        public decimal RemainingVacationDays { get; set; }
        public int TotalVacationDays { get; set; }
        public string ApprovalStatus { get; set; }
    }
}
