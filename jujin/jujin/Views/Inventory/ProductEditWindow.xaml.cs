using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Win32;

namespace jujin.Views.Inventory
{
    public partial class ProductEditWindow : Window
    {
        private readonly HttpClient httpClient;
        private readonly ProductInfo productInfo;
        private readonly IAmazonS3 s3Client;
        private string selectedImagePath = string.Empty;
        private string currentImageUrl = string.Empty;
        public event EventHandler ProductUpdated;

        public ProductEditWindow(ProductInfo product)
        {
            InitializeComponent();
            httpClient = new HttpClient();
            productInfo = product;
            currentImageUrl = product.ImageUrl ?? string.Empty;
            
            // AWS S3 클라이언트 초기화
            var s3Config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2,
                ServiceURL = "https://s3.ap-southeast-2.amazonaws.com"
            };
            s3Client = new AmazonS3Client("AKIASKD5PB3ZHMNVFSVG", "3mmMbruDzQfsZ61PSCsE6zo92aDc0EmlBA/Axu0I", s3Config);
            
            LoadProductData();
        }

        private void LoadProductData()
        {
            // 제품 정보 로드
            ProductIdTextBox.Text = productInfo.ProductId.ToString();
            ProductNameTextBox.Text = productInfo.ProductName;
            PriceTextBox.Text = productInfo.Price.ToString();
            StockQuantityTextBox.Text = productInfo.StockQuantity.ToString();
            SafetyStockTextBox.Text = productInfo.SafetyStock.ToString();
            LocationTextBox.Text = productInfo.LocationName ?? "미지정";
            
            // 카테고리 설정
            foreach (System.Windows.Controls.ComboBoxItem item in CategoryComboBox.Items)
            {
                if (item.Content.ToString() == productInfo.Category)
                {
                    CategoryComboBox.SelectedItem = item;
                    break;
                }
            }
            
            // 이미지 로드
            LoadProductImage();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 입력값 검증
                if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
                {
                    MessageBox.Show("품목명을 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(PriceTextBox.Text, out decimal price))
                {
                    MessageBox.Show("올바른 가격을 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 재고량은 읽기 전용이므로 검증하지 않음
                int stockQuantity = productInfo.StockQuantity; // 기존 재고량 유지

                if (!int.TryParse(SafetyStockTextBox.Text, out int safetyStock))
                {
                    MessageBox.Show("올바른 안전재고량을 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 카테고리 가져오기
                string category = CategoryComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem ? selectedItem.Content.ToString() : "";

                // 저장 버튼 비활성화
                SaveButton.IsEnabled = false;
                SaveButton.Content = "저장 중...";

                // 새 이미지가 선택된 경우 S3에 업로드
                string newImageUrl = currentImageUrl; // 기본적으로 현재 이미지 URL 유지
                if (!string.IsNullOrEmpty(selectedImagePath) && File.Exists(selectedImagePath))
                {
                    try
                    {
                        SaveButton.Content = "이미지 업로드 중...";
                        newImageUrl = await UploadImageToS3(selectedImagePath);
                        if (string.IsNullOrEmpty(newImageUrl))
                        {
                            MessageBox.Show("새 이미지 업로드에 실패했습니다.", "업로드 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"새 이미지 업로드 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // 제품 수정 요청
                SaveButton.Content = "제품 정보 저장 중...";
                var updateRequest = new
                {
                    ProductName = ProductNameTextBox.Text,
                    Category = category,
                    Price = price,
                    StockQuantity = stockQuantity,
                    SafetyStock = safetyStock,
                    ImageUrl = newImageUrl
                };

                var json = JsonSerializer.Serialize(updateRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync($"http://localhost:5185/api/product/update/{productInfo.ProductId}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("제품이 성공적으로 수정되었습니다!", "수정 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                    ProductUpdated?.Invoke(this, EventArgs.Empty);
                    this.Close();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"수정에 실패했습니다: {errorContent}", "수정 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"수정 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 저장 버튼 다시 활성화
                SaveButton.IsEnabled = true;
                SaveButton.Content = "저장";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadProductImage()
        {
            if (!string.IsNullOrEmpty(currentImageUrl))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(currentImageUrl);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ProductImagePreview.Source = bitmap;
                    ImageUrlTextBlock.Text = "현재 이미지";
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

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "제품 이미지 선택",
                Filter = "이미지 파일 (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|모든 파일 (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePath = openFileDialog.FileName;
                SelectedImagePath.Text = $"새 파일: {Path.GetFileName(selectedImagePath)}";

                try
                {
                    // 이미지 미리보기 업데이트
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedImagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ProductImagePreview.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"이미지를 로드하는 중 오류가 발생했습니다: {ex.Message}", 
                                    "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task<string> UploadImageToS3(string imagePath)
        {
            try
            {
                // 파일 확장자 검증
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                var fileExtension = Path.GetExtension(imagePath).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new ArgumentException("지원하지 않는 이미지 형식입니다.");
                }

                // 고유한 파일명 생성
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var bucketName = "devmour";

                using var fileStream = File.OpenRead(imagePath);
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = $"products/{fileName}",
                    InputStream = fileStream,
                    ContentType = GetContentType(fileExtension)
                };

                var response = await s3Client.PutObjectAsync(request);
                
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    var imageUrl = $"https://{bucketName}.s3.ap-southeast-2.amazonaws.com/products/{fileName}";
                    return imageUrl;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"S3 업로드 중 오류 발생: {ex.Message}");
            }
        }

        private string GetContentType(string fileExtension)
        {
            return fileExtension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".bmp" => "image/bmp",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}
