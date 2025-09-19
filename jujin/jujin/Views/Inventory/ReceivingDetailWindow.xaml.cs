using System;
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
        public event EventHandler ProductUpdated;

        public ReceivingDetailWindow(ProductInfo product)
        {
            InitializeComponent();
            httpClient = new HttpClient();
            productInfo = product;
            LoadProductData();
            ReceiveQuantityTextBox.TextChanged += ReceiveQuantityTextBox_TextChanged;
        }

        private void LoadProductData()
        {
            // 제품 정보 로드 (읽기 전용)
            ProductIdTextBox.Text = productInfo.ProductId.ToString();
            ProductNameTextBox.Text = productInfo.ProductName;
            CategoryTextBox.Text = productInfo.Category ?? "미분류";
            PriceTextBox.Text = productInfo.Price.ToString("#,##0원");
            CurrentStockTextBox.Text = productInfo.StockQuantity.ToString();

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

        private void UpdateNewStock()
        {
            if (int.TryParse(ReceiveQuantityTextBox.Text, out int receiveQuantity))
            {
                int newStock = productInfo.StockQuantity + receiveQuantity;
                NewStockTextBox.Text = newStock.ToString();
            }
            else
            {
                NewStockTextBox.Text = productInfo.StockQuantity.ToString();
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

                // 확인 버튼 비활성화
                ConfirmButton.IsEnabled = false;
                ConfirmButton.Content = "처리 중...";

                // 입고 처리 요청
                var receiveRequest = new
                {
                    Quantity = receiveQuantity
                };

                var json = JsonSerializer.Serialize(receiveRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"http://localhost:5185/api/product/receive/{productInfo.ProductId}", content);

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
                    
                    ProductUpdated?.Invoke(this, EventArgs.Empty);
                    this.Close();
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
}
