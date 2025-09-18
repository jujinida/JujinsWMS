using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace jujin
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

        // 품목관리 페이지 표시 (품목등록과 조회/수정을 포함한 서브메뉴)
        private void ShowItemManagement()
        {
            var itemManagementPage = new ItemManagementSubPage();
            MainContentControl.Content = itemManagementPage;
        }

        // 일반 페이지 표시 메서드
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
            
            MainContentControl.Content = page;
        }
    }
}
