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


        [HttpGet("list")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                _logger.LogInformation("제품 목록 조회 요청");

                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                var products = new List<ProductDto>();

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT product_id, product_name, category, price, stock_quantity, pd_url
                        FROM Products
                        ORDER BY product_id";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var productId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                var productName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                                var category = reader.IsDBNull(2) ? null : reader.GetString(2);
                                var price = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                                var stockQuantity = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                                var imageUrl = reader.IsDBNull(5) ? null : reader.GetString(5);
                                
                                _logger.LogDebug("제품 데이터: ID={ProductId}, Name={ProductName}, Category={Category}, Price={Price}, Stock={StockQuantity}", 
                                    productId, productName, category, price, stockQuantity);
                                
                                products.Add(new ProductDto
                                {
                                    ProductId = productId,
                                    ProductName = productName,
                                    Category = category,
                                    Price = price,
                                    StockQuantity = stockQuantity,
                                    ImageUrl = imageUrl
                                });
                            }
                        }
                    }
                }

                _logger.LogInformation("제품 목록 조회 완료: {Count}개", products.Count);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "제품 목록 조회 중 오류 발생");
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
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

        [HttpPost("seed")]
        public async Task<IActionResult> SeedTestData()
        {
            try
            {
                _logger.LogInformation("테스트 데이터 생성 요청");

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    
                    // 기존 테스트 데이터 삭제
                    string deleteQuery = "DELETE FROM Products WHERE product_name LIKE '테스트%'";
                    using (var deleteCmd = new SqlCommand(deleteQuery, conn))
                    {
                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    // 테스트 데이터 삽입
                    string insertQuery = @"
                        INSERT INTO Products (product_name, category, price, stock_quantity, pd_url, created_at, updated_at)
                        VALUES 
                        ('테스트 제품 1', '주류', 15000, 50, 'https://example.com/image1.jpg', GETDATE(), GETDATE()),
                        ('테스트 제품 2', '음료', 3000, 100, 'https://example.com/image2.jpg', GETDATE(), GETDATE()),
                        ('테스트 제품 3', '기타', 25000, 30, 'https://example.com/image3.jpg', GETDATE(), GETDATE()),
                        ('테스트 제품 4', '주류', 45000, 20, 'https://example.com/image4.jpg', GETDATE(), GETDATE()),
                        ('테스트 제품 5', '음료', 5000, 80, 'https://example.com/image5.jpg', GETDATE(), GETDATE())";

                    using (var insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        await insertCmd.ExecuteNonQueryAsync();
                    }
                }

                _logger.LogInformation("테스트 데이터 생성 완료");
                return Ok(new { message = "테스트 데이터가 성공적으로 생성되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "테스트 데이터 생성 중 오류 발생");
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateRequest request)
        {
            try
            {
                _logger.LogInformation("제품 수정 요청: ProductId={ProductId}, ProductName={ProductName}", id, request.ProductName);

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"
                        UPDATE Products 
                        SET product_name = @ProductName,
                            category = @Category,
                            price = @Price,
                            stock_quantity = @StockQuantity,
                            pd_url = @ImageUrl,
                            updated_at = GETDATE()
                        WHERE product_id = @ProductId";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", id);
                        cmd.Parameters.AddWithValue("@ProductName", request.ProductName);
                        cmd.Parameters.AddWithValue("@Category", request.Category ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Price", request.Price);
                        cmd.Parameters.AddWithValue("@StockQuantity", request.StockQuantity);
                        cmd.Parameters.AddWithValue("@ImageUrl", request.ImageUrl ?? (object)DBNull.Value);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        
                        if (rowsAffected == 0)
                        {
                            return NotFound(new { message = "제품을 찾을 수 없습니다." });
                        }
                    }
                }

                _logger.LogInformation("제품 수정 완료: ProductId={ProductId}", id);
                return Ok(new { message = "제품이 성공적으로 수정되었습니다." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "제품 수정 중 오류 발생");
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        [HttpPost("receive/{id}")]
        public async Task<IActionResult> ReceiveProduct(int id, [FromBody] ProductReceiveRequest request)
        {
            try
            {
                _logger.LogInformation("제품 입고 요청: ProductId={ProductId}, Quantity={Quantity}", id, request.Quantity);

                if (request.Quantity <= 0)
                {
                    return BadRequest(new { message = "입고량은 0보다 커야 합니다." });
                }

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    
                    // 트랜잭션 시작
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 현재 재고량 조회
                            string selectQuery = "SELECT stock_quantity FROM Products WHERE product_id = @ProductId";
                            int currentStock = 0;
                            
                            using (var selectCmd = new SqlCommand(selectQuery, conn, transaction))
                            {
                                selectCmd.Parameters.AddWithValue("@ProductId", id);
                                var result = await selectCmd.ExecuteScalarAsync();
                                
                                if (result == null)
                                {
                                    return NotFound(new { message = "제품을 찾을 수 없습니다." });
                                }
                                
                                currentStock = Convert.ToInt32(result);
                            }

                            // 재고량 업데이트
                            int newStock = currentStock + request.Quantity;
                            string updateQuery = @"
                                UPDATE Products 
                                SET stock_quantity = @NewStock, updated_at = GETDATE()
                                WHERE product_id = @ProductId";

                            using (var updateCmd = new SqlCommand(updateQuery, conn, transaction))
                            {
                                updateCmd.Parameters.AddWithValue("@ProductId", id);
                                updateCmd.Parameters.AddWithValue("@NewStock", newStock);
                                
                                int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                                if (rowsAffected == 0)
                                {
                                    return NotFound(new { message = "제품을 찾을 수 없습니다." });
                                }
                            }

                            // 재고 로그 추가
                            string logQuery = @"
                                INSERT INTO InventoryLogs (product_id, change_type, quantity_changed, current_quantity)
                                VALUES (@ProductId, @ChangeType, @QuantityChanged, @CurrentQuantity)";

                            using (var logCmd = new SqlCommand(logQuery, conn, transaction))
                            {
                                logCmd.Parameters.AddWithValue("@ProductId", id);
                                logCmd.Parameters.AddWithValue("@ChangeType", "입고");
                                logCmd.Parameters.AddWithValue("@QuantityChanged", request.Quantity);
                                logCmd.Parameters.AddWithValue("@CurrentQuantity", newStock);
                                
                                await logCmd.ExecuteNonQueryAsync();
                            }

                            // 트랜잭션 커밋
                            transaction.Commit();
                            
                            _logger.LogInformation("제품 입고 완료: ProductId={ProductId}, OldStock={OldStock}, NewStock={NewStock}", 
                                id, currentStock, newStock);
                            
                            return Ok(new { 
                                message = "입고가 성공적으로 처리되었습니다.",
                                oldStock = currentStock,
                                newStock = newStock,
                                quantityReceived = request.Quantity
                            });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "제품 입고 중 오류 발생");
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        [HttpPost("ship/{id}")]
        public async Task<IActionResult> ShipProduct(int id, [FromBody] ProductShipRequest request)
        {
            try
            {
                _logger.LogInformation("제품 출고 요청: ProductId={ProductId}, Quantity={Quantity}", id, request.Quantity);

                if (request.Quantity <= 0)
                {
                    return BadRequest(new { message = "출고량은 0보다 커야 합니다." });
                }

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    
                    // 트랜잭션 시작
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 현재 재고량 조회
                            string selectQuery = "SELECT stock_quantity FROM Products WHERE product_id = @ProductId";
                            int currentStock = 0;
                            
                            using (var selectCmd = new SqlCommand(selectQuery, conn, transaction))
                            {
                                selectCmd.Parameters.AddWithValue("@ProductId", id);
                                var result = await selectCmd.ExecuteScalarAsync();
                                
                                if (result == null)
                                {
                                    return NotFound(new { message = "제품을 찾을 수 없습니다." });
                                }
                                
                                currentStock = Convert.ToInt32(result);
                            }

                            // 재고량이 부족한지 확인
                            if (currentStock < request.Quantity)
                            {
                                return BadRequest(new { message = $"재고가 부족합니다. 현재 재고: {currentStock}개, 요청 출고량: {request.Quantity}개" });
                            }

                            // 재고량 업데이트 (출고량만큼 차감)
                            int newStock = currentStock - request.Quantity;
                            string updateQuery = @"
                                UPDATE Products 
                                SET stock_quantity = @NewStock, updated_at = GETDATE()
                                WHERE product_id = @ProductId";

                            using (var updateCmd = new SqlCommand(updateQuery, conn, transaction))
                            {
                                updateCmd.Parameters.AddWithValue("@ProductId", id);
                                updateCmd.Parameters.AddWithValue("@NewStock", newStock);
                                
                                int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                                if (rowsAffected == 0)
                                {
                                    return NotFound(new { message = "제품을 찾을 수 없습니다." });
                                }
                            }

                            // 재고 로그 추가
                            string logQuery = @"
                                INSERT INTO InventoryLogs (product_id, change_type, quantity_changed, current_quantity)
                                VALUES (@ProductId, @ChangeType, @QuantityChanged, @CurrentQuantity)";

                            using (var logCmd = new SqlCommand(logQuery, conn, transaction))
                            {
                                logCmd.Parameters.AddWithValue("@ProductId", id);
                                logCmd.Parameters.AddWithValue("@ChangeType", "출고");
                                logCmd.Parameters.AddWithValue("@QuantityChanged", request.Quantity);
                                logCmd.Parameters.AddWithValue("@CurrentQuantity", newStock);
                                
                                await logCmd.ExecuteNonQueryAsync();
                            }

                            // 트랜잭션 커밋
                            transaction.Commit();
                            
                            _logger.LogInformation("제품 출고 완료: ProductId={ProductId}, OldStock={OldStock}, NewStock={NewStock}", 
                                id, currentStock, newStock);
                            
                            return Ok(new { 
                                message = "출고가 성공적으로 처리되었습니다.",
                                oldStock = currentStock,
                                newStock = newStock,
                                quantityShipped = request.Quantity
                            });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "제품 출고 중 오류 발생");
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        [HttpGet("receiving-history")]
        public async Task<IActionResult> GetReceivingHistory()
        {
            try
            {
                _logger.LogInformation("입고내역 조회 요청");

                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                var receivingHistory = new List<ReceivingHistoryDto>();

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT I.log_id, I.log_date, P.product_id, P.product_name, I.quantity_changed, I.current_quantity
                        FROM Products P RIGHT JOIN InventoryLogs I
                        ON P.product_id = I.product_id
                        WHERE I.change_type = '입고'";
                        

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var logId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                var logDate = reader.IsDBNull(1) ? DateTime.MinValue : reader.GetDateTime(1);
                                var productId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                                var productName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                                var quantityChanged = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                                var currentQuantity = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                                
                                receivingHistory.Add(new ReceivingHistoryDto
                                {
                                    LogId = logId,
                                    LogDate = logDate,
                                    ProductId = productId,
                                    ProductName = productName,
                                    QuantityChanged = quantityChanged,
                                    CurrentQuantity = currentQuantity
                                });
                            }
                        }
                    }
                }

                _logger.LogInformation("입고내역 조회 완료: {Count}개", receivingHistory.Count);
                return Ok(receivingHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "입고내역 조회 중 오류 발생");
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        [HttpGet("shipping-history")]
        public async Task<IActionResult> GetShippingHistory()
        {
            try
            {
                _logger.LogInformation("출고내역 조회 요청");

                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                var shippingHistory = new List<ShippingHistoryDto>();

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT I.log_id, I.log_date, P.product_id, P.product_name, I.quantity_changed, I.current_quantity
                        FROM Products P RIGHT JOIN InventoryLogs I
                        ON P.product_id = I.product_id
                        WHERE I.change_type = '출고'";
                        

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var logId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                var logDate = reader.IsDBNull(1) ? DateTime.MinValue : reader.GetDateTime(1);
                                var productId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                                var productName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                                var quantityChanged = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                                var currentQuantity = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                                
                                shippingHistory.Add(new ShippingHistoryDto
                                {
                                    LogId = logId,
                                    LogDate = logDate,
                                    ProductId = productId,
                                    ProductName = productName,
                                    QuantityChanged = quantityChanged,
                                    CurrentQuantity = currentQuantity
                                });
                            }
                        }
                    }
                }

                _logger.LogInformation("출고내역 조회 완료: {Count}개", shippingHistory.Count);
                return Ok(shippingHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "출고내역 조회 중 오류 발생");
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

    }

    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ProductUpdateRequest
    {
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ProductRegistrationRequest
    {
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ProductReceiveRequest
    {
        public int Quantity { get; set; }
    }

    public class ReceivingHistoryDto
    {
        public int LogId { get; set; }
        public DateTime LogDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantityChanged { get; set; }
        public int CurrentQuantity { get; set; }
    }

    public class ProductShipRequest
    {
        public int Quantity { get; set; }
    }

    public class ShippingHistoryDto
    {
        public int LogId { get; set; }
        public DateTime LogDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantityChanged { get; set; }
        public int CurrentQuantity { get; set; }
    }

}
