using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace jujin.Views.HR
{
    public partial class MonthlyAttendanceCalendarWindow : Window
    {
        private readonly HttpClient httpClient;
        private readonly int employeeId;
        private readonly string employeeName;
        private DateTime currentMonth;
        private List<MonthlyAttendanceRecordDto> attendanceRecords;

        public MonthlyAttendanceCalendarWindow(int employeeId, string employeeName)
        {
            InitializeComponent();
            httpClient = new HttpClient();
            this.employeeId = employeeId;
            this.employeeName = employeeName;
            this.currentMonth = DateTime.Now;
            
            EmployeeNameText.Text = $"- {employeeName}";
            attendanceRecords = new List<MonthlyAttendanceRecordDto>();
            
            Loaded += MonthlyAttendanceCalendarWindow_Loaded;
        }

        private async void MonthlyAttendanceCalendarWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMonthlyAttendanceData();
            UpdateCalendarDisplay();
        }

        private async Task LoadMonthlyAttendanceData()
        {
            try
            {
                var yearMonth = currentMonth.ToString("yyyy-MM");
                var response = await httpClient.GetAsync($"http://localhost:5185/api/hr/attendance-records/{employeeId}/monthly?yearMonth={yearMonth}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    attendanceRecords = JsonSerializer.Deserialize<List<MonthlyAttendanceRecordDto>>(json, options) ?? new List<MonthlyAttendanceRecordDto>();
                }
                else
                {
                    MessageBox.Show("월별 출근 기록을 불러오는데 실패했습니다.", "오류", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"월별 출근 기록을 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCalendarDisplay()
        {
            CurrentMonthText.Text = currentMonth.ToString("yyyy년 M월");
            
            // 기존 달력 셀들 제거 (헤더 제외)
            for (int i = CalendarGrid.RowDefinitions.Count - 1; i >= 1; i--)
            {
                CalendarGrid.RowDefinitions.RemoveAt(i);
            }
            
            // 새로운 행들 추가
            for (int i = 1; i <= 6; i++)
            {
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
            }
            
            // 달력 데이터 생성
            var firstDayOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            
            int day = 1;
            
            // 달력 셀들 생성
            for (int row = 1; row <= 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    var border = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(1)
                    };
                    
                    var stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    
                    if (row == 1 && col < firstDayOfWeek)
                    {
                        // 이전 달의 날짜들 (빈 셀)
                        border.Background = Brushes.LightGray;
                    }
                    else if (day <= lastDayOfMonth.Day)
                    {
                        // 현재 달의 날짜
                        var currentDay = day; // 클로저 문제 해결을 위해 로컬 변수 사용
                        
                        var dayText = new TextBlock
                        {
                            Text = day.ToString(),
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        
                        var attendanceInfo = GetAttendanceInfoForDay(currentDay);
                        var statusText = new TextBlock
                        {
                            Text = attendanceInfo.Status,
                            FontSize = 10,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Foreground = GetStatusColor(attendanceInfo.Status)
                        };
                        
                        stackPanel.Children.Add(dayText);
                        stackPanel.Children.Add(statusText);
                        
                        // 클릭 이벤트 추가 - 올바른 날짜로 수정
                        border.MouseLeftButtonDown += (s, e) => ShowDayDetail(currentDay, attendanceInfo);
                        border.Cursor = System.Windows.Input.Cursors.Hand;
                        
                        day++;
                    }
                    else
                    {
                        // 다음 달의 날짜들 (빈 셀)
                        border.Background = Brushes.LightGray;
                    }
                    
                    border.Child = stackPanel;
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                    CalendarGrid.Children.Add(border);
                }
            }
        }

        private AttendanceInfo GetAttendanceInfoForDay(int day)
        {
            try
            {
                var targetDate = new DateTime(currentMonth.Year, currentMonth.Month, day).ToString("yyyy-MM-dd");
                var record = attendanceRecords.FirstOrDefault(r => r.RecordDate == targetDate);
                
                if (record == null)
                {
                    // 주말 체크
                    var date = new DateTime(currentMonth.Year, currentMonth.Month, day);
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        return new AttendanceInfo { Status = "주말", CheckInTime = "", CheckOutTime = "", WorkHours = "0시간" };
                    }
                    return new AttendanceInfo { Status = "미출근", CheckInTime = "", CheckOutTime = "", WorkHours = "0시간" };
                }
                
                return new AttendanceInfo
                {
                    Status = record.AttendanceStatus,
                    CheckInTime = record.CheckInTime,
                    CheckOutTime = record.CheckOutTime,
                    WorkHours = FormatWorkHours(record.WorkHours, record.WorkMinutes)
                };
            }
            catch (ArgumentOutOfRangeException)
            {
                // 잘못된 날짜인 경우 (예: 2월 30일)
                return new AttendanceInfo { Status = "오류", CheckInTime = "", CheckOutTime = "", WorkHours = "0시간" };
            }
        }

        private string FormatWorkHours(int hours, int minutes)
        {
            if (hours == 0 && minutes == 0)
                return "0시간";
            
            if (minutes == 0)
                return $"{hours}시간";
            
            return $"{hours}시간 {minutes}분";
        }

        private Brush GetStatusColor(string status)
        {
            return status switch
            {
                "정상" => Brushes.Green,
                "지각" => Brushes.Orange,
                "결근" => Brushes.Red,
                "주말" => Brushes.Gray,
                "미출근" => Brushes.DarkRed,
                _ => Brushes.Black
            };
        }

        private void ShowDayDetail(int day, AttendanceInfo info)
        {
            try
            {
                // 날짜 유효성 검사
                if (day < 1 || day > 31)
                {
                    MessageBox.Show("잘못된 날짜입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 해당 월의 마지막 날짜 확인
                var lastDayOfMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
                if (day > lastDayOfMonth)
                {
                    MessageBox.Show("잘못된 날짜입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var date = new DateTime(currentMonth.Year, currentMonth.Month, day);
                var message = $"{date:yyyy년 M월 d일} ({GetDayOfWeekKorean(date.DayOfWeek)})\n\n";
                message += $"상태: {info.Status}\n";
                message += $"출근시간: {info.CheckInTime}\n";
                message += $"퇴근시간: {info.CheckOutTime}\n";
                message += $"근무시간: {info.WorkHours}";
                
                MessageBox.Show(message, "출근 상세 정보", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("잘못된 날짜입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetDayOfWeekKorean(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => "일요일",
                DayOfWeek.Monday => "월요일",
                DayOfWeek.Tuesday => "화요일",
                DayOfWeek.Wednesday => "수요일",
                DayOfWeek.Thursday => "목요일",
                DayOfWeek.Friday => "금요일",
                DayOfWeek.Saturday => "토요일",
                _ => ""
            };
        }

        private async void PrevMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentMonth = currentMonth.AddMonths(-1);
            await LoadMonthlyAttendanceData();
            UpdateCalendarDisplay();
        }

        private async void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentMonth = currentMonth.AddMonths(1);
            await LoadMonthlyAttendanceData();
            UpdateCalendarDisplay();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class AttendanceInfo
    {
        public string Status { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public string WorkHours { get; set; }
    }

    public class MonthlyAttendanceRecordDto
    {
        public string RecordDate { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public int WorkHours { get; set; }
        public int WorkMinutes { get; set; }
        public string AttendanceStatus { get; set; }
    }
}
