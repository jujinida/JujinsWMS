using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace jujin.Views.Inventory
{
    public partial class ShippingDetailWindow : Window
    {
        private readonly HttpClient httpClient;
        private readonly ProductInfo productInfo;
        private int selectedLocationId = 0;
        public event EventHandler ProductUpdated;

        public ShippingDetailWindow(ProductInfo product)
        {
            InitializeComponent();
            httpClient = new HttpClient();
            productInfo = product;
            LoadProductData();
        }

        private async void LoadProductData()
        {
            ProductIdTextBlock.Text = productInfo.ProductId.ToString();
            ProductNameTextBlock.Text = productInfo.ProductName;
            CategoryTextBlock.Text = productInfo.Category;
            PriceTextBlock.Text = $"{productInfo.Price:#,##0}원";
            TotalStockTextBox.Text = "0"; // 초기화
            
            // 이미지 로드
            LoadProductImage();
            
            await LoadWarehouseStock();
        }

        private void LoadProductImage()
        {
            if (!string.IsNullOrEmpty(productInfo.ImageUrl))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(productInfo.ImageUrl);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ProductImagePreview.Source = bitmap;
                    ImageUrlTextBlock.Text = "제품 이미지";
                }
                catch (Exception ex)
                {
                    ImageUrlTextBlock.Text = $"이미지 로드 실패: {ex.Message}";
                    SetDefaultImage();
                }
            }
            else
            {
                SetDefaultImage();
            }
        }

        private void SetDefaultImage()
        {
            try
            {
                var defaultBitmap = new BitmapImage();
                defaultBitmap.BeginInit();
                defaultBitmap.UriSource = new Uri("pack://application:,,,/Resources/Images/logo.jpg");
                defaultBitmap.EndInit();
                ProductImagePreview.Source = defaultBitmap;
                ImageUrlTextBlock.Text = "기본 이미지";
            }
            catch
            {
                ImageUrlTextBlock.Text = "이미지 없음";
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
                    var warehouseStocks = JsonSerializer.Deserialize<WarehouseStockDto[]>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var stockDict = warehouseStocks?.ToDictionary(ws => ws.LocationId, ws => ws.StockQuantity) ?? new Dictionary<int, int>();
                    
                    A1StockTextBlock.Text = $"재고: {stockDict.GetValueOrDefault(1, 0)}";
                    A2StockTextBlock.Text = $"재고: {stockDict.GetValueOrDefault(2, 0)}";
                    A3StockTextBlock.Text = $"재고: {stockDict.GetValueOrDefault(3, 0)}";
                    B1StockTextBlock.Text = $"재고: {stockDict.GetValueOrDefault(4, 0)}";
                    B2StockTextBlock.Text = $"재고: {stockDict.GetValueOrDefault(5, 0)}";
                    C1StockTextBlock.Text = $"재고: {stockDict.GetValueOrDefault(6, 0)}";
                    
                    int totalStock = stockDict.Values.Sum();
                    TotalStockTextBox.Text = totalStock.ToString();
                }
                else
                {
                    SetDefaultWarehouseStock();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"창고 재고량 로드 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                SetDefaultWarehouseStock();
            }
        }

        private void SetDefaultWarehouseStock()
        {
            A1StockTextBlock.Text = "재고: 0";
            A2StockTextBlock.Text = "재고: 0";
            A3StockTextBlock.Text = "재고: 0";
            B1StockTextBlock.Text = "재고: 0";
            B2StockTextBlock.Text = "재고: 0";
            C1StockTextBlock.Text = "재고: 0";
            TotalStockTextBox.Text = "0";
        }

        private void WarehouseBorder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                ResetWarehouseBorderColors();
                border.Background = System.Windows.Media.Brushes.White;
                border.BorderBrush = System.Windows.Media.Brushes.Blue;
                border.BorderThickness = new System.Windows.Thickness(2);
                selectedLocationId = int.Parse(border.Tag.ToString());
                var warehouseName = GetWarehouseName(selectedLocationId);
                MessageBox.Show($"{warehouseName} 창고가 선택되었습니다.", "창고 선택", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ResetWarehouseBorderColors()
        {
            var defaultBackground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8F9FA"));
            var defaultBorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#D8DEE9"));
            
            A1WarehouseBorder.Background = defaultBackground;
            A1WarehouseBorder.BorderBrush = defaultBorderBrush;
            A1WarehouseBorder.BorderThickness = new System.Windows.Thickness(1);
            
            A2WarehouseBorder.Background = defaultBackground;
            A2WarehouseBorder.BorderBrush = defaultBorderBrush;
            A2WarehouseBorder.BorderThickness = new System.Windows.Thickness(1);
            
            A3WarehouseBorder.Background = defaultBackground;
            A3WarehouseBorder.BorderBrush = defaultBorderBrush;
            A3WarehouseBorder.BorderThickness = new System.Windows.Thickness(1);
            
            B1WarehouseBorder.Background = defaultBackground;
            B1WarehouseBorder.BorderBrush = defaultBorderBrush;
            B1WarehouseBorder.BorderThickness = new System.Windows.Thickness(1);
            
            B2WarehouseBorder.Background = defaultBackground;
            B2WarehouseBorder.BorderBrush = defaultBorderBrush;
            B2WarehouseBorder.BorderThickness = new System.Windows.Thickness(1);
            
            C1WarehouseBorder.Background = defaultBackground;
            C1WarehouseBorder.BorderBrush = defaultBorderBrush;
            C1WarehouseBorder.BorderThickness = new System.Windows.Thickness(1);
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

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 입력값 검증
                if (!int.TryParse(ShippingQuantityTextBox.Text, out int shippingQuantity))
                {
                    MessageBox.Show("올바른 출고량을 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (shippingQuantity <= 0)
                {
                    MessageBox.Show("출고량은 0보다 커야 합니다.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 창고 선택 검증
                if (selectedLocationId == 0)
                {
                    MessageBox.Show("출고할 창고를 선택해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 현재 총 재고량 확인
                if (!int.TryParse(TotalStockTextBox.Text, out int currentTotalStock))
                {
                    MessageBox.Show("재고량 정보를 불러올 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (shippingQuantity > currentTotalStock)
                {
                    MessageBox.Show($"재고가 부족합니다.\n현재 총 재고: {currentTotalStock}개\n요청 출고량: {shippingQuantity}개", 
                                    "재고 부족", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var warehouseName = GetWarehouseName(selectedLocationId);

                // 확인 대화상자
                var result = MessageBox.Show($"정말로 {shippingQuantity}개를 {warehouseName} 창고에서 출고하시겠습니까?\n\n" +
                                           $"품목: {productInfo.ProductName}\n" +
                                           $"현재 총 재고: {currentTotalStock}개\n" +
                                           $"출고 후 총 재고: {currentTotalStock - shippingQuantity}개", 
                                           "출고 확인", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }

                // 출고 처리 버튼 비활성화
                ConfirmButton.IsEnabled = false;
                ConfirmButton.Content = "처리 중...";

                // 출고 요청
                var shipRequest = new
                {
                    Quantity = shippingQuantity,
                    LocationId = selectedLocationId
                };

                var json = JsonSerializer.Serialize(shipRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"http://localhost:5185/api/product/ship-location/{productInfo.ProductId}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    var message = responseData.GetProperty("message").GetString();
                    var oldStock = responseData.GetProperty("oldStock").GetInt32();
                    var newStock = responseData.GetProperty("newStock").GetInt32();
                    var quantityShipped = responseData.GetProperty("quantityShipped").GetInt32();

                    MessageBox.Show($"출고가 성공적으로 처리되었습니다!\n\n" +
                                  $"품목: {productInfo.ProductName}\n" +
                                  $"출고량: {quantityShipped}개\n" +
                                  $"이전 재고: {oldStock}개\n" +
                                  $"현재 재고: {newStock}개", 
                                  "출고 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // 창고 재고량 새로고침
                    await LoadWarehouseStock();
                    ProductUpdated?.Invoke(this, EventArgs.Empty);
                    // this.Close(); // 창고를 유지하여 추가 출고 가능
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorData = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    var errorMessage = errorData.GetProperty("message").GetString();
                    
                    MessageBox.Show($"출고 처리에 실패했습니다: {errorMessage}", "출고 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"출고 처리 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 출고 처리 버튼 다시 활성화
                ConfirmButton.IsEnabled = true;
                ConfirmButton.Content = "확인";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
