using System;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.Inventory
{
    public partial class ShippingManagementSubPage : UserControl
    {
        public ShippingManagementSubPage()
        {
            InitializeComponent();
            // 기본적으로 출고등록 페이지 표시
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
            var registrationPage = new ShippingRegistrationPage();
            SubContentControl.Content = registrationPage;
        }

        private void ShowHistoryPage()
        {
            var historyPage = new ShippingHistoryPage();
            SubContentControl.Content = historyPage;
        }
    }
}
