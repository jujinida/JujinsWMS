using System;
using System.Windows;

namespace jujin.Views.HR
{
    public partial class EmployeeEditDialog : Window
    {
        private EmployeeInfo _employee;

        public EmployeeEditDialog(EmployeeInfo employee)
        {
            InitializeComponent();
            _employee = employee;
            LoadEmployeeData();
        }

        private void LoadEmployeeData()
        {
            EmployeeIdTextBox.Text = _employee.EmployeeId;
            EmployeeNameTextBox.Text = _employee.EmployeeName;
            
            // 부서 설정
            foreach (System.Windows.Controls.ComboBoxItem item in DepartmentComboBox.Items)
            {
                if (item.Content.ToString() == _employee.DepartmentName)
                {
                    DepartmentComboBox.SelectedItem = item;
                    break;
                }
            }
            
            // 직급 설정
            foreach (System.Windows.Controls.ComboBoxItem item in PositionComboBox.Items)
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
            
            PhoneNumberTextBox.Text = _employee.PhoneNumber;
            EmailTextBox.Text = _employee.Email;
            SalaryTextBox.Text = _employee.Salary;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 입력값 검증
            if (string.IsNullOrWhiteSpace(EmployeeNameTextBox.Text))
            {
                MessageBox.Show("이름을 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 데이터 업데이트
            _employee.EmployeeName = EmployeeNameTextBox.Text;
            _employee.DepartmentName = DepartmentComboBox.SelectedItem?.ToString() ?? "";
            _employee.Position = PositionComboBox.SelectedItem?.ToString() ?? "";
            _employee.HireDate = HireDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
            _employee.BirthDate = BirthDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
            _employee.PhoneNumber = PhoneNumberTextBox.Text;
            _employee.Email = EmailTextBox.Text;
            _employee.Salary = SalaryTextBox.Text;

            MessageBox.Show("직원 정보가 수정되었습니다.", "수정 완료", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
