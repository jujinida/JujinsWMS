using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.HR
{
    public partial class AttendanceRecordPage : UserControl
    {
        public AttendanceRecordPage()
        {
            InitializeComponent();
            // 오늘 날짜로 초기화
            SelectedDatePicker.SelectedDate = DateTime.Today;
            LoadSampleData();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            var selectedDate = SelectedDatePicker.SelectedDate ?? DateTime.Today;
            var dayOfWeek = selectedDate.DayOfWeek;
            
            // 샘플 데이터 로드 (실제로는 데이터베이스에서 가져옴)
            var sampleAttendance = new ObservableCollection<AttendanceRecord>
            {
                new AttendanceRecord { EmployeeId = "EMP001", EmployeeName = "김철수", DepartmentName = "개발팀", Position = "대리", 
                    CheckInTime = "09:15", CheckOutTime = "18:30", WorkHours = "9시간 15분", LateCount = 2, AbsentCount = 0, Status = "지각" },
                new AttendanceRecord { EmployeeId = "EMP002", EmployeeName = "이영희", DepartmentName = "마케팅팀", Position = "과장", 
                    CheckInTime = "08:45", CheckOutTime = "18:00", WorkHours = "9시간 15분", LateCount = 0, AbsentCount = 0, Status = "정상" },
                new AttendanceRecord { EmployeeId = "EMP003", EmployeeName = "박민수", DepartmentName = "인사팀", Position = "차장", 
                    CheckInTime = "09:00", CheckOutTime = "18:15", WorkHours = "9시간 15분", LateCount = 1, AbsentCount = 0, Status = "정상" },
                new AttendanceRecord { EmployeeId = "EMP004", EmployeeName = "최지영", DepartmentName = "재무팀", Position = "부장", 
                    CheckInTime = "08:30", CheckOutTime = "17:45", WorkHours = "9시간 15분", LateCount = 0, AbsentCount = 0, Status = "정상" },
                new AttendanceRecord { EmployeeId = "EMP005", EmployeeName = "정현우", DepartmentName = "영업팀", Position = "사원", 
                    CheckInTime = "09:20", CheckOutTime = "18:45", WorkHours = "9시간 25분", LateCount = 3, AbsentCount = 0, Status = "지각" },
                new AttendanceRecord { EmployeeId = "EMP006", EmployeeName = "한소영", DepartmentName = "기획팀", Position = "대리", 
                    CheckInTime = "09:00", CheckOutTime = "18:00", WorkHours = "9시간", LateCount = 0, AbsentCount = 0, Status = "정상" },
                new AttendanceRecord { EmployeeId = "EMP007", EmployeeName = "윤태호", DepartmentName = "개발팀", Position = "과장", 
                    CheckInTime = "08:45", CheckOutTime = "18:30", WorkHours = "9시간 45분", LateCount = 0, AbsentCount = 0, Status = "정상" },
                new AttendanceRecord { EmployeeId = "EMP008", EmployeeName = "강미래", DepartmentName = "마케팅팀", Position = "사원", 
                    CheckInTime = "", CheckOutTime = "", WorkHours = "0시간", LateCount = 0, AbsentCount = 1, Status = "무단결근" }
            };

            // 주말인 경우 출퇴근 시간을 비워둠
            if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            {
                foreach (var record in sampleAttendance)
                {
                    record.CheckInTime = "";
                    record.CheckOutTime = "";
                    record.WorkHours = "0시간";
                    record.Status = "주말";
                }
            }

            AttendanceDataGrid.ItemsSource = sampleAttendance;
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
}
