using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace jujin.Views.Inventory
{
    public partial class ItemRegistrationPage : UserControl
    {
        private string selectedImagePath = string.Empty;
        private readonly HttpClient httpClient;
        private readonly IAmazonS3 s3Client;

        public ItemRegistrationPage()
        {
            InitializeComponent();
            SetDefaultImage();
            httpClient = new HttpClient();
            
            // AWS S3 클라이언트 초기화
            var accessKey = App.Configuration["AWS:AccessKey"];
            var secretKey = App.Configuration["AWS:SecretKey"];
            
            if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("AWS 자격증명이 설정되지 않았습니다. appsettings.json을 확인하세요.");
            }
            
            var s3Config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2,
                ServiceURL = "https://s3.ap-southeast-2.amazonaws.com"
            };
            s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
        }

        private void SetDefaultImage()
        {
            // 기본 플레이스홀더 이미지 설정
            var defaultBitmap = new System.Windows.Media.Imaging.BitmapImage();
            defaultBitmap.BeginInit();
            defaultBitmap.UriSource = new Uri("pack://application:,,,/Resources/Images/logo.jpg");
            defaultBitmap.EndInit();
            ProductImagePreview.Source = defaultBitmap;
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
                SelectedImagePath.Text = $"선택됨: {Path.GetFileName(selectedImagePath)}";
                
                try
                {
                    // 이미지 미리보기 업데이트
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedImagePath);
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
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
                    // ACL 설정 제거 - 버킷 정책으로 공개 접근 설정
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

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
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

                if (!int.TryParse(StockQuantityTextBox.Text, out int stockQuantity))
                {
                    MessageBox.Show("올바른 수량을 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(SafetyStockTextBox.Text, out int safetyStock))
                {
                    MessageBox.Show("올바른 안전재고량을 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 창고 선택 검증
                if (LocationComboBox.SelectedItem is not ComboBoxItem selectedLocationItem)
                {
                    MessageBox.Show("창고를 선택해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                int locationId = int.Parse(selectedLocationItem.Tag.ToString());

                // 카테고리 가져오기
                string category = CategoryComboBox.SelectedItem is ComboBoxItem selectedItem ? selectedItem.Content.ToString() : "";

                // 등록 버튼 비활성화
                RegisterButton.IsEnabled = false;
                RegisterButton.Content = "등록 중...";

                // 이미지가 선택된 경우 S3에 업로드
                string imageUrl = null;
                if (!string.IsNullOrEmpty(selectedImagePath) && File.Exists(selectedImagePath))
                {
                    try
                    {
                        RegisterButton.Content = "이미지 업로드 중...";
                        imageUrl = await UploadImageToS3(selectedImagePath);
                        
                        if (string.IsNullOrEmpty(imageUrl))
                        {
                            MessageBox.Show("이미지 업로드에 실패했습니다.", "업로드 실패", 
                                            MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"이미지 업로드 중 오류가 발생했습니다: {ex.Message}", 
                                        "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // 제품 등록
                RegisterButton.Content = "제품 등록 중...";
                await RegisterProduct(ProductNameTextBox.Text, category, price, stockQuantity, safetyStock, locationId, imageUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"등록 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 등록 버튼 다시 활성화
                RegisterButton.IsEnabled = true;
                RegisterButton.Content = "등록";
            }
        }

        private async Task RegisterProduct(string productName, string category, decimal price, int stockQuantity, int safetyStock, int locationId, string imageUrl)
        {
            try
            {
                var productData = new
                {
                    ProductName = productName,
                    Category = category,
                    Price = price,
                    StockQuantity = stockQuantity,
                    SafetyStock = safetyStock,
                    LocationId = locationId,
                    ImageUrl = imageUrl
                };

                var json = JsonSerializer.Serialize(productData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("http://localhost:5185/api/product/register", content);
                
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("제품이 성공적으로 등록되었습니다!", "등록 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                    ResetForm();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"등록에 실패했습니다: {errorContent}", "등록 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"등록 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void ResetForm()
        {
            ProductNameTextBox.Text = "";
            CategoryComboBox.SelectedIndex = -1;
            PriceTextBox.Text = "";
            StockQuantityTextBox.Text = "";
            SafetyStockTextBox.Text = "";
            LocationComboBox.SelectedIndex = -1;
            selectedImagePath = "";
            SelectedImagePath.Text = "선택된 파일이 없습니다.";
            SetDefaultImage();
        }
    }
}
