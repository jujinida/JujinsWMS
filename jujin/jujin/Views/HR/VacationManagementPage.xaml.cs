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
    public partial class VacationManagementPage : UserControl
    {
        private readonly HttpClient httpClient;
        private ObservableCollection<VacationRequest> vacationRequests;
        private ObservableCollection<VacationRequest> allVacationRequests; // 전체 데이터 저장용

        public VacationManagementPage()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            vacationRequests = new ObservableCollection<VacationRequest>();
            allVacationRequests = new ObservableCollection<VacationRequest>();
            VacationDataGrid.ItemsSource = vacationRequests;
            
            Loaded += VacationManagementPage_Loaded;
        }

        private async void VacationManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadVacationData();
        }

        private async Task LoadVacationData()
        {
            try
            {
                var response = await httpClient.GetAsync("http://localhost:5185/api/hr/vacation-requests");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var vacationDtos = JsonSerializer.Deserialize<VacationRequestDto[]>(json, options);

                    allVacationRequests.Clear(); // 전체 데이터 초기화
                    
                    if (vacationDtos != null)
                    {
                        foreach (var dto in vacationDtos)
                        {
                            var vacationRequest = new VacationRequest
                            {
                                RequestId = dto.RequestId,
                                EmployeeId = dto.EmployeeId.ToString(),
                                EmployeeName = dto.EmployeeName,
                                DepartmentName = GetDepartmentName(dto.DepartmentId),
                                Position = dto.Position,
                                VacationReason = dto.Reason,
                                StartDate = dto.StartDate,
                                EndDate = dto.EndDate,
                                VacationDays = dto.VacationDays,
                                TotalVacationDays = dto.TotalVacationDays,
                                RemainingVacationDays = dto.RemainingVacationDays,
                                ApprovalStatus = dto.ApprovalStatus
                            };
                            
                            allVacationRequests.Add(vacationRequest);
                        }
                    }
                    
                    ApplyFilters(); // 필터 적용
                }
                else
                {
                    MessageBox.Show("휴가 신청 목록을 불러오는데 실패했습니다.", "오류", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"휴가 신청 목록을 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", 
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
                _ => "미지정"
            };
        }

        private void VacationDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowVacationDetailDialog();
        }

        private async void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedVacation = VacationDataGrid.SelectedItem as VacationRequest;
            if (selectedVacation != null)
            {
                // 이미 승인된 상태인지 확인
                if (selectedVacation.ApprovalStatus == "승인")
                {
                    MessageBox.Show("이미 승인되었습니다.", "알림", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show($"{selectedVacation.EmployeeName}님의 휴가를 승인하시겠습니까?", "휴가 승인 확인",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var httpClient = new HttpClient();
                        var response = await httpClient.PutAsync($"http://localhost:5185/api/hr/vacation-requests/{selectedVacation.RequestId}/approve", null);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            selectedVacation.ApprovalStatus = "승인";
                            MessageBox.Show("휴가가 승인되었습니다.", "승인 완료", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            
                            // 목록 다시 불러오기
                            await LoadVacationData();
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"승인 처리 중 오류가 발생했습니다: {errorContent}", "오류", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"승인 처리 중 오류가 발생했습니다: {ex.Message}", "오류", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedVacation = VacationDataGrid.SelectedItem as VacationRequest;
            if (selectedVacation != null)
            {
                // 이미 거부된 상태인지 확인
                if (selectedVacation.ApprovalStatus == "거부")
                {
                    MessageBox.Show("이미 거부되었습니다.", "알림", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show($"{selectedVacation.EmployeeName}님의 휴가를 거부하시겠습니까?", "휴가 거부 확인",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var httpClient = new HttpClient();
                        var response = await httpClient.PutAsync($"http://localhost:5185/api/hr/vacation-requests/{selectedVacation.RequestId}/reject", null);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            selectedVacation.ApprovalStatus = "거부";
                            MessageBox.Show("휴가가 거부되었습니다.", "거부 완료", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            
                            // 목록 다시 불러오기
                            await LoadVacationData();
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"거부 처리 중 오류가 발생했습니다: {errorContent}", "오류", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"거부 처리 중 오류가 발생했습니다: {ex.Message}", "오류", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            vacationRequests.Clear();

            var filteredRequests = allVacationRequests.Where(request =>
            {
                // 직원명 필터
                var employeeNameFilter = EmployeeNameFilter.Text.Trim();
                if (!string.IsNullOrEmpty(employeeNameFilter) && 
                    !request.EmployeeName.Contains(employeeNameFilter, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // 부서 필터
                var selectedDepartment = DepartmentFilter.SelectedItem as ComboBoxItem;
                if (selectedDepartment != null && selectedDepartment.Tag != null)
                {
                    var departmentId = int.Parse(selectedDepartment.Tag.ToString());
                    if (departmentId != 0)
                    {
                        var expectedDepartmentName = GetDepartmentName(departmentId);
                        if (request.DepartmentName != expectedDepartmentName)
                        {
                            return false;
                        }
                    }
                }

                // 승인상태 필터
                var selectedStatus = ApprovalStatusFilter.SelectedItem as ComboBoxItem;
                if (selectedStatus != null && selectedStatus.Content.ToString() != "전체")
                {
                    if (request.ApprovalStatus != selectedStatus.Content.ToString())
                    {
                        return false;
                    }
                }

                return true;
            });

            foreach (var request in filteredRequests)
            {
                vacationRequests.Add(request);
            }
        }
    }

    // 휴가 신청 클래스
    public class VacationRequest
    {
        public int RequestId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string Position { get; set; }
        public string VacationReason { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public decimal VacationDays { get; set; }
        public int TotalVacationDays { get; set; }
        public decimal RemainingVacationDays { get; set; }
        public string ApprovalStatus { get; set; }
    }

    // 백엔드 DTO 클래스
    public class VacationRequestDto
    {
        public int RequestId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int DepartmentId { get; set; }
        public string Position { get; set; }
        public string Reason { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public decimal VacationDays { get; set; }
        public bool IsHalfDay { get; set; }
        public decimal RemainingVacationDays { get; set; }
        public int TotalVacationDays { get; set; }
        public string ApprovalStatus { get; set; }
    }
}
