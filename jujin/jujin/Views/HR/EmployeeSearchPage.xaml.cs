using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.HR
{
    public partial class EmployeeSearchPage : UserControl
    {
        public EmployeeSearchPage()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // 샘플 데이터 로드 (실제로는 데이터베이스에서 가져옴)
            var sampleEmployees = new ObservableCollection<EmployeeInfo>
            {
                new EmployeeInfo { EmployeeId = "EMP001", EmployeeName = "김철수", DepartmentName = "개발팀", Position = "대리", HireDate = "2020-03-15", BirthDate = "1985-05-20", PhoneNumber = "010-1234-5678", Email = "kim@company.com", Salary = "4,500만원" },
                new EmployeeInfo { EmployeeId = "EMP002", EmployeeName = "이영희", DepartmentName = "마케팅팀", Position = "과장", HireDate = "2019-07-01", BirthDate = "1982-12-10", PhoneNumber = "010-2345-6789", Email = "lee@company.com", Salary = "5,200만원" },
                new EmployeeInfo { EmployeeId = "EMP003", EmployeeName = "박민수", DepartmentName = "인사팀", Position = "차장", HireDate = "2018-01-10", BirthDate = "1980-08-25", PhoneNumber = "010-3456-7890", Email = "park@company.com", Salary = "6,000만원" },
                new EmployeeInfo { EmployeeId = "EMP004", EmployeeName = "최지영", DepartmentName = "재무팀", Position = "부장", HireDate = "2017-06-20", BirthDate = "1978-03-15", PhoneNumber = "010-4567-8901", Email = "choi@company.com", Salary = "7,500만원" },
                new EmployeeInfo { EmployeeId = "EMP005", EmployeeName = "정현우", DepartmentName = "영업팀", Position = "사원", HireDate = "2021-09-01", BirthDate = "1990-11-30", PhoneNumber = "010-5678-9012", Email = "jung@company.com", Salary = "3,800만원" },
                new EmployeeInfo { EmployeeId = "EMP006", EmployeeName = "한소영", DepartmentName = "기획팀", Position = "대리", HireDate = "2020-11-15", BirthDate = "1987-07-08", PhoneNumber = "010-6789-0123", Email = "han@company.com", Salary = "4,200만원" },
                new EmployeeInfo { EmployeeId = "EMP007", EmployeeName = "윤태호", DepartmentName = "개발팀", Position = "과장", HireDate = "2018-04-01", BirthDate = "1983-09-12", PhoneNumber = "010-7890-1234", Email = "yoon@company.com", Salary = "5,800만원" },
                new EmployeeInfo { EmployeeId = "EMP008", EmployeeName = "강미래", DepartmentName = "마케팅팀", Position = "사원", HireDate = "2022-02-01", BirthDate = "1992-04-18", PhoneNumber = "010-8901-2345", Email = "kang@company.com", Salary = "3,500만원" }
            };

            EmployeeDataGrid.ItemsSource = sampleEmployees;
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

        private void ShowEditDialog()
        {
            var selectedEmployee = EmployeeDataGrid.SelectedItem as EmployeeInfo;
            if (selectedEmployee != null)
            {
                var editDialog = new EmployeeEditDialog(selectedEmployee);
                editDialog.Owner = Window.GetWindow(this);
                editDialog.ShowDialog();
                
                // 다이얼로그가 닫힌 후 데이터 새로고침
                LoadSampleData();
            }
        }
    }

    // 직원 정보 클래스
    public class EmployeeInfo
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string Position { get; set; }
        public string HireDate { get; set; }
        public string BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Salary { get; set; }
    }
}
