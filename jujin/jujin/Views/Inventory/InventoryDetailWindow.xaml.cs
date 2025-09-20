using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace jujin.Views.Inventory
{
    public partial class InventoryDetailWindow : Window
    {
        private readonly HttpClient httpClient;
        private readonly ProductInfo productInfo;
        private ObservableCollection<WarehouseStockInfo> warehouseStocks;

        public InventoryDetailWindow(ProductInfo item)
        {
            InitializeComponent();
            httpClient = new HttpClient();
            productInfo = item;
            warehouseStocks = new ObservableCollection<WarehouseStockInfo>();
            WarehouseDataGrid.ItemsSource = warehouseStocks;
            
            LoadProductData();
            LoadWarehouseStock();
        }

        private void LoadProductData()
        {
            ProductIdTextBlock.Text = productInfo.ProductId.ToString();
            ProductNameTextBlock.Text = productInfo.ProductName;
            CategoryTextBlock.Text = productInfo.Category;
            SafetyStockTextBlock.Text = productInfo.SafetyStock.ToString();
            CurrentStockTextBlock.Text = productInfo.StockQuantity.ToString();
            
            // 상태 계산
            var status = DetermineStatus(productInfo.StockQuantity, productInfo.SafetyStock);
            StatusTextBlock.Text = status;
            
            // 상태에 따른 색상 설정
            StatusTextBlock.Foreground = status switch
            {
                "위험" => System.Windows.Media.Brushes.Red,
                "경고" => System.Windows.Media.Brushes.Orange,
                "안전" => System.Windows.Media.Brushes.Green,
                _ => System.Windows.Media.Brushes.Black
            };
            
            LoadProductImage();
        }

        private void LoadProductImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(productInfo.ImageUrl))
                {
                    ProductImage.Source = new BitmapImage(new Uri(productInfo.ImageUrl));
                }
                else
                {
                    // 이미지 URL이 없는 경우 기본 이미지 표시
                    ProductImage.Source = null;
                }
            }
            catch
            {
                // 이미지 로드 실패 시 기본 이미지 표시
                ProductImage.Source = null;
            }
        }

        private async Task LoadWarehouseStock()
        {
            try
            {
                var response = await httpClient.GetAsync($"http://localhost:5185/api/product/warehouse-stock/{productInfo.ProductId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var warehouseStocksData = JsonSerializer.Deserialize<WarehouseStockDto[]>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    warehouseStocks.Clear();
                    if (warehouseStocksData != null)
                    {
                        foreach (var ws in warehouseStocksData)
                        {
                            var warehouseName = GetWarehouseName(ws.LocationId);
                            var status = DetermineWarehouseStatus(ws.StockQuantity, productInfo.SafetyStock);
                            
                            warehouseStocks.Add(new WarehouseStockInfo
                            {
                                WarehouseName = warehouseName,
                                StockQuantity = ws.StockQuantity,
                                Status = status
                            });
                        }
                    }
                }
                else
                {
                    // 창고별 재고 정보가 없을 경우 기본 창고 정보 표시
                    LoadDefaultWarehouseInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"창고별 재고 정보를 불러오는데 실패했습니다: {ex.Message}", "오류", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                LoadDefaultWarehouseInfo();
            }
        }

        private void LoadDefaultWarehouseInfo()
        {
            warehouseStocks.Clear();
            var warehouses = new[] { "A-1", "A-2", "A-3", "B-1", "B-2", "C-1" };
            
            for (int i = 0; i < warehouses.Length; i++)
            {
                warehouseStocks.Add(new WarehouseStockInfo
                {
                    WarehouseName = warehouses[i],
                    StockQuantity = 0,
                    Status = "재고 없음"
                });
            }
        }

        private string GetWarehouseName(int locationId)
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

        private string DetermineWarehouseStatus(int stockQuantity, int safetyStock)
        {
            if (stockQuantity == 0)
            {
                return "재고 없음";
            }
            else if (stockQuantity < safetyStock)
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class WarehouseStockInfo
    {
        public string WarehouseName { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; }
    }
}
