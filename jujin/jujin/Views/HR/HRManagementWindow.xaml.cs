using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace jujin.Views.HR
{
    public partial class HRManagementPage : UserControl
    {
        public HRManagementPage()
        {
            InitializeComponent();
            // 기본적으로 직원 기본 정보 관리 페이지 표시
            ShowEmployeeManagement();
        }

        // 메뉴 버튼 이벤트 핸들러들
        private void EmployeeInfoButton_Click(object sender, RoutedEventArgs e)
        {
            ShowEmployeeManagement();
        }

        private void AttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAttendanceManagement();
        }

        private void PayrollButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPayrollManagement();
        }

        // 직원관리 페이지 표시
        private void ShowEmployeeManagement()
        {
            var employeeManagementPage = new EmployeeManagementSubPage();
            MainContentFrame.Content = employeeManagementPage;
        }

        // 근태관리 페이지 표시
        private void ShowAttendanceManagement()
        {
            var attendanceManagementPage = new AttendanceManagementSubPage();
            MainContentFrame.Content = attendanceManagementPage;
        }

        // 급여관리 페이지 표시
        private void ShowPayrollManagement()
        {
            var payrollManagementPage = new PayrollManagementPage();
            MainContentFrame.Content = payrollManagementPage;
        }

        // 페이지 표시 메서드
        private void ShowPage(string title, string description)
        {
            // 간단한 페이지 내용 생성
            var page = new Page();
            var stackPanel = new StackPanel();
            stackPanel.Margin = new Thickness(20);
            
            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80))
            };
            
            var descText = new TextBlock
            {
                Text = description,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
                LineHeight = 24
            };
            
            stackPanel.Children.Add(titleText);
            stackPanel.Children.Add(descText);
            page.Content = stackPanel;
            
            MainContentFrame.Navigate(page);
        }

    }
}
