using System;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.Inventory
{
    public partial class ItemManagementSubPage : UserControl
    {
        public ItemManagementSubPage()
        {
            InitializeComponent();
            // 기본적으로 품목등록 페이지 표시
            ShowRegistrationPage();
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            ShowRegistrationPage();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSearchPage();
        }

        private void ShowRegistrationPage()
        {
            var registrationPage = new ItemRegistrationPage();
            SubContentControl.Content = registrationPage;
        }

        private void ShowSearchPage()
        {
            var searchPage = new ItemSearchPage();
            SubContentControl.Content = searchPage;
        }
    }
}
