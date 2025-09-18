using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace jujin.Views.Inventory
{
    public partial class InventoryManagementPage : UserControl
    {
        public InventoryManagementPage()
        {
            InitializeComponent();
            // 기본적으로 품목관리 페이지 표시
            ShowItemManagement();
        }

        // 메뉴 버튼 이벤트 핸들러들
        private void ItemManagementButton_Click(object sender, RoutedEventArgs e)
        {
            ShowItemManagement();
        }

        private void ReceivingManagementButton_Click(object sender, RoutedEventArgs e)
        {
            ShowReceivingManagement();
        }

        private void ShippingManagementButton_Click(object sender, RoutedEventArgs e)
        {
            ShowShippingManagement();
        }

        private void InventoryStatusButton_Click(object sender, RoutedEventArgs e)
        {
            ShowInventoryStatus();
        }

        private void InventoryStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowInventoryStatistics();
        }

        // 품목관리 페이지 표시 (품목등록과 조회/수정을 포함한 서브메뉴)
        private void ShowItemManagement()
        {
            var itemManagementPage = new ItemManagementSubPage();
            MainContentControl.Content = itemManagementPage;
        }

        // 입고관리 페이지 표시 (입고등록과 입고내역조회를 포함한 서브메뉴)
        private void ShowReceivingManagement()
        {
            var receivingManagementPage = new ReceivingManagementSubPage();
            MainContentControl.Content = receivingManagementPage;
        }

        // 출고관리 페이지 표시 (출고등록과 출고내역조회를 포함한 서브메뉴)
        private void ShowShippingManagement()
        {
            var shippingManagementPage = new ShippingManagementSubPage();
            MainContentControl.Content = shippingManagementPage;
        }

        // 재고현황 페이지 표시 (현재 재고 조회)
        private void ShowInventoryStatus()
        {
            var inventoryStatusPage = new InventoryStatusPage();
            MainContentControl.Content = inventoryStatusPage;
        }

        // 재고통계 페이지 표시 (통계 분석)
        private void ShowInventoryStatistics()
        {
            var inventoryStatisticsPage = new InventoryStatisticsPage();
            MainContentControl.Content = inventoryStatisticsPage;
        }

        // 일반 페이지 표시 메서드
        private void ShowPage(string title, string description)
        {
            // 간단한 페이지 내용 생성
            var userControl = new UserControl();
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
            userControl.Content = stackPanel;
            
            MainContentControl.Content = userControl;
        }
    }
}
