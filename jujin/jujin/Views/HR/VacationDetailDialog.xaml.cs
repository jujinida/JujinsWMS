using System;
using System.Windows;

namespace jujin.Views.HR
{
    public partial class VacationDetailDialog : Window
    {
        private VacationRequest _vacationRequest;

        public VacationDetailDialog(VacationRequest vacationRequest)
        {
            InitializeComponent();
            _vacationRequest = vacationRequest;
            LoadVacationData();
        }

        private void LoadVacationData()
        {
            EmployeeIdText.Text = _vacationRequest.EmployeeId;
            EmployeeNameText.Text = _vacationRequest.EmployeeName;
            DepartmentText.Text = _vacationRequest.DepartmentName;
            PositionText.Text = _vacationRequest.Position;
            StartDateText.Text = _vacationRequest.StartDate;
            EndDateText.Text = _vacationRequest.EndDate;
            VacationDaysText.Text = _vacationRequest.VacationDays.ToString() + "일";
            RemainingDaysText.Text = _vacationRequest.RemainingDays.ToString() + "일";
            VacationReasonText.Text = _vacationRequest.VacationReason;
            ApprovalStatusText.Text = _vacationRequest.ApprovalStatus;

            // 승인 상태에 따라 버튼 활성화/비활성화
            if (_vacationRequest.ApprovalStatus == "승인" || _vacationRequest.ApprovalStatus == "거부")
            {
                ApproveButton.IsEnabled = false;
                RejectButton.IsEnabled = false;
            }
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"{_vacationRequest.EmployeeName}님의 휴가를 승인하시겠습니까?", "휴가 승인 확인",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _vacationRequest.ApprovalStatus = "승인";
                ApprovalStatusText.Text = "승인";
                ApproveButton.IsEnabled = false;
                RejectButton.IsEnabled = false;
                
                MessageBox.Show("휴가가 승인되었습니다.", "승인 완료", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"{_vacationRequest.EmployeeName}님의 휴가를 거부하시겠습니까?", "휴가 거부 확인",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _vacationRequest.ApprovalStatus = "거부";
                ApprovalStatusText.Text = "거부";
                ApproveButton.IsEnabled = false;
                RejectButton.IsEnabled = false;
                
                MessageBox.Show("휴가가 거부되었습니다.", "거부 완료", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
