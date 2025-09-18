using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.HR
{
    public partial class PayrollManagementPage : UserControl
    {
        public PayrollManagementPage()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // 샘플 데이터 로드 (실제로는 데이터베이스에서 가져옴)
            var samplePayrolls = new ObservableCollection<PayrollRecord>
            {
                new PayrollRecord { EmployeeId = "EMP001", EmployeeName = "김철수", DepartmentName = "개발팀", Position = "대리", 
                    BaseSalary = 3500000, Allowance = 500000, Deduction = 400000, NetSalary = 3600000, 
                    PayDate = "2024-02-25", PaymentStatus = "지급완료" },
                new PayrollRecord { EmployeeId = "EMP002", EmployeeName = "이영희", DepartmentName = "마케팅팀", Position = "과장", 
                    BaseSalary = 4200000, Allowance = 600000, Deduction = 500000, NetSalary = 4300000, 
                    PayDate = "2024-02-25", PaymentStatus = "지급완료" },
                new PayrollRecord { EmployeeId = "EMP003", EmployeeName = "박민수", DepartmentName = "인사팀", Position = "차장", 
                    BaseSalary = 4800000, Allowance = 700000, Deduction = 600000, NetSalary = 4900000, 
                    PayDate = "2024-02-25", PaymentStatus = "지급완료" },
                new PayrollRecord { EmployeeId = "EMP004", EmployeeName = "최지영", DepartmentName = "재무팀", Position = "부장", 
                    BaseSalary = 5500000, Allowance = 800000, Deduction = 700000, NetSalary = 5600000, 
                    PayDate = "2024-02-25", PaymentStatus = "지급완료" },
                new PayrollRecord { EmployeeId = "EMP005", EmployeeName = "정현우", DepartmentName = "영업팀", Position = "사원", 
                    BaseSalary = 2800000, Allowance = 300000, Deduction = 300000, NetSalary = 2800000, 
                    PayDate = "2024-02-25", PaymentStatus = "지급완료" },
                new PayrollRecord { EmployeeId = "EMP006", EmployeeName = "한소영", DepartmentName = "기획팀", Position = "대리", 
                    BaseSalary = 3200000, Allowance = 400000, Deduction = 350000, NetSalary = 3250000, 
                    PayDate = "2024-02-25", PaymentStatus = "지급완료" },
                new PayrollRecord { EmployeeId = "EMP007", EmployeeName = "윤태호", DepartmentName = "개발팀", Position = "과장", 
                    BaseSalary = 4500000, Allowance = 650000, Deduction = 550000, NetSalary = 4600000, 
                    PayDate = "2024-02-25", PaymentStatus = "지급완료" },
                new PayrollRecord { EmployeeId = "EMP008", EmployeeName = "강미래", DepartmentName = "마케팅팀", Position = "사원", 
                    BaseSalary = 2600000, Allowance = 250000, Deduction = 280000, NetSalary = 2570000, 
                    PayDate = "2024-02-25", PaymentStatus = "지급완료" },
                new PayrollRecord { EmployeeId = "EMP009", EmployeeName = "서동현", DepartmentName = "영업팀", Position = "대리", 
                    BaseSalary = 3300000, Allowance = 450000, Deduction = 380000, NetSalary = 3370000, 
                    PayDate = "2024-02-25", PaymentStatus = "미지급" },
                new PayrollRecord { EmployeeId = "EMP010", EmployeeName = "임수진", DepartmentName = "기획팀", Position = "사원", 
                    BaseSalary = 2700000, Allowance = 280000, Deduction = 290000, NetSalary = 2690000, 
                    PayDate = "2024-02-25", PaymentStatus = "미지급" }
            };

            PayrollDataGrid.ItemsSource = samplePayrolls;
        }

        private void PayrollDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowPayrollDetailDialog();
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPayroll = PayrollDataGrid.SelectedItem as PayrollRecord;
            if (selectedPayroll != null && selectedPayroll.PaymentStatus == "미지급")
            {
                var result = MessageBox.Show($"{selectedPayroll.EmployeeName}님의 급여를 지급하시겠습니까?\n지급액: {selectedPayroll.NetSalary:N0}원", 
                    "급여 지급 확인", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    selectedPayroll.PaymentStatus = "지급완료";
                    selectedPayroll.PayDate = DateTime.Now.ToString("yyyy-MM-dd");
                    MessageBox.Show("급여가 지급되었습니다.", "지급 완료", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (selectedPayroll != null && selectedPayroll.PaymentStatus == "지급완료")
            {
                MessageBox.Show("이미 지급된 급여입니다.", "알림", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
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
        public string Position { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Allowance { get; set; }
        public decimal Deduction { get; set; }
        public decimal NetSalary { get; set; }
        public string PayDate { get; set; }
        public string PaymentStatus { get; set; }
    }
}
