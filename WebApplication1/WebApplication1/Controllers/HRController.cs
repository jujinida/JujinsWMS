using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HRController : ControllerBase
    {
        private readonly string _connectionString;

        public HRController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
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
