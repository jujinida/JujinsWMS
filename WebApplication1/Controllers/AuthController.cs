using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // DB 연결 문자열 - 설정 파일에서 읽기
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string pwHash = null;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT user_pw_hash FROM Users WHERE user_id = @userId";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", request.UserId);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        pwHash = result.ToString();
                    }
                }
            }

            if (pwHash == null)
            {
                return Unauthorized(new { message = "존재하지 않는 ID입니다." });
            }

            // Bcrypt를 사용하여 비밀번호 검증
            if (!BCrypt.Net.BCrypt.Verify(request.Password, pwHash))
            {
                return Unauthorized(new { message = "비밀번호가 일치하지 않습니다." });
            }

            // JWT 생성
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = _configuration["JWT:SecretKey"];
            
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT 시크릿 키가 설정되지 않았습니다. appsettings.json을 확인하세요.");
            }
            
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("userId", request.UserId)
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwtToken = tokenHandler.WriteToken(token);

            return Ok(new { token = jwtToken });
        }
    }

    public class LoginRequest
    {
        public string UserId { get; set; }
        public string Password { get; set; }
    }
}