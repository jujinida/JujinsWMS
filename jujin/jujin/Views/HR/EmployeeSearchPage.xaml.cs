using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.HR
{
    public partial class EmployeeSearchPage : UserControl
    {
        private readonly HttpClient httpClient;
        private ObservableCollection<EmployeeInfo> allEmployees;
        private ObservableCollection<EmployeeInfo> filteredEmployees;

        public EmployeeSearchPage()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            allEmployees = new ObservableCollection<EmployeeInfo>();
            filteredEmployees = new ObservableCollection<EmployeeInfo>();
            EmployeeDataGrid.ItemsSource = filteredEmployees;
            Loaded += EmployeeSearchPage_Loaded;
        }

        private async void EmployeeSearchPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadEmployees();
        }

        private async Task LoadEmployees()
        {
            try
            {
                var response = await httpClient.GetAsync("http://localhost:5185/api/hr/employees");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var employeeDtos = JsonSerializer.Deserialize<List<EmployeeDto>>(json, options);

                    allEmployees.Clear();
                    foreach (var dto in employeeDtos)
                    {
                        allEmployees.Add(new EmployeeInfo
                        {
                            EmployeeId = dto.EmployeeId,
                            EmployeeName = dto.EmployeeName,
                            DepartmentName = GetDepartmentName(dto.DepartmentId),
                            Position = dto.Position,
                            HireDate = dto.HireDate,
                            BirthDate = dto.BirthDate,
                            PhoneNumber = dto.PhoneNumber,
                            Address = dto.Address,
                            Email = dto.Email ?? "",
                            Salary = dto.Salary,
                            ProfileUrl = dto.ProfileUrl
                        });
                    }

                    ApplyFilters();
                }
                else
                {
                    MessageBox.Show("직원 목록을 불러오는데 실패했습니다.", "오류", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"직원 목록을 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetDepartmentName(int departmentId)
        {
            return departmentId switch
            {
                1 => "IT",
                2 => "마케팅",
                3 => "인사",
                4 => "재무",
                5 => "영업",
                6 => "기획",
                _ => "알 수 없음"
            };
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void EmployeeDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowEditDialog();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            ShowEditDialog();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("선택한 직원을 삭제하시겠습니까?", "삭제 확인",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // 실제로는 데이터베이스에서 삭제
                MessageBox.Show("직원 정보가 삭제되었습니다.", "삭제 완료", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ApplyFilters()
        {
            filteredEmployees.Clear();

            var employeeIdFilter = GetTextBoxValue("EmployeeIdTextBox");
            var nameFilter = GetTextBoxValue("NameTextBox");
            var departmentFilter = GetComboBoxValue("DepartmentComboBox");

            foreach (var employee in allEmployees)
            {
                bool matches = true;

                if (!string.IsNullOrEmpty(employeeIdFilter) && 
                    !employee.EmployeeName.Contains(employeeIdFilter, StringComparison.OrdinalIgnoreCase))
                {
                    matches = false;
                }

                if (!string.IsNullOrEmpty(nameFilter) && 
                    !employee.EmployeeName.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                {
                    matches = false;
                }

                if (!string.IsNullOrEmpty(departmentFilter) && departmentFilter != "전체" && 
                    employee.DepartmentName != departmentFilter)
                {
                    matches = false;
                }

                if (matches)
                {
                    filteredEmployees.Add(employee);
                }
            }
        }

        private string GetTextBoxValue(string name)
        {
            var textBox = FindName(name) as TextBox;
            return textBox?.Text?.Trim() ?? "";
        }

        private string GetComboBoxValue(string name)
        {
            var comboBox = FindName(name) as ComboBox;
            return (comboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        }

        private void ShowEditDialog()
        {
            var selectedEmployee = EmployeeDataGrid.SelectedItem as EmployeeInfo;
            if (selectedEmployee != null)
            {
                var editDialog = new EmployeeEditDialog(selectedEmployee);
                editDialog.Owner = Window.GetWindow(this);
                editDialog.ShowDialog();
                
                // 다이얼로그가 닫힌 후 데이터 새로고침
                LoadEmployees();
            }
        }
    }

    // 직원 정보 클래스
    public class EmployeeInfo
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string Position { get; set; }
        public string HireDate { get; set; }
        public string BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int Salary { get; set; }
        public string Address { get; set; }
        public string ProfileUrl { get; set; }
    }

    // 백엔드에서 받아오는 DTO
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string BirthDate { get; set; }
        public string HireDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public int DepartmentId { get; set; }
        public int Salary { get; set; }
        public string ProfileUrl { get; set; }
    }
}
