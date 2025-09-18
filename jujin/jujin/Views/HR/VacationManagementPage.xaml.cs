using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.HR
{
    public partial class VacationManagementPage : UserControl
    {
        public VacationManagementPage()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // 샘플 데이터 로드 (실제로는 데이터베이스에서 가져옴)
            var sampleVacations = new ObservableCollection<VacationRequest>
            {
                new VacationRequest { EmployeeId = "EMP001", EmployeeName = "김철수", DepartmentName = "개발팀", Position = "대리", 
                    VacationReason = "개인사정", StartDate = "2024-02-15", EndDate = "2024-02-16", VacationDays = 2, RemainingDays = 8, ApprovalStatus = "대기" },
                new VacationRequest { EmployeeId = "EMP002", EmployeeName = "이영희", DepartmentName = "마케팅팀", Position = "과장", 
                    VacationReason = "가족여행", StartDate = "2024-02-20", EndDate = "2024-02-22", VacationDays = 3, RemainingDays = 12, ApprovalStatus = "대기" },
                new VacationRequest { EmployeeId = "EMP003", EmployeeName = "박민수", DepartmentName = "인사팀", Position = "차장", 
                    VacationReason = "건강검진", StartDate = "2024-02-10", EndDate = "2024-02-10", VacationDays = 1, RemainingDays = 15, ApprovalStatus = "승인" },
                new VacationRequest { EmployeeId = "EMP004", EmployeeName = "최지영", DepartmentName = "재무팀", Position = "부장", 
                    VacationReason = "가족행사", StartDate = "2024-02-25", EndDate = "2024-02-27", VacationDays = 3, RemainingDays = 20, ApprovalStatus = "대기" },
                new VacationRequest { EmployeeId = "EMP005", EmployeeName = "정현우", DepartmentName = "영업팀", Position = "사원", 
                    VacationReason = "개인휴가", StartDate = "2024-02-12", EndDate = "2024-02-14", VacationDays = 3, RemainingDays = 5, ApprovalStatus = "거부" },
                new VacationRequest { EmployeeId = "EMP006", EmployeeName = "한소영", DepartmentName = "기획팀", Position = "대리", 
                    VacationReason = "병원진료", StartDate = "2024-02-18", EndDate = "2024-02-18", VacationDays = 1, RemainingDays = 10, ApprovalStatus = "승인" },
                new VacationRequest { EmployeeId = "EMP007", EmployeeName = "윤태호", DepartmentName = "개발팀", Position = "과장", 
                    VacationReason = "가족여행", StartDate = "2024-03-01", EndDate = "2024-03-05", VacationDays = 5, RemainingDays = 18, ApprovalStatus = "대기" },
                new VacationRequest { EmployeeId = "EMP008", EmployeeName = "강미래", DepartmentName = "마케팅팀", Position = "사원", 
                    VacationReason = "개인사정", StartDate = "2024-02-28", EndDate = "2024-02-29", VacationDays = 2, RemainingDays = 7, ApprovalStatus = "대기" }
            };

            VacationDataGrid.ItemsSource = sampleVacations;
        }

        private void VacationDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowVacationDetailDialog();
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedVacation = VacationDataGrid.SelectedItem as VacationRequest;
            if (selectedVacation != null)
            {
                var result = MessageBox.Show($"{selectedVacation.EmployeeName}님의 휴가를 승인하시겠습니까?", "휴가 승인 확인",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    selectedVacation.ApprovalStatus = "승인";
                    MessageBox.Show("휴가가 승인되었습니다.", "승인 완료", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedVacation = VacationDataGrid.SelectedItem as VacationRequest;
            if (selectedVacation != null)
            {
                var result = MessageBox.Show($"{selectedVacation.EmployeeName}님의 휴가를 거부하시겠습니까?", "휴가 거부 확인",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    selectedVacation.ApprovalStatus = "거부";
                    MessageBox.Show("휴가가 거부되었습니다.", "거부 완료", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ShowVacationDetailDialog()
        {
            var selectedVacation = VacationDataGrid.SelectedItem as VacationRequest;
            if (selectedVacation != null)
            {
                var detailDialog = new VacationDetailDialog(selectedVacation);
                detailDialog.Owner = Window.GetWindow(this);
                detailDialog.ShowDialog();
            }
        }
    }

    // 휴가 신청 클래스
    public class VacationRequest
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string Position { get; set; }
        public string VacationReason { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int VacationDays { get; set; }
        public int RemainingDays { get; set; }
        public string ApprovalStatus { get; set; }
    }
}
