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
            
            // 기본적으로 재고관리 페이지 표시
            ShowInventoryManagement();
        }

        public MainScreenWindow()
        {
            InitializeComponent();
            // 기본적으로 재고관리 페이지 표시
            ShowInventoryManagement();
        }

        // 헤더 버튼 이벤트 핸들러들
        private void InventoryManagementButton_Click(object sender, RoutedEventArgs e)
        {
            ShowInventoryManagement();
        }

        private void HRManagementButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHRManagement();
        }

        // 컴포넌트 전환 메서드들
        private void ShowInventoryManagement()
        {
            var inventoryPage = new InventoryManagementPage();
            MainContentControl.Content = inventoryPage;
        }

        private void ShowHRManagement()
        {
            var hrPage = new HRManagementPage();
            MainContentControl.Content = hrPage;
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