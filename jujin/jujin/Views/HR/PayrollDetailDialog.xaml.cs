using System;
using System.Windows;

namespace jujin.Views.HR
{
    public partial class PayrollDetailDialog : Window
    {
        private PayrollRecord _payrollRecord;

        public PayrollDetailDialog(PayrollRecord payrollRecord)
        {
            InitializeComponent();
            _payrollRecord = payrollRecord;
            LoadPayrollData();
        }

        private void LoadPayrollData()
        {
            EmployeeIdText.Text = _payrollRecord.EmployeeId;
            EmployeeNameText.Text = _payrollRecord.EmployeeName;
            DepartmentText.Text = _payrollRecord.DepartmentName;
            PositionText.Text = _payrollRecord.Position;
            BaseSalaryText.Text = _payrollRecord.BaseSalary.ToString("N0") + "원";
            
            // 수당 내역 (샘플 데이터)
            AllowanceDetailText.Text = "• 성과급: " + (_payrollRecord.Allowance * 0.6m).ToString("N0") + "원\n" +
                                      "• 교통비: " + (_payrollRecord.Allowance * 0.2m).ToString("N0") + "원\n" +
                                      "• 식비: " + (_payrollRecord.Allowance * 0.2m).ToString("N0") + "원";
            TotalAllowanceText.Text = _payrollRecord.Allowance.ToString("N0") + "원";
            
            // 공제 내역 (샘플 데이터)
            DeductionDetailText.Text = "• 국민연금: " + (_payrollRecord.Deduction * 0.45m).ToString("N0") + "원\n" +
                                      "• 건강보험: " + (_payrollRecord.Deduction * 0.35m).ToString("N0") + "원\n" +
                                      "• 고용보험: " + (_payrollRecord.Deduction * 0.1m).ToString("N0") + "원\n" +
                                      "• 소득세: " + (_payrollRecord.Deduction * 0.1m).ToString("N0") + "원";
            TotalDeductionText.Text = _payrollRecord.Deduction.ToString("N0") + "원";
            
            NetSalaryText.Text = _payrollRecord.NetSalary.ToString("N0") + "원";
            PayDateText.Text = _payrollRecord.PayDate;
            PaymentStatusText.Text = _payrollRecord.PaymentStatus;

            // 지급여부에 따라 버튼 활성화/비활성화
            if (_payrollRecord.PaymentStatus == "지급완료")
            {
                PayButton.IsEnabled = false;
                PayButton.Content = "지급완료";
                PayButton.Background = System.Windows.Media.Brushes.Gray;
            }
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"{_payrollRecord.EmployeeName}님의 급여를 지급하시겠습니까?\n지급액: {_payrollRecord.NetSalary:N0}원", 
                "급여 지급 확인", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _payrollRecord.PaymentStatus = "지급완료";
                _payrollRecord.PayDate = DateTime.Now.ToString("yyyy-MM-dd");
                
                // UI 업데이트
                PaymentStatusText.Text = "지급완료";
                PayDateText.Text = _payrollRecord.PayDate;
                PayButton.IsEnabled = false;
                PayButton.Content = "지급완료";
                PayButton.Background = System.Windows.Media.Brushes.Gray;
                
                MessageBox.Show("급여가 지급되었습니다.", "지급 완료", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
