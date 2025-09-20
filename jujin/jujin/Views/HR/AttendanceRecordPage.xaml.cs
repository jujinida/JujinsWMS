using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.HR
{
    public partial class AttendanceRecordPage : UserControl
    {
        private readonly HttpClient httpClient;
        private ObservableCollection<AttendanceRecord> allAttendanceRecords; // 전체 데이터 저장용

        public AttendanceRecordPage()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            allAttendanceRecords = new ObservableCollection<AttendanceRecord>();
            // 오늘 날짜로 초기화
            SelectedDatePicker.SelectedDate = DateTime.Today;
            Loaded += AttendanceRecordPage_Loaded;
        }

        private async void AttendanceRecordPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAttendanceData();
        }

        private async void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadAttendanceData();
        }

        private async Task LoadAttendanceData()
        {
            try
            {
                var selectedDate = SelectedDatePicker.SelectedDate ?? DateTime.Today;
                var dateString = selectedDate.ToString("yyyy-MM-dd");

                var response = await httpClient.GetAsync($"http://localhost:5185/api/hr/attendance-records?date={dateString}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var attendanceDtos = JsonSerializer.Deserialize<List<AttendanceRecordDto>>(json, options);

                    allAttendanceRecords.Clear(); // 전체 데이터 초기화
                    foreach (var dto in attendanceDtos)
                    {
                        allAttendanceRecords.Add(new AttendanceRecord
                        {
                            EmployeeId = dto.EmployeeId.ToString(),
                            EmployeeName = dto.EmployeeName,
                            DepartmentName = GetDepartmentName(dto.DepartmentId),
                            Position = dto.Position,
                            CheckInTime = dto.CheckInTime,
                            CheckOutTime = dto.CheckOutTime,
                            WorkHours = FormatWorkHours(dto.WorkHours, dto.WorkMinutes),
                            LateCount = dto.LateCount,
                            AbsentCount = dto.AbsentWithoutLeaveCount,
                            Status = GetAttendanceStatus(dto.CheckInTime, dto.CheckOutTime, dto.LateCount, dto.AbsentWithoutLeaveCount)
                        });
                    }

                    ApplyFilters(); // 필터 적용
                }
                else
                {
                    MessageBox.Show("출근 기록을 불러오는데 실패했습니다.", "오류", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"출근 기록을 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", 
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

        private string FormatWorkHours(int hours, int minutes)
        {
            if (hours == 0 && minutes == 0)
                return "0시간";
            
            if (minutes == 0)
                return $"{hours}시간";
            
            return $"{hours}시간 {minutes}분";
        }

        private string GetAttendanceStatus(string checkInTime, string checkOutTime, int lateCount, int absentCount)
        {
            if (absentCount > 0)
                return "무단결근";
            
            if (lateCount > 0)
                return "지각";
            
            if (string.IsNullOrEmpty(checkInTime) && string.IsNullOrEmpty(checkOutTime))
                return "미출근";
            
            return "정상";
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is AttendanceRecord record)
            {
                // EmployeeId를 int로 변환
                if (int.TryParse(record.EmployeeId, out int employeeId))
                {
                    var calendarWindow = new MonthlyAttendanceCalendarWindow(employeeId, record.EmployeeName);
                    calendarWindow.Owner = Window.GetWindow(this);
                    calendarWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("직원 ID를 확인할 수 없습니다.", "오류", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filteredRecords = allAttendanceRecords.Where(record =>
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
                    if (departmentId != 0)
                    {
                        var expectedDepartmentName = GetDepartmentName(departmentId);
                        if (record.DepartmentName != expectedDepartmentName)
                        {
                            return false;
                        }
                    }
                }

                return true;
            });

            var attendanceRecords = new ObservableCollection<AttendanceRecord>(filteredRecords);
            AttendanceDataGrid.ItemsSource = attendanceRecords;
        }
    }

    // 출퇴근 기록 클래스
    public class AttendanceRecord
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string Position { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public string WorkHours { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public string Status { get; set; }
    }

    // 백엔드에서 받아오는 DTO
    public class AttendanceRecordDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int DepartmentId { get; set; }
        public string Position { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public int WorkHours { get; set; }
        public int WorkMinutes { get; set; }
        public int LateCount { get; set; }
        public int AbsentWithoutLeaveCount { get; set; }
    }
}
