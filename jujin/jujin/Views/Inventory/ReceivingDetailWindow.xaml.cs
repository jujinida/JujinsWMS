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
    public partial class ReceivingDetailWindow : Window
    {
        private readonly HttpClient httpClient;
        private readonly ProductInfo productInfo;
        private int selectedLocationId = 0; // 선택된 창고 ID
        public event EventHandler ProductUpdated;

        public ReceivingDetailWindow(ProductInfo product)
        {
            InitializeComponent();
            httpClient = new HttpClient();
            productInfo = product;
            LoadProductData();
            ReceiveQuantityTextBox.TextChanged += ReceiveQuantityTextBox_TextChanged;
        }

        private async void LoadProductData()
        {
            // 제품 정보 로드 (읽기 전용)
            ProductIdTextBox.Text = productInfo.ProductId.ToString();
            ProductNameTextBox.Text = productInfo.ProductName;
            CategoryTextBox.Text = productInfo.Category ?? "미분류";
            PriceTextBox.Text = productInfo.Price.ToString("#,##0원");

            // 이미지 로드
            LoadProductImage();
            
            // 창고별 재고량 로드
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
                }
                catch (Exception ex)
                {
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
            }
            catch
            {
                // 기본 이미지 로드 실패 시 무시
            }
        }

        private void ReceiveQuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateNewStock();
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

                    // 창고별 재고량 표시
                    var stockDict = warehouseStocks?.ToDictionary(ws => ws.LocationId, ws => ws.StockQuantity) ?? new Dictionary<int, int>();
                    
                    A1StockTextBlock.Text = $"재고: {stockDict.GetValueOrDefault(1, 0)}";
                    A2StockTextBlock.Text = $"재고: {stockDict.GetValueOrDefault(2, 0)}";
                    A3StockTextBlock.Text = $"재고: {stockDict.GetValueOrDefault(3, 0)}";
                    B1StockTextBlock.Text = $"재고: {stockDict.GetValueOrDefault(4, 0)}";
                    B2StockTextBlock.Text = $"재고: {stockDict.GetValueOrDefault(5, 0)}";
                    C1StockTextBlock.Text = $"재고: {stockDict.GetValueOrDefault(6, 0)}";
                    
                    // 총 재고량 계산
                    int totalStock = stockDict.Values.Sum();
                    TotalStockTextBox.Text = totalStock.ToString();
                }
                else
                {
                    // 기본값 설정
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

        private void UpdateNewStock()
        {
            if (int.TryParse(ReceiveQuantityTextBox.Text, out int receiveQuantity))
            {
                if (int.TryParse(TotalStockTextBox.Text, out int currentTotalStock))
                {
                    int newStock = currentTotalStock + receiveQuantity;
                    NewStockTextBox.Text = newStock.ToString();
                }
            }
            else
            {
                NewStockTextBox.Text = TotalStockTextBox.Text;
            }
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 입력값 검증
                if (!int.TryParse(ReceiveQuantityTextBox.Text, out int receiveQuantity))
                {
                    MessageBox.Show("올바른 입고량을 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (receiveQuantity <= 0)
                {
                    MessageBox.Show("입고량은 0보다 커야 합니다.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (selectedLocationId == 0)
                {
                    MessageBox.Show("입고할 창고를 선택해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 확인 버튼 비활성화
                ConfirmButton.IsEnabled = false;
                ConfirmButton.Content = "처리 중...";

                // 새로운 입고 처리 요청 (Product_Locations 테이블 기반)
                var receiveRequest = new
                {
                    Quantity = receiveQuantity,
                    LocationId = selectedLocationId
                };

                var json = JsonSerializer.Serialize(receiveRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"http://localhost:5185/api/product/receive-location/{productInfo.ProductId}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ReceiveResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    MessageBox.Show($"입고가 성공적으로 처리되었습니다!\n\n" +
                                  $"입고량: {result.QuantityReceived}개\n" +
                                  $"이전 재고: {result.OldStock}개\n" +
                                  $"현재 재고: {result.NewStock}개", 
                                  "입고 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // 창고별 재고량 다시 로드
                    await LoadWarehouseStock();
                    
                    ProductUpdated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"입고 처리에 실패했습니다: {errorContent}", "입고 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"입고 처리 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 확인 버튼 다시 활성화
                ConfirmButton.IsEnabled = true;
                ConfirmButton.Content = "입고 확인";
            }
        }

        private void WarehouseBorder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                // 모든 창고 Border의 배경색을 기본색으로 리셋
                ResetWarehouseBorderColors();
                
                // 선택된 창고 Border의 배경색을 하얀색으로 변경
                border.Background = System.Windows.Media.Brushes.White;
                border.BorderBrush = System.Windows.Media.Brushes.Blue;
                border.BorderThickness = new System.Windows.Thickness(2);
                
                // 선택된 창고 ID 저장
                selectedLocationId = int.Parse(border.Tag.ToString());
                
                // 선택된 창고명 표시
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class ReceiveResponse
    {
        public string Message { get; set; }
        public int OldStock { get; set; }
        public int NewStock { get; set; }
        public int QuantityReceived { get; set; }
    }

    public class WarehouseStockDto
    {
        public int LocationId { get; set; }
        public int StockQuantity { get; set; }
    }
}
