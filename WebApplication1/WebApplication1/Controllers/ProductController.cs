using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Amazon.S3.Model;
using System.Text;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAmazonS3 _s3Client;

        public ProductController(ILogger<ProductController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            // AWS S3 클라이언트 설정
            var s3Config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2,
                ServiceURL = "https://s3.ap-southeast-2.amazonaws.com"
            };

            _s3Client = new AmazonS3Client("AKIASKD5PB3ZHMNVFSVG", "3mmMbruDzQfsZ61PSCsE6zo92aDc0EmlBA/Axu0I", s3Config);
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterProduct([FromBody] ProductRegistrationRequest request)
        {
            try
            {
                _logger.LogInformation("제품 등록 요청: {ProductName}", request.ProductName);

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    var query = @"
                        INSERT INTO Products (product_name, category, price, stock_quantity, pd_url)
                        VALUES (@product_name, @category, @price, @stock_quantity, @pd_url)";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@product_name", request.ProductName);
                        cmd.Parameters.AddWithValue("@category", request.Category ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@price", request.Price);
                        cmd.Parameters.AddWithValue("@stock_quantity", request.StockQuantity);
                        cmd.Parameters.AddWithValue("@pd_url", request.ImageUrl ?? (object)DBNull.Value);

                        var result = await cmd.ExecuteNonQueryAsync();

                        if (result > 0)
                        {
                            _logger.LogInformation("제품 등록 성공: {ProductName}", request.ProductName);
                            return Ok(new { message = "제품이 성공적으로 등록되었습니다." });
                        }
                        else
                        {
                            _logger.LogWarning("제품 등록 실패: {ProductName}", request.ProductName);
                            return BadRequest(new { message = "제품 등록에 실패했습니다." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "제품 등록 중 오류 발생");
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

    }

    public class ProductRegistrationRequest
    {
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
    }

}
