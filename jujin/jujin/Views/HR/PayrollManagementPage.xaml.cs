using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.HR
{
    public partial class PayrollManagementPage : UserControl
    {
        private ObservableCollection<PayrollRecord> payrollRecords;
        private ObservableCollection<PayrollRecord> allPayrollRecords;
        private DateTime currentMonth;

        public PayrollManagementPage()
        {
            InitializeComponent();
            payrollRecords = new ObservableCollection<PayrollRecord>();
            allPayrollRecords = new ObservableCollection<PayrollRecord>();
            PayrollDataGrid.ItemsSource = payrollRecords;
            
            // 현재 월로 초기화
            currentMonth = new DateTime(2025, 9, 1);
            UpdateMonthDisplay();
            LoadPayrollData();
        }

        private async Task LoadPayrollData(string paymentMonth = null)
        {
            try
            {
                if (paymentMonth == null)
                {
                    paymentMonth = currentMonth.ToString("yyyy-MM");
                }

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"http://localhost:5185/api/hr/payroll?paymentMonth={paymentMonth}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var payrollDtos = JsonSerializer.Deserialize<List<PayrollDto>>(json, options);

                    allPayrollRecords.Clear();
                    
                    if (payrollDtos != null)
                    {
                        foreach (var dto in payrollDtos)
                        {
                            var payrollRecord = new PayrollRecord
                            {
                                EmployeeId = dto.EmployeeId.ToString(),
                                EmployeeName = dto.EmployeeName,
                                DepartmentName = GetDepartmentName(dto.DepartmentId),
                                DepartmentId = dto.DepartmentId,
                                Position = dto.Position,
                                BaseSalary = dto.GrossPay,
                                Allowance = dto.Allowance,
                                Deduction = dto.Deductions,
                                NetSalary = dto.NetPay,
                                PayDate = dto.PaymentDate,
                                PaymentStatus = dto.PaymentStatus
                            };
                            allPayrollRecords.Add(payrollRecord);
                        }
                    }
                    
                    ApplyFilters();
                }
                else
                {
                    MessageBox.Show("급여 데이터를 불러오는데 실패했습니다.", "오류", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"급여 데이터를 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateMonthDisplay()
        {
            CurrentMonthText.Text = $"{currentMonth.Year}년 {currentMonth.Month}월";
        }

        private void ApplyFilters()
        {
            payrollRecords.Clear();

            var filteredRecords = allPayrollRecords.Where(record =>
            {
                // 직원명 필터
                var employeeNameFilter = EmployeeNameFilter.Text.Trim();
                if (!string.IsNullOrEmpty(employeeNameFilter) && 
                    !record.EmployeeName.Contains(employeeNameFilter, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // 부서 필터
                var selectedDepartment = DepartmentFilter.SelectedItem as ComboBoxItem;
                if (selectedDepartment != null && selectedDepartment.Tag != null)
                {
                    var departmentId = int.Parse(selectedDepartment.Tag.ToString());
                    if (departmentId != 0 && record.DepartmentId != departmentId)
                    {
                        return false;
                    }
                }

                // 지급상태 필터
                var selectedStatus = PaymentStatusFilter.SelectedItem as ComboBoxItem;
                if (selectedStatus != null && selectedStatus.Content.ToString() != "전체")
                {
                    if (record.PaymentStatus != selectedStatus.Content.ToString())
                    {
                        return false;
                    }
                }

                return true;
            });

            foreach (var record in filteredRecords)
            {
                payrollRecords.Add(record);
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

        private void PayrollDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowPayrollDetailDialog();
        }

        private void PreviousMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentMonth = currentMonth.AddMonths(-1);
            UpdateMonthDisplay();
            LoadPayrollData();
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentMonth = currentMonth.AddMonths(1);
            UpdateMonthDisplay();
            LoadPayrollData();
        }

        private void CalendarButton_Click(object sender, RoutedEventArgs e)
        {
            var calendarDialog = new MonthCalendarDialog(currentMonth);
            calendarDialog.Owner = Window.GetWindow(this);
            
            if (calendarDialog.ShowDialog() == true)
            {
                currentMonth = calendarDialog.SelectedDate;
                UpdateMonthDisplay();
                LoadPayrollData();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPayroll = PayrollDataGrid.SelectedItem as PayrollRecord;
            if (selectedPayroll != null)
            {
                var recordDialog = new PayrollRecordDialog(selectedPayroll.EmployeeId, currentMonth.Year);
                recordDialog.Owner = Window.GetWindow(this);
                recordDialog.ShowDialog();
            }
        }

        private void DetailButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPayrollDetailDialog();
        }

        private void ShowPayrollDetailDialog()
        {
            var selectedPayroll = PayrollDataGrid.SelectedItem as PayrollRecord;
            if (selectedPayroll != null)
            {
                var detailDialog = new PayrollDetailDialog(selectedPayroll);
                detailDialog.Owner = Window.GetWindow(this);
                detailDialog.ShowDialog();
            }
        }
    }

    // 급여 기록 클래스
    public class PayrollRecord
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; }
        public string Position { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Allowance { get; set; }
        public decimal Deduction { get; set; }
        public decimal NetSalary { get; set; }
        public string PayDate { get; set; }
        public string PaymentStatus { get; set; }
    }

    // 백엔드 DTO 클래스
    public class PayrollDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int DepartmentId { get; set; }
        public string Position { get; set; }
        public decimal GrossPay { get; set; }
        public decimal Allowance { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }
        public string PaymentMonth { get; set; }
        public string PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
    }
}
