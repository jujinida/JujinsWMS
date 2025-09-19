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
}
