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
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // DB 연결 문자열
            string connectionString = "Server=jujin.database.windows.net;Database=jujinscshop;User Id=jujin;Password=dkvm7607@;";
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
            var key = Encoding.ASCII.GetBytes("your-very-long-secret-key-here-1234");
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