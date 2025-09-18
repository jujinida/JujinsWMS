using System;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.Inventory
{
    public partial class InventoryStatisticsDetailPage : UserControl
    {
        private InventoryStatistics _statistics;

        public InventoryStatisticsDetailPage(InventoryStatistics statistics)
        {
            InitializeComponent();
            _statistics = statistics;
            this.DataContext = statistics;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // 부모 컨테이너에 메인 페이지로 돌아가기
            var parent = this.Parent as ContentControl;
            if (parent != null)
            {
                var mainPage = new InventoryStatisticsPage();
                parent.Content = mainPage;
            }
        }
    }
}
