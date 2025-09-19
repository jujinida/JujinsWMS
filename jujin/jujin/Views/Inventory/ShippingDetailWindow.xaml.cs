using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace jujin.Views.Inventory
{
    public partial class ShippingDetailWindow : Window
    {
        private readonly HttpClient httpClient;
        private readonly ProductInfo productInfo;
        public event EventHandler ProductUpdated;

        public ShippingDetailWindow(ProductInfo product)
        {
            InitializeComponent();
            httpClient = new HttpClient();
            productInfo = product;
            LoadProductData();
        }

        private void LoadProductData()
        {
            ProductIdTextBlock.Text = productInfo.ProductId.ToString();
            ProductNameTextBlock.Text = productInfo.ProductName;
            CategoryTextBlock.Text = productInfo.Category;
            PriceTextBlock.Text = $"{productInfo.Price:#,##0}원";
            CurrentStockTextBlock.Text = $"{productInfo.StockQuantity}개";
            
            // 이미지 로드
            LoadProductImage();
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

                if (shippingQuantity > productInfo.StockQuantity)
                {
                    MessageBox.Show($"재고가 부족합니다.\n현재 재고: {productInfo.StockQuantity}개\n요청 출고량: {shippingQuantity}개", 
                                    "재고 부족", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 확인 대화상자
                var result = MessageBox.Show($"정말로 {shippingQuantity}개를 출고하시겠습니까?\n\n" +
                                           $"품목: {productInfo.ProductName}\n" +
                                           $"현재 재고: {productInfo.StockQuantity}개\n" +
                                           $"출고 후 재고: {productInfo.StockQuantity - shippingQuantity}개", 
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
                    Quantity = shippingQuantity
                };

                var json = JsonSerializer.Serialize(shipRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"http://localhost:5185/api/product/ship/{productInfo.ProductId}", content);
                
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
                    
                    ProductUpdated?.Invoke(this, EventArgs.Empty);
                    this.Close();
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
