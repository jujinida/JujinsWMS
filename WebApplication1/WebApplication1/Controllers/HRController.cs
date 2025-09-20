using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
// ILogger를 사용하기 위해 추가
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HRController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<HRController> _logger;
        private readonly IConfiguration _configuration;

        public HRController(IConfiguration configuration, ILogger<HRController> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"
                    SELECT 
                        employee_idx,
                        employee_name,
                        birth_date,
                        hire_date,
                        phone_number,
                        address,
                        position,
                        email,
                        department_id,
                        salary,
                        profile_url
                    FROM Employees
                    ORDER BY employee_idx";

                using var cmd = new SqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var employees = new List<EmployeeDto>();
                while (await reader.ReadAsync())
                {
                    var employee = new EmployeeDto();

                    // 각 필드를 안전하게 읽기 (SQL 쿼리 순서에 맞춰 인덱스 수정)
                    employee.EmployeeId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);  // employee_idx
                    employee.EmployeeName = reader.IsDBNull(1) ? "" : reader.GetString(1);  // employee_name
                    employee.BirthDate = reader.IsDBNull(2) ? "" : reader.GetDateTime(2).ToString("yyyy-MM-dd");  // birth_date
                    employee.HireDate = reader.IsDBNull(3) ? "" : reader.GetDateTime(3).ToString("yyyy-MM-dd");  // hire_date
                    employee.PhoneNumber = reader.IsDBNull(4) ? "" : reader.GetString(4);  // phone_number
                    employee.Address = reader.IsDBNull(5) ? "" : reader.GetString(5);  // address
                    employee.Position = reader.IsDBNull(6) ? "" : reader.GetString(6);  // position
                    employee.Email = reader.IsDBNull(7) ? "" : reader.GetString(7);  // email
                    employee.DepartmentId = reader.IsDBNull(8) ? 0 : reader.GetInt32(8);  // department_id
                    employee.Salary = reader.IsDBNull(9) ? 0 : reader.GetInt32(9);  // salary
                    employee.ProfileUrl = reader.IsDBNull(10) ? "" : reader.GetString(10);  // profile_url
                    
                    employees.Add(employee);
                }

                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"직원 목록 조회 중 오류가 발생했습니다: {ex.Message}" });
            }
        }

        [HttpPut("employees/{employeeId}")]
        public async Task<IActionResult> UpdateEmployee(int employeeId, [FromBody] EmployeeRegistrationRequest request)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"
                    UPDATE Employees SET
                        employee_name = @EmployeeName,
                        birth_date = @BirthDate,
                        hire_date = @HireDate,
                        phone_number = @PhoneNumber,
                        address = @Address,
                        position = @Position,
                        email = @Email,
                        department_id = @DepartmentId,
                        salary = @Salary,
                        profile_url = @ProfileUrl
                    WHERE employee_idx = @EmployeeId";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@EmployeeName", request.EmployeeName);
                cmd.Parameters.AddWithValue("@BirthDate", DateTime.Parse(request.BirthDate));
                cmd.Parameters.AddWithValue("@HireDate", DateTime.Parse(request.HireDate));
                cmd.Parameters.AddWithValue("@PhoneNumber", request.PhoneNumber);
                cmd.Parameters.AddWithValue("@Address", request.Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Position", request.Position);
                cmd.Parameters.AddWithValue("@Email", request.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DepartmentId", request.DepartmentId);
                cmd.Parameters.AddWithValue("@Salary", request.Salary);
                cmd.Parameters.AddWithValue("@ProfileUrl", request.ProfileUrl ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@EmployeeId", employeeId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return Ok(new { message = "직원 정보가 수정되었습니다." });
                }
                else
                {
                    return BadRequest(new { message = "직원 정보 수정에 실패했습니다." });
                }
            }
            catch (Exception ex)
            {
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
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"
                    INSERT INTO Employees (
                        employee_name, 
                        birth_date, 
                        hire_date, 
                        phone_number, 
                        address, 
                        position, 
                        email, 
                        department_id, 
                        salary, 
                        profile_url
                    ) 
                    VALUES (
                        @EmployeeName, 
                        @BirthDate, 
                        @HireDate, 
                        @PhoneNumber, 
                        @Address, 
                        @Position, 
                        @Email, 
                        @DepartmentId, 
                        @Salary, 
                        @ProfileUrl
                    )";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@EmployeeName", request.EmployeeName);
                cmd.Parameters.AddWithValue("@BirthDate", DateTime.Parse(request.BirthDate));
                cmd.Parameters.AddWithValue("@HireDate", DateTime.Parse(request.HireDate));
                cmd.Parameters.AddWithValue("@PhoneNumber", request.PhoneNumber);
                cmd.Parameters.AddWithValue("@Address", request.Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Position", request.Position);
                cmd.Parameters.AddWithValue("@Email", request.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DepartmentId", request.DepartmentId);
                cmd.Parameters.AddWithValue("@Salary", request.Salary);
                cmd.Parameters.AddWithValue("@ProfileUrl", request.ProfileUrl ?? (object)DBNull.Value);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return Ok(new { message = "직원 등록이 완료되었습니다." });
                }
                else
                {
                    return BadRequest(new { message = "직원 등록에 실패했습니다." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"직원 등록 중 오류가 발생했습니다: {ex.Message}" });
            }
        }

        [HttpGet("vacation-requests")]
        public async Task<IActionResult> GetVacationRequests()
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"
                    SELECT 
                        V.request_id,
                        E.employee_idx, 
                        E.employee_name, 
                        E.department_id, 
                        E.position, 
                        V.reason, 
                        V.start_date, 
                        V.end_date, 
                        V.vacation_days, 
                        V.is_half_day, 
                        E.remaining_vacation_days, 
                        E.total_vacation_days, 
                        V.approval_status
                    FROM Employees E 
                    LEFT JOIN Vacation_Requests V ON E.employee_idx = V.employee_idx
                    ORDER BY E.employee_idx";

                using var cmd = new SqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var vacationRequests = new List<VacationRequestDto>();

                while (await reader.ReadAsync())
                {
                    var vacationRequest = new VacationRequestDto
                    {
                        RequestId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        EmployeeId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                        EmployeeName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        DepartmentId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        Position = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        Reason = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        StartDate = reader.IsDBNull(6) ? "" : reader.GetDateTime(6).ToString("yyyy-MM-dd"),
                        EndDate = reader.IsDBNull(7) ? "" : reader.GetDateTime(7).ToString("yyyy-MM-dd"),
                        VacationDays = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                        IsHalfDay = reader.IsDBNull(9) ? false : reader.GetBoolean(9),
                        RemainingVacationDays = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10),
                        TotalVacationDays = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                        ApprovalStatus = reader.IsDBNull(12) ? "" : reader.GetString(12)
                    };

                    vacationRequests.Add(vacationRequest);
                }

                return Ok(vacationRequests);
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
