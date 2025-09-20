using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

// ILogger를 사용하기 위해 추가
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;


namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(ILogger<AuthController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public class EmployeeInfoDto
        {
            public string EmployeeId { get; set; }
            public string EmployeeName { get; set; }
            public DateTime? BirthDate { get; set; } // Nullable로 변경
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public string Position { get; set; }
            public string Email { get; set; }
            public string DepartmentName { get; set; }
            public string Profile_Url { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login 요청이 들어옴: UserId={UserId}, Timestamp={Timestamp}", request.UserId, DateTime.UtcNow);

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // 변수 선언을 try-catch 블록 바깥으로 이동
            string pwHash = null;
            EmployeeInfoDto employeeInfo = null;

            int maxRetries = 3;
            int retryDelay = 1000; // 1초

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        _logger.LogDebug("DB 연결 성공 (시도 {Attempt}/{MaxRetries})", attempt, maxRetries);

                    // 비밀번호 해시와 직원 정보를 하나의 쿼리로 조회
                    string query = @"SELECT 
                U.user_pw_hash,
                E.employee_id,
                E.employee_name,
                E.birth_date,
                E.phone_number,
                E.address,
                E.position,
                E.email,
                E.profile_url,
                D.department_name
            FROM Users U
            LEFT JOIN Employees E ON (U.user_id = E.employee_id)
            LEFT JOIN Departments D ON (E.department_id = D.department_id)
            WHERE U.user_id = @userId";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", request.UserId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // 1. 비밀번호 해시를 먼저 가져옴
                                pwHash = reader["user_pw_hash"]?.ToString();

                                // 2. 비밀번호 불일치 시 바로 반환
                                if (pwHash == null || !BCrypt.Net.BCrypt.Verify(request.Password, pwHash))
                                {
                                    _logger.LogWarning("로그인 실패: 비밀번호 불일치 또는 존재하지 않는 사용자. UserId={UserId}", request.UserId);
                                    return Unauthorized(new { message = "ID 또는 비밀번호가 일치하지 않습니다." });
                                }

                                // 3. 비밀번호 일치 시에만 직원 정보를 DTO에 매핑
                                employeeInfo = new EmployeeInfoDto
                                {
                                    EmployeeId = reader["employee_id"]?.ToString(),
                                    EmployeeName = reader["employee_name"]?.ToString(),
                                    BirthDate = reader["birth_date"] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader["birth_date"]),
                                    PhoneNumber = reader["phone_number"]?.ToString(),
                                    Address = reader["address"]?.ToString(),
                                    Position = reader["position"]?.ToString(),
                                    Email = reader["email"]?.ToString(),
                                    DepartmentName = reader["department_name"]?.ToString(),
                                    Profile_Url = reader["profile_url"]?.ToString()
                                };
                                _logger.LogDebug("사용자 정보 조회 및 비밀번호 확인 성공");
                            }
                            else
                            {
                                _logger.LogWarning("존재하지 않는 ID: {UserId}", request.UserId);
                                return Unauthorized(new { message = "존재하지 않는 ID입니다." });
                            }
                        }
                    }
                    }
                    
                    // 성공 시 루프 종료
                    break;
                }
                catch (SqlException sqlEx) when (attempt < maxRetries)
                {
                    _logger.LogWarning(sqlEx, "DB 연결 실패 (시도 {Attempt}/{MaxRetries}), 재시도 중...", attempt, maxRetries);
                    
                    // 연결 타임아웃이나 일시적 오류인 경우 재시도
                    if (sqlEx.Number == -2 || sqlEx.Number == 2 || sqlEx.Number == 53) // 연결 타임아웃 관련 오류
                    {
                        await Task.Delay(retryDelay * attempt); // 지수 백오프
                        continue;
                    }
                    else
                    {
                        // 다른 SQL 오류는 재시도하지 않음
                        _logger.LogError(sqlEx, "DB 연결 또는 쿼리 실행 중 오류 발생");
                        return StatusCode(500, new { message = "서버 오류" });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DB 연결 또는 쿼리 실행 중 오류 발생");
                    return StatusCode(500, new { message = "서버 오류" });
                }
            }

            // JWT 생성 (성공 시에만 실행)
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = _configuration["JWT:SecretKey"];
            
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT 시크릿 키가 설정되지 않았습니다. appsettings.json을 확인하세요.");
            }
            
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            var claims = new List<System.Security.Claims.Claim>
    {
        new System.Security.Claims.Claim("userId", request.UserId)
    };

            if (employeeInfo != null)
            {
                claims.Add(new System.Security.Claims.Claim("employeeId", employeeInfo.EmployeeId));
                claims.Add(new System.Security.Claims.Claim("employeeName", employeeInfo.EmployeeName));
                claims.Add(new System.Security.Claims.Claim("departmentName", employeeInfo.DepartmentName));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwtToken = tokenHandler.WriteToken(token);

            _logger.LogInformation("JWT 토큰 발급 완료: UserId={UserId}", request.UserId);

            return Ok(new { token = jwtToken, userInfo = employeeInfo });
        }
    }

    public class LoginRequest
    {
        public string UserId { get; set; }
        public string Password { get; set; }
    }
}