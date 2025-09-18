using System;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.HR
{
    public partial class AttendanceManagementSubPage : UserControl
    {
        public AttendanceManagementSubPage()
        {
            InitializeComponent();
            // 기본적으로 출퇴근 기록 페이지 표시
            ShowAttendanceRecordPage();
        }

        private void AttendanceRecordButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAttendanceRecordPage();
        }

        private void VacationManagementButton_Click(object sender, RoutedEventArgs e)
        {
            ShowVacationManagementPage();
        }

        private void ShowAttendanceRecordPage()
        {
            var attendanceRecordPage = new AttendanceRecordPage();
            SubContentControl.Content = attendanceRecordPage;
        }

        private void ShowVacationManagementPage()
        {
            var vacationManagementPage = new VacationManagementPage();
            SubContentControl.Content = vacationManagementPage;
        }
    }
}
