using System;
using System.Diagnostics;
using System.Windows;
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

        private void LogisticsButton_Click(object sender, RoutedEventArgs e)
        {
            // 더미 함수: 아무 동작 없음
        }

        private void HRButton_Click(object sender, RoutedEventArgs e)
        {
            // 더미 함수: 아무 동작 없음
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