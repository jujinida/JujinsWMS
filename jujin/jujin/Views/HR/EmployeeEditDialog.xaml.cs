using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Amazon.S3;
using Amazon.S3.Model;

namespace jujin.Views.HR
{
    public partial class EmployeeEditDialog : Window
    {
        private EmployeeInfo _employee;
        private string selectedImagePath = string.Empty;
        private readonly HttpClient httpClient;

        public EmployeeEditDialog(EmployeeInfo employee)
        {
            InitializeComponent();
            _employee = employee;
            httpClient = new HttpClient();
            LoadEmployeeData();
        }

        private void LoadEmployeeData()
        {
            NameTextBox.Text = _employee.EmployeeName;
            
            // 부서 설정
            foreach (ComboBoxItem item in DepartmentComboBox.Items)
            {
                if (item.Content.ToString() == _employee.DepartmentName)
                {
                    DepartmentComboBox.SelectedItem = item;
                    break;
                }
            }
            
            // 직급 설정
            foreach (ComboBoxItem item in PositionComboBox.Items)
            {
                if (item.Content.ToString() == _employee.Position)
                {
                    PositionComboBox.SelectedItem = item;
                    break;
                }
            }
            
            // 날짜 설정
            if (DateTime.TryParse(_employee.HireDate, out DateTime hireDate))
            {
                HireDatePicker.SelectedDate = hireDate;
            }
            
            if (DateTime.TryParse(_employee.BirthDate, out DateTime birthDate))
            {
                BirthDatePicker.SelectedDate = birthDate;
            }
            
            PhoneTextBox.Text = _employee.PhoneNumber;
            AddressTextBox.Text = _employee.Address ?? "";
            EmailTextBox.Text = _employee.Email;
            SalaryTextBox.Text = _employee.Salary.ToString();

            // 이미지 로드
            if (!string.IsNullOrEmpty(_employee.ProfileUrl))
            {
                LoadImageFromUrl(_employee.ProfileUrl);
            }
        }

        private void LoadImageFromUrl(string imageUrl)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ProfileImagePreview.Source = bitmap;
                SelectedFilePathTextBlock.Text = "기존 이미지";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"이미지를 로드하는 중 오류가 발생했습니다: {ex.Message}", 
                                "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 입력 검증
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    MessageBox.Show("이름을 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    NameTextBox.Focus();
                    return;
                }

                if (DepartmentComboBox.SelectedItem == null)
                {
                    MessageBox.Show("부서를 선택해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DepartmentComboBox.Focus();
                    return;
                }

                if (PositionComboBox.SelectedItem == null)
                {
                    MessageBox.Show("직급을 선택해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    PositionComboBox.Focus();
                    return;
                }

                if (HireDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("입사일을 선택해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    HireDatePicker.Focus();
                    return;
                }

                if (BirthDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("생년월일을 선택해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    BirthDatePicker.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
                {
                    MessageBox.Show("전화번호를 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    PhoneTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
                {
                    MessageBox.Show("이메일을 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    EmailTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(SalaryTextBox.Text))
                {
                    MessageBox.Show("급여를 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SalaryTextBox.Focus();
                    return;
                }

                // AWS S3에 이미지 업로드 (새로운 이미지가 선택된 경우)
                string imageUrl = _employee.ProfileUrl; // 기존 이미지 URL 유지
                if (!string.IsNullOrEmpty(selectedImagePath))
                {
                    imageUrl = await UploadImageToS3(selectedImagePath);
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        MessageBox.Show("이미지 업로드에 실패했습니다.", "업로드 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // 백엔드에 직원 정보 업데이트
                await UpdateEmployee(imageUrl);

                MessageBox.Show("직원 정보가 수정되었습니다.", "수정 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"직원 정보 수정 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "프로필 이미지 선택",
                Filter = "이미지 파일 (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|모든 파일 (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePath = openFileDialog.FileName;
                LoadImagePreview(selectedImagePath);
                SelectedFilePathTextBlock.Text = Path.GetFileName(selectedImagePath);
            }
        }

        private void LoadImagePreview(string imagePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ProfileImagePreview.Source = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"이미지를 로드하는 중 오류가 발생했습니다: {ex.Message}", 
                                "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> UploadImageToS3(string imagePath)
        {
            try
            {
                var s3Client = new AmazonS3Client("AKIASKD5PB3ZHMNVFSVG", "3mmMbruDzQfsZ61PSCsE6zo92aDc0EmlBA/Axu0I", Amazon.RegionEndpoint.APSoutheast2);
                string bucketName = "devmour";

                string fileName = $"employees/{Guid.NewGuid()}{Path.GetExtension(imagePath)}";
                string fileExtension = Path.GetExtension(imagePath);

                using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = fileName,
                        InputStream = fileStream,
                        ContentType = GetContentType(fileExtension)
                    };

                    var response = await s3Client.PutObjectAsync(request);
                    
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var imageUrl = $"https://{bucketName}.s3.ap-southeast-2.amazonaws.com/{fileName}";
                        return imageUrl;
                    }
                    else
                    {
                        return string.Empty;
                    }
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

        private async Task UpdateEmployee(string imageUrl)
        {
            try
            {
                // 부서명을 department_id로 변환
                int departmentId = GetDepartmentId(((ComboBoxItem)DepartmentComboBox.SelectedItem).Content.ToString());

                var employeeData = new
                {
                    EmployeeName = NameTextBox.Text.Trim(),
                    BirthDate = BirthDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    HireDate = HireDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    PhoneNumber = PhoneTextBox.Text.Trim(),
                    Address = AddressTextBox.Text.Trim(),
                    Position = ((ComboBoxItem)PositionComboBox.SelectedItem).Content.ToString(),
                    Email = EmailTextBox.Text.Trim(),
                    DepartmentId = departmentId,
                    Salary = int.Parse(SalaryTextBox.Text.Trim()),
                    ProfileUrl = imageUrl
                };

                var json = JsonSerializer.Serialize(employeeData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync($"http://localhost:5185/api/hr/employees/{_employee.EmployeeId}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"백엔드 업데이트 실패: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"직원 정보 업데이트 중 오류 발생: {ex.Message}");
            }
        }

        private int GetDepartmentId(string departmentName)
        {
            return departmentName switch
            {
                "IT" => 1,
                "마케팅" => 2,
                "인사" => 3,
                "재무" => 4,
                "영업" => 5,
                "기획" => 6,
                _ => 1
            };
        }
    }
}
