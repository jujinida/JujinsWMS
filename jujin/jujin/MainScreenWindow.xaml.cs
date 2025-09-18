using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static jujin.MainWindow;
using jujin;

namespace jujin
{
    public partial class MainScreenWindow : Window
    {
        public MainScreenWindow(EmployeeInfoDto UserInfo)
        {
            
            InitializeComponent();
            // userInfo 객체에서 필요한 값들을 사용
            this.DataContext = UserInfo; // WPF에서 데이터 바인딩을 위한 일반적인 방법
        }

        public MainScreenWindow()
        {
            InitializeComponent();
        }

        // 메뉴 버튼 이벤트 핸들러들
        private void ItemManagementButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("품목관리", "품목 등록, 조회/수정 기능이 포함된 페이지입니다.");
        }

        private void ReceivingManagementButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("입고관리", "입고 등록, 입고 내역 조회 기능이 포함된 페이지입니다.");
        }

        private void ShippingManagementButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("출고관리", "출고 등록, 출고 내역 조회 기능이 포함된 페이지입니다.");
        }

        private void InventoryStatusButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("재고현황", "현재 재고 조회, 재고 위치 관리 기능이 포함된 페이지입니다.");
        }

        private void InventoryStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("재고통계", "일별/월별/분기별 입출고 현황 분석 기능이 포함된 페이지입니다.");
        }

        // 헤더 버튼 이벤트 핸들러들
        private void InventoryManagementButton_Click(object sender, RoutedEventArgs e)
        {
            // 재고관리 메뉴 표시 (현재 사이드바 메뉴들)
            ShowInventoryMenu();
        }

        private void HRManagementButton_Click(object sender, RoutedEventArgs e)
        {
            // 인사관리 창 열기
            var hrWindow = new HRManagementWindow(CurrentUserInfo);
            hrWindow.Show();
            this.Close();
        }

        // 재고관리 메뉴 표시
        private void ShowInventoryMenu()
        {
            // 사이드바 메뉴들을 보이게 하고 메인 콘텐츠를 숨김
            var sidebar = (Grid)this.FindName("LeftSidebarMenu");
            var mainContent = (Grid)this.FindName("MainContentArea");
            
            if (sidebar != null) sidebar.Visibility = Visibility.Visible;
            if (mainContent != null) mainContent.Visibility = Visibility.Visible;
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
                Foreground = new SolidColorBrush(Color.FromRgb(46, 52, 64))
            };
            
            var descText = new TextBlock
            {
                Text = description,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(76, 86, 106))
            };
            
            stackPanel.Children.Add(titleText);
            stackPanel.Children.Add(descText);
            page.Content = stackPanel;
            
            MainContentFrame.Navigate(page);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // 로그아웃 확인 메시지
            MessageBoxResult result = MessageBox.Show("로그아웃 하시겠습니까?", "로그아웃 확인", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                // 토큰과 사용자 정보 정리
                MainWindow.CurrentToken = null;
                MainWindow.CurrentUserInfo = null;
                
                // MainWindow로 되돌아가기
                var mainWindow = new MainWindow();
                mainWindow.Show();
                
                // 현재 창 닫기
                this.Close();
            }
        }
    }
}