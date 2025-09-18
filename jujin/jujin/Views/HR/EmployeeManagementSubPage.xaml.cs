using System;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.HR
{
    public partial class EmployeeManagementSubPage : UserControl
    {
        public EmployeeManagementSubPage()
        {
            InitializeComponent();
            // 기본적으로 직원등록 페이지 표시
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
            var registrationPage = new EmployeeRegistrationPage();
            SubContentControl.Content = registrationPage;
        }

        private void ShowSearchPage()
        {
            var searchPage = new EmployeeSearchPage();
            SubContentControl.Content = searchPage;
        }
    }
}
