using System;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.Inventory
{
    public partial class ReceivingManagementSubPage : UserControl
    {
        public ReceivingManagementSubPage()
        {
            InitializeComponent();
            // 기본적으로 입고등록 페이지 표시
            ShowRegistrationPage();
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            ShowRegistrationPage();
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHistoryPage();
        }

        private void ShowRegistrationPage()
        {
            var registrationPage = new ReceivingRegistrationPage();
            SubContentControl.Content = registrationPage;
        }

        private void ShowHistoryPage()
        {
            var historyPage = new ReceivingHistoryPage();
            SubContentControl.Content = historyPage;
        }
    }
}
