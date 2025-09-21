using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Amazon.S3;
using Amazon.S3.Model;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAmazonS3 _s3Client;
        private readonly ApplicationDbContext _context;

        public ProductController(ILogger<ProductController> logger, IConfiguration configuration, ApplicationDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
            
            // AWS S3 클라이언트 설정
            var s3Config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2,
                ServiceURL = "https://s3.ap-southeast-2.amazonaws.com"
            };

            var accessKey = _configuration["AWS:AccessKey"];
            var secretKey = _configuration["AWS:SecretKey"];
            
            if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("AWS 자격증명이 설정되지 않았습니다. appsettings.json을 확인하세요.");
            }

            _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
        }


        [HttpGet("list")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                _logger.LogInformation("제품 목록 조회 요청");

                var productData = await _context.Products
                    .Include(p => p.ProductLocations)
                    .OrderBy(p => p.ProductId)
                    .Select(p => new
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        Category = p.Category,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity,
                        SafetyStock = p.SafetyStock,
                        ImageUrl = p.ImageUrl,
                        ProductLocations = p.ProductLocations
                    })
                    .ToListAsync();

                var products = productData.SelectMany(p => p.ProductLocations.DefaultIfEmpty(),
                    (p, pl) => new ProductDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        Category = p.Category,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity,
                        SafetyStock = p.SafetyStock,
                        LocationId = pl != null ? pl.LocationId : 0,
                        LocationName = pl != null ? GetLocationName(pl.LocationId) : "미지정",
                        ImageUrl = p.ImageUrl
                    }).ToList();

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

                var product = new Product
                {
                    ProductName = request.ProductName,
                    Category = request.Category,
                    Price = request.Price,
                    StockQuantity = request.StockQuantity,
                    SafetyStock = request.SafetyStock,
                    ImageUrl = request.ImageUrl,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Product_Locations 테이블에 데이터 삽입
                var productLocation = new ProductLocation
                {
                    ProductId = product.ProductId,
                    LocationId = request.LocationId,
                    StockQuantity = request.StockQuantity
                };

                _context.ProductLocations.Add(productLocation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("제품 등록 성공: {ProductName}, ProductId={ProductId}", request.ProductName, product.ProductId);
                return Ok(new { message = "제품이 성공적으로 등록되었습니다." });
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

                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "제품을 찾을 수 없습니다." });
                }

                // 엔티티 업데이트
                product.ProductName = request.ProductName;
                product.Category = request.Category;
                product.Price = request.Price;
                product.StockQuantity = request.StockQuantity;
                product.SafetyStock = request.SafetyStock;
                product.ImageUrl = request.ImageUrl;
                product.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

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

        [HttpPost("ship-location/{id}")]
        public async Task<IActionResult> ShipProductLocation(int id, [FromBody] ProductShipRequest request)
        {
            try
            {
                _logger.LogInformation("제품 출고 요청 (Location 기반): ProductId={ProductId}, Quantity={Quantity}, LocationId={LocationId}", 
                    id, request.Quantity, request.LocationId);

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
                            // 현재 총 재고량 조회 (Product_Locations 테이블에서)
                            string selectQuery = @"
                                SELECT SUM(stock_quantity) as total_quantity
                                FROM Product_Locations
                                WHERE product_id = @ProductId";
                            
                            int currentTotalStock = 0;
                            using (var selectCmd = new SqlCommand(selectQuery, conn, transaction))
                            {
                                selectCmd.Parameters.AddWithValue("@ProductId", id);
                                var result = await selectCmd.ExecuteScalarAsync();
                                
                                _logger.LogInformation("총 재고량 조회 결과: ProductId={ProductId}, Result={Result}", id, result);
                                
                                if (result != null && result != DBNull.Value)
                                {
                                    currentTotalStock = Convert.ToInt32(result);
                                    _logger.LogInformation("현재 총 재고량: {CurrentTotalStock}", currentTotalStock);
                                }
                                else
                                {
                                    _logger.LogWarning("총 재고량 조회 결과가 NULL입니다. ProductId={ProductId}", id);
                                }
                            }

                            // 선택된 창고의 현재 재고량 조회
                            string locationQuery = @"
                                SELECT stock_quantity
                                FROM Product_Locations
                                WHERE product_id = @ProductId AND location_id = @LocationId";
                            
                            int targetLocationId = request.LocationId;
                            int currentLocationStock = 0;
                            bool locationExists = false;
                            
                            using (var locationCmd = new SqlCommand(locationQuery, conn, transaction))
                            {
                                locationCmd.Parameters.AddWithValue("@ProductId", id);
                                locationCmd.Parameters.AddWithValue("@LocationId", request.LocationId);
                                
                                var result = await locationCmd.ExecuteScalarAsync();
                                _logger.LogInformation("창고별 재고량 조회 결과: ProductId={ProductId}, LocationId={LocationId}, Result={Result}", 
                                    id, request.LocationId, result);
                                
                                if (result != null && result != DBNull.Value)
                                {
                                    currentLocationStock = Convert.ToInt32(result);
                                    locationExists = true;
                                    _logger.LogInformation("현재 창고 재고량: {CurrentLocationStock}", currentLocationStock);
                                }
                                else
                                {
                                    _logger.LogWarning("창고 재고량 조회 결과가 NULL입니다. ProductId={ProductId}, LocationId={LocationId} - 출고 불가", 
                                        id, request.LocationId);
                                    return BadRequest(new { message = "해당 창고에 재고가 없습니다." });
                                }
                            }

                            // 재고량이 부족한지 확인 (선택된 창고 기준)
                            if (currentLocationStock < request.Quantity)
                            {
                                return BadRequest(new { message = $"해당 창고의 재고가 부족합니다. 현재 재고: {currentLocationStock}개, 요청 출고량: {request.Quantity}개" });
                            }

                            // 해당 창고의 재고량 업데이트 (출고량만큼 차감)
                            int newLocationStock = currentLocationStock - request.Quantity;
                            
                            string updateQuery = @"
                                UPDATE Product_Locations 
                                SET stock_quantity = @NewStock, updated_at = GETDATE()
                                WHERE product_id = @ProductId AND location_id = @LocationId";

                            _logger.LogInformation("창고 재고량 출고 업데이트: ProductId={ProductId}, LocationId={LocationId}, OldStock={OldStock}, NewStock={NewStock}, Quantity={Quantity}", 
                                id, targetLocationId, currentLocationStock, newLocationStock, request.Quantity);

                            using (var updateCmd = new SqlCommand(updateQuery, conn, transaction))
                            {
                                updateCmd.Parameters.AddWithValue("@ProductId", id);
                                updateCmd.Parameters.AddWithValue("@LocationId", targetLocationId);
                                updateCmd.Parameters.AddWithValue("@NewStock", newLocationStock);
                                
                                int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                                _logger.LogInformation("창고 재고량 출고 업데이트 결과: RowsAffected={RowsAffected}", rowsAffected);
                                
                                if (rowsAffected == 0)
                                {
                                    _logger.LogError("창고 재고량 출고 업데이트 실패: ProductId={ProductId}, LocationId={LocationId}", id, targetLocationId);
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
                                logCmd.Parameters.AddWithValue("@CurrentQuantity", currentTotalStock - request.Quantity);
                                
                                await logCmd.ExecuteNonQueryAsync();
                                _logger.LogInformation("재고 로그 추가 완료: ProductId={ProductId}, ChangeType=출고, QuantityChanged={QuantityChanged}, CurrentQuantity={CurrentQuantity}", 
                                    id, request.Quantity, currentTotalStock - request.Quantity);
                            }

                            // 트랜잭션 커밋
                            transaction.Commit();
                            
                            _logger.LogInformation("제품 출고 완료 (Location 기반): ProductId={ProductId}, OldStock={OldStock}, NewStock={NewStock}", 
                                id, currentTotalStock, currentTotalStock - request.Quantity);
                            
                            return Ok(new { 
                                message = "출고가 성공적으로 처리되었습니다.",
                                oldStock = currentTotalStock,
                                newStock = currentTotalStock - request.Quantity,
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
                _logger.LogError(ex, "제품 출고 중 오류 발생 (Location 기반)");
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

        [HttpGet("warehouse-stock/{productId}")]
        public async Task<IActionResult> GetWarehouseStock(int productId)
        {
            try
            {
                _logger.LogInformation("창고별 재고량 조회 요청: ProductId={ProductId}", productId);

                var warehouseStocks = await _context.ProductLocations
                    .Where(pl => pl.ProductId == productId)
                    .Select(pl => new WarehouseStockDto
                    {
                        LocationId = pl.LocationId,
                        StockQuantity = pl.StockQuantity
                    })
                    .ToListAsync();

                _logger.LogInformation("창고별 재고량 조회 완료: {Count}개", warehouseStocks.Count);
                return Ok(warehouseStocks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "창고별 재고량 조회 중 오류 발생");
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        [HttpPost("receive-location/{id}")]
        public async Task<IActionResult> ReceiveProductLocation(int id, [FromBody] ProductReceiveRequest request)
        {
            try
            {
                _logger.LogInformation("제품 입고 요청 (Location 기반): ProductId={ProductId}, Quantity={Quantity}", id, request.Quantity);

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
                            // 현재 총 재고량 조회 (Product_Locations 테이블에서)
                            string selectQuery = @"
                                SELECT SUM(stock_quantity) as total_quantity
                                FROM Product_Locations
                                WHERE product_id = @ProductId";
                            
                            int currentTotalStock = 0;
                            using (var selectCmd = new SqlCommand(selectQuery, conn, transaction))
                            {
                                selectCmd.Parameters.AddWithValue("@ProductId", id);
                                var result = await selectCmd.ExecuteScalarAsync();
                                
                                _logger.LogInformation("총 재고량 조회 결과: ProductId={ProductId}, Result={Result}", id, result);
                                
                                if (result != null && result != DBNull.Value)
                                {
                                    currentTotalStock = Convert.ToInt32(result);
                                    _logger.LogInformation("현재 총 재고량: {CurrentTotalStock}", currentTotalStock);
                                }
                                else
                                {
                                    _logger.LogWarning("총 재고량 조회 결과가 NULL입니다. ProductId={ProductId}", id);
                                }
                            }

                            // 선택된 창고의 현재 재고량 조회
                            string locationQuery = @"
                                SELECT stock_quantity
                                FROM Product_Locations
                                WHERE product_id = @ProductId AND location_id = @LocationId";
                            
                            int targetLocationId = request.LocationId;
                            int currentLocationStock = 0;
                            bool locationExists = false;
                            
                            using (var locationCmd = new SqlCommand(locationQuery, conn, transaction))
                            {
                                locationCmd.Parameters.AddWithValue("@ProductId", id);
                                locationCmd.Parameters.AddWithValue("@LocationId", request.LocationId);
                                
                                var result = await locationCmd.ExecuteScalarAsync();
                                _logger.LogInformation("창고별 재고량 조회 결과: ProductId={ProductId}, LocationId={LocationId}, Result={Result}", 
                                    id, request.LocationId, result);
                                
                                if (result != null && result != DBNull.Value)
                                {
                                    currentLocationStock = Convert.ToInt32(result);
                                    locationExists = true;
                                    _logger.LogInformation("현재 창고 재고량: {CurrentLocationStock}", currentLocationStock);
                                }
                                else
                                {
                                    _logger.LogWarning("창고 재고량 조회 결과가 NULL입니다. ProductId={ProductId}, LocationId={LocationId} - 새 레코드 생성 예정", 
                                        id, request.LocationId);
                                }
                            }

                            // 해당 창고의 재고량 업데이트 또는 삽입
                            int newLocationStock = currentLocationStock + request.Quantity;
                            
                            if (locationExists)
                            {
                                // 기존 레코드가 있으면 UPDATE
                                string updateQuery = @"
                                    UPDATE Product_Locations 
                                    SET stock_quantity = @NewStock, updated_at = GETDATE()
                                    WHERE product_id = @ProductId AND location_id = @LocationId";

                                _logger.LogInformation("창고 재고량 업데이트: ProductId={ProductId}, LocationId={LocationId}, OldStock={OldStock}, NewStock={NewStock}, Quantity={Quantity}", 
                                    id, targetLocationId, currentLocationStock, newLocationStock, request.Quantity);

                                using (var updateCmd = new SqlCommand(updateQuery, conn, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@ProductId", id);
                                    updateCmd.Parameters.AddWithValue("@LocationId", targetLocationId);
                                    updateCmd.Parameters.AddWithValue("@NewStock", newLocationStock);
                                    
                                    int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                                    _logger.LogInformation("창고 재고량 업데이트 결과: RowsAffected={RowsAffected}", rowsAffected);
                                    
                                    if (rowsAffected == 0)
                                    {
                                        _logger.LogError("창고 재고량 업데이트 실패: ProductId={ProductId}, LocationId={LocationId}", id, targetLocationId);
                                        return NotFound(new { message = "제품을 찾을 수 없습니다." });
                                    }
                                }
                            }
                            else
                            {
                                // 기존 레코드가 없으면 INSERT
                                string insertQuery = @"
                                    INSERT INTO Product_Locations (product_id, location_id, stock_quantity)
                                    VALUES (@ProductId, @LocationId, @NewStock)";

                                _logger.LogInformation("창고 재고량 신규 생성: ProductId={ProductId}, LocationId={LocationId}, NewStock={NewStock}, Quantity={Quantity}", 
                                    id, targetLocationId, newLocationStock, request.Quantity);

                                using (var insertCmd = new SqlCommand(insertQuery, conn, transaction))
                                {
                                    insertCmd.Parameters.AddWithValue("@ProductId", id);
                                    insertCmd.Parameters.AddWithValue("@LocationId", targetLocationId);
                                    insertCmd.Parameters.AddWithValue("@NewStock", newLocationStock);
                                    
                                    int rowsAffected = await insertCmd.ExecuteNonQueryAsync();
                                    _logger.LogInformation("창고 재고량 신규 생성 결과: RowsAffected={RowsAffected}", rowsAffected);
                                    
                                    if (rowsAffected == 0)
                                    {
                                        _logger.LogError("창고 재고량 신규 생성 실패: ProductId={ProductId}, LocationId={LocationId}", id, targetLocationId);
                                        return StatusCode(500, new { message = "창고 재고량 생성에 실패했습니다." });
                                    }
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
                                logCmd.Parameters.AddWithValue("@CurrentQuantity", currentTotalStock + request.Quantity);
                                
                                await logCmd.ExecuteNonQueryAsync();
                                _logger.LogInformation("재고 로그 추가 완료: ProductId={ProductId}, ChangeType=입고, QuantityChanged={QuantityChanged}, CurrentQuantity={CurrentQuantity}", 
                                    id, request.Quantity, currentTotalStock + request.Quantity);
                            }

                            // 트랜잭션 커밋
                            transaction.Commit();
                            
                            _logger.LogInformation("제품 입고 완료 (Location 기반): ProductId={ProductId}, OldStock={OldStock}, NewStock={NewStock}", 
                                id, currentTotalStock, currentTotalStock + request.Quantity);
                            
                            return Ok(new { 
                                message = "입고가 성공적으로 처리되었습니다.",
                                oldStock = currentTotalStock,
                                newStock = currentTotalStock + request.Quantity,
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
                _logger.LogError(ex, "제품 입고 중 오류 발생 (Location 기반)");
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        private static string GetLocationName(int locationId)
        {
            return locationId switch
            {
                1 => "A-1",
                2 => "A-2",
                3 => "A-3",
                4 => "B-1",
                5 => "B-2",
                6 => "C-1",
                _ => "미지정"
            };
        }

        [HttpGet("inventory-status")]
        public async Task<IActionResult> GetInventoryStatus()
        {
            try
            {
                _logger.LogInformation("재고 현황 조회 요청");

                var inventoryStatusList = await _context.Products
                    .OrderBy(p => p.ProductId)
                    .Select(p => new InventoryStatusDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        SafetyStock = p.SafetyStock,
                        StockQuantity = p.StockQuantity,
                        Category = p.Category,
                        Price = p.Price,
                        ImageUrl = p.ImageUrl
                    })
                    .ToListAsync();

                _logger.LogInformation("재고 현황 조회 완료: {Count}개 항목", inventoryStatusList.Count);

                return Ok(inventoryStatusList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "재고 현황 조회 중 오류 발생");
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        [HttpGet("inventory-statistics")]
        public async Task<IActionResult> GetInventoryStatistics([FromQuery] string? productName = null, [FromQuery] string? category = null)
        {
            try
            {
                _logger.LogInformation("재고 통계 조회 요청: ProductName={ProductName}, Category={Category}", productName, category);

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string query = @"
                        WITH MonthlySales AS (
                            -- 1. 각 품목별 9월 월간 판매량 및 매출액 계산
                            SELECT
                                product_id,
                                SUM(quantity) AS monthly_sales_volume, -- 월 매출량
                                SUM(quantity * sale_price) AS total_monthly_revenue -- 총 월 매출액 (재고 회전율 계산에 사용)
                            FROM
                                Sales
                            WHERE
                                sale_date BETWEEN '2025-09-01' AND '2025-09-30'
                            GROUP BY
                                product_id
                        ),
                        AverageInventory AS (
                            -- 2. 각 품목별 8월 말, 9월 말 재고액을 이용한 평균 재고액 계산
                            SELECT
                                product_id,
                                AVG(end_of_period_value) AS average_inventory_value -- 평균 재고액
                            FROM
                                InventorySummary
                            WHERE
                                period_date IN ('2025-08-31', '2025-09-30')
                            GROUP BY
                                product_id
                        )
                        -- 3. 모든 CTE와 Products 테이블을 JOIN하여 최종 지표 계산
                        SELECT
                            p.product_id AS 품목코드,
                            p.product_name AS 품목명,
                            p.category AS 카테고리,
                            -- 재고 회전율: 총 매출액 / 평균 재고액
                            -- 재고액은 제품 가격으로 환산되어 있으므로 매출액을 사용합니다.
                            (ms.total_monthly_revenue / ai.average_inventory_value) AS 재고_회전율,
                            -- 재고 소진 지수: 월 매출량 / 안전재고량
                            -- 1 이상이면 안전재고량을 초과하여 판매된 것이므로, 재고 부족 가능성이 높다는 의미
                            CAST(ms.monthly_sales_volume AS DECIMAL(18, 2)) / p.safety_stock AS 재고_소진_지수,
                            ms.monthly_sales_volume AS 월매출량
                        FROM
                            Products AS p
                        JOIN
                            MonthlySales AS ms ON p.product_id = ms.product_id
                        JOIN
                            AverageInventory AS ai ON p.product_id = ai.product_id
                        WHERE
                            1=1";

                    var parameters = new List<SqlParameter>();

                    // 품목명 검색 조건 추가
                    if (!string.IsNullOrEmpty(productName))
                    {
                        query += " AND p.product_name LIKE @productName";
                        parameters.Add(new SqlParameter("@productName", $"%{productName}%"));
                    }

                    // 카테고리 검색 조건 추가
                    if (!string.IsNullOrEmpty(category) && category != "전체")
                    {
                        query += " AND p.category = @category";
                        parameters.Add(new SqlParameter("@category", category));
                    }

                    query += " ORDER BY p.product_id";

                    var statisticsList = new List<InventoryStatisticsDto>();

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        // 매개변수 추가
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.Add(param);
                        }

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                statisticsList.Add(new InventoryStatisticsDto
                                {
                                    ProductId = reader.GetInt32(0),
                                    ProductName = reader.GetString(1),
                                    Category = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    InventoryTurnoverRate = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                                    InventoryDepletionIndex = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                                    MonthlySalesVolume = reader.GetInt32(5)
                                });
                            }
                        }
                    }

                    _logger.LogInformation("재고 통계 조회 완료: {Count}개 항목", statisticsList.Count);

                    return Ok(statisticsList);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "재고 통계 조회 중 오류 발생");
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }

        [HttpGet("sales-trend/{productId}")]
        public async Task<IActionResult> GetSalesTrend(int productId)
        {
            try
            {
                _logger.LogInformation("제품 판매 추이 조회 요청: ProductId={ProductId}", productId);

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string query = @"
                        SELECT 
                            DATEPART(DAY, sale_date) AS DayOfMonth,
                            SUM(quantity) AS DailySales
                        FROM Sales 
                        WHERE product_id = @productId 
                            AND sale_date >= DATEADD(MONTH, -1, GETDATE())
                            AND sale_date < GETDATE()
                        GROUP BY DATEPART(DAY, sale_date)
                        ORDER BY DayOfMonth";

                    var salesTrendList = new List<SalesTrendDto>();

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@productId", productId);
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                salesTrendList.Add(new SalesTrendDto
                                {
                                    DayOfMonth = reader.GetInt32(0),
                                    DailySales = reader.GetInt32(1)
                                });
                            }
                        }
                    }

                    _logger.LogInformation("제품 판매 추이 조회 완료: ProductId={ProductId}, {Count}개 일자", productId, salesTrendList.Count);

                    return Ok(salesTrendList);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "제품 판매 추이 조회 중 오류 발생: ProductId={ProductId}", productId);
                return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
            }
        }
}

public class WarehouseStockDto
{
    public int LocationId { get; set; }
    public int StockQuantity { get; set; }
}

public class InventoryStatusDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string Category { get; set; }
    public int SafetyStock { get; set; }
    public int StockQuantity { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
}

public class InventoryStatisticsDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string Category { get; set; }
    public decimal InventoryTurnoverRate { get; set; }
    public decimal InventoryDepletionIndex { get; set; }
    public int MonthlySalesVolume { get; set; }
}

public class SalesTrendDto
{
    public int DayOfMonth { get; set; }
    public int DailySales { get; set; }
}

public class ProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int SafetyStock { get; set; }
    public int LocationId { get; set; }
    public string LocationName { get; set; }
    public string ImageUrl { get; set; }
}

public class ProductUpdateRequest
{
    public string ProductName { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int SafetyStock { get; set; }
    public string ImageUrl { get; set; }
}

public class ProductRegistrationRequest
{
    public string ProductName { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int SafetyStock { get; set; }
    public int LocationId { get; set; }
    public string ImageUrl { get; set; }
}

public class ProductReceiveRequest
{
    public int Quantity { get; set; }
    public int LocationId { get; set; }
}

public class ProductShipRequest
{
    public int Quantity { get; set; }
    public int LocationId { get; set; }
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
