using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static jujin.MainWindow;

namespace jujin
{
    public partial class HRManagementWindow : Window
    {
        public HRManagementWindow(EmployeeInfoDto UserInfo)
        {
            InitializeComponent();
            this.DataContext = UserInfo;
        }

        public HRManagementWindow()
        {
            InitializeComponent();
        }

        // 메뉴 버튼 이벤트 핸들러들
        private void EmployeeInfoButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("직원 기본 정보 관리", 
                "• 직원 등록/수정: 사원번호, 이름, 부서, 직급, 입사일 등 기본 정보 입력\n" +
                "• 직원 조회: 조건(부서, 직급 등)에 따라 직원 목록을 검색하고 조회");
        }

        private void AttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("근태 관리", 
                "• 출퇴근 기록: 출근 및 퇴근 시간을 기록하고 조회\n" +
                "• 휴가 관리: 휴가 신청, 승인, 잔여 휴가일수 조회");
        }

        private void PayrollButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("급여 관리", 
                "• 급여 지급 내역: 직원의 기본급, 수당, 공제액 등 간단한 급여 내역을 입력하고 조회");
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

        // 뒤로가기 버튼
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // MainScreenWindow로 돌아가기
            var mainScreen = new MainScreenWindow(CurrentUserInfo);
            mainScreen.Show();
            this.Close();
        }
    }
}
