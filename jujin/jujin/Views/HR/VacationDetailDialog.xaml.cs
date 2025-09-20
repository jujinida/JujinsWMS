using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace jujin.Views.HR
{
    public partial class VacationDetailDialog : Window
    {
        private VacationRequest _vacationRequest;
        private HttpClient _httpClient;

        public VacationDetailDialog(VacationRequest vacationRequest)
        {
            InitializeComponent();
            _vacationRequest = vacationRequest;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5185/api/hr/");
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
            RemainingDaysText.Text = _vacationRequest.RemainingVacationDays.ToString() + "일";
            VacationReasonText.Text = _vacationRequest.VacationReason;
            ApprovalStatusText.Text = _vacationRequest.ApprovalStatus;

            // 승인 상태에 따라 버튼 활성화/비활성화
            if (_vacationRequest.ApprovalStatus == "승인" || _vacationRequest.ApprovalStatus == "거부")
            {
                ApproveButton.IsEnabled = false;
                RejectButton.IsEnabled = false;
            }
        }

        private async void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"{_vacationRequest.EmployeeName}님의 휴가를 승인하시겠습니까?", "휴가 승인 확인",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    ApproveButton.IsEnabled = false;
                    RejectButton.IsEnabled = false;

                    var response = await _httpClient.PutAsync($"vacation-requests/{_vacationRequest.RequestId}/approve", null);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        _vacationRequest.ApprovalStatus = "승인";
                        ApprovalStatusText.Text = "승인";
                        
                        MessageBox.Show("휴가가 승인되었습니다.", "승인 완료", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"승인 처리 중 오류가 발생했습니다: {errorContent}", "오류", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        
                        ApproveButton.IsEnabled = true;
                        RejectButton.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"승인 처리 중 오류가 발생했습니다: {ex.Message}", "오류", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    ApproveButton.IsEnabled = true;
                    RejectButton.IsEnabled = true;
                }
            }
        }

        private async void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"{_vacationRequest.EmployeeName}님의 휴가를 거부하시겠습니까?", "휴가 거부 확인",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    ApproveButton.IsEnabled = false;
                    RejectButton.IsEnabled = false;

                    var response = await _httpClient.PutAsync($"vacation-requests/{_vacationRequest.RequestId}/reject", null);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        _vacationRequest.ApprovalStatus = "거부";
                        ApprovalStatusText.Text = "거부";
                        
                        MessageBox.Show("휴가가 거부되었습니다.", "거부 완료", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"거부 처리 중 오류가 발생했습니다: {errorContent}", "오류", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        
                        ApproveButton.IsEnabled = true;
                        RejectButton.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"거부 처리 중 오류가 발생했습니다: {ex.Message}", "오류", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    ApproveButton.IsEnabled = true;
                    RejectButton.IsEnabled = true;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
