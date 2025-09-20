using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.Inventory
{
    public partial class InventoryStatusPage : UserControl
    {
        private readonly HttpClient httpClient;
        private ObservableCollection<ProductInfo> inventoryItems;

        public InventoryStatusPage()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            inventoryItems = new ObservableCollection<ProductInfo>();
            InventoryDataGrid.ItemsSource = inventoryItems;
            LoadInventoryData();
        }

        private async void LoadInventoryData()
        {
            try
            {
                var response = await httpClient.GetAsync("http://localhost:5185/api/product/inventory-status");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var inventoryDtos = JsonSerializer.Deserialize<InventoryStatusDto[]>(json, options);
                    
                    inventoryItems.Clear();
                    if (inventoryDtos != null && inventoryDtos.Length > 0)
                    {
                        foreach (var dto in inventoryDtos)
                        {
                            var status = DetermineStatus(dto.StockQuantity, dto.SafetyStock);
                            
                            inventoryItems.Add(new ProductInfo
                            {
                                ProductId = dto.ProductId,
                                ProductName = dto.ProductName ?? "이름 없음",
                                Category = dto.Category ?? "미분류",
                                Price = dto.Price,
                                StockQuantity = dto.StockQuantity,
                                SafetyStock = dto.SafetyStock,
                                LocationId = 0, // 재고 현황에서는 위치 정보가 필요 없으므로 0으로 설정
                                ImageUrl = dto.ImageUrl,
                                Status = status
                            });
                        }
                    }
                }
                else
                {
                    MessageBox.Show("재고 현황 데이터를 불러오는데 실패했습니다.", "오류", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"재고 현황 데이터 로드 중 오류가 발생했습니다: {ex.Message}", "오류", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string DetermineStatus(int stockQuantity, int safetyStock)
        {
            if (stockQuantity < safetyStock)
            {
                return "위험";
            }
            else if (stockQuantity < safetyStock * 1.2)
            {
                return "경고";
            }
            else
            {
                return "안전";
            }
        }

        private void DetailButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductInfo item)
            {
                var detailWindow = new InventoryDetailWindow(item);
                detailWindow.ShowDialog();
            }
        }
    }

    // 백엔드 DTO 클래스
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
}
