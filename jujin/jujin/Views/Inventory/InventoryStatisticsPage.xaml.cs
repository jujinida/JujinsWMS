using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.Inventory
{
    public partial class InventoryStatisticsPage : UserControl
    {
        public InventoryStatisticsPage()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // 샘플 데이터 로드 (실제로는 데이터베이스에서 가져옴)
            var sampleStatistics = new ObservableCollection<InventoryStatistics>
            {
                new InventoryStatistics { ItemCode = "ITEM001", ItemName = "스마트폰", Category = "완제품", DailyChangeRate = "+5.2%", MonthlyChangeRate = "+12.8%", QuarterlyChangeRate = "+25.4%", CurrentStock = 120, SafetyStock = 50, Status = "정상" },
                new InventoryStatistics { ItemCode = "ITEM002", ItemName = "배터리", Category = "원자재", DailyChangeRate = "-2.1%", MonthlyChangeRate = "-8.5%", QuarterlyChangeRate = "-15.2%", CurrentStock = 45, SafetyStock = 100, Status = "부족" },
                new InventoryStatistics { ItemCode = "ITEM003", ItemName = "LCD패널", Category = "원자재", DailyChangeRate = "+1.8%", MonthlyChangeRate = "+3.2%", QuarterlyChangeRate = "+8.7%", CurrentStock = 15, SafetyStock = 20, Status = "부족" },
                new InventoryStatistics { ItemCode = "ITEM004", ItemName = "케이스", Category = "소모품", DailyChangeRate = "+8.5%", MonthlyChangeRate = "+18.2%", QuarterlyChangeRate = "+32.1%", CurrentStock = 350, SafetyStock = 200, Status = "정상" },
                new InventoryStatistics { ItemCode = "ITEM005", ItemName = "충전기", Category = "소모품", DailyChangeRate = "+2.3%", MonthlyChangeRate = "+6.8%", QuarterlyChangeRate = "+14.5%", CurrentStock = 95, SafetyStock = 80, Status = "정상" },
                new InventoryStatistics { ItemCode = "ITEM006", ItemName = "메인보드", Category = "원자재", DailyChangeRate = "-1.5%", MonthlyChangeRate = "-5.2%", QuarterlyChangeRate = "-12.8%", CurrentStock = 25, SafetyStock = 30, Status = "부족" },
                new InventoryStatistics { ItemCode = "ITEM007", ItemName = "RAM", Category = "원자재", DailyChangeRate = "+3.7%", MonthlyChangeRate = "+9.1%", QuarterlyChangeRate = "+18.3%", CurrentStock = 65, SafetyStock = 50, Status = "정상" },
                new InventoryStatistics { ItemCode = "ITEM008", ItemName = "SSD", Category = "원자재", DailyChangeRate = "-0.8%", MonthlyChangeRate = "-2.1%", QuarterlyChangeRate = "-6.5%", CurrentStock = 38, SafetyStock = 40, Status = "부족" }
            };

            StatisticsDataGrid.ItemsSource = sampleStatistics;
        }

        private void StatisticsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowDetailPage();
        }

        private void DetailButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDetailPage();
        }

        private void ShowDetailPage()
        {
            var selectedItem = StatisticsDataGrid.SelectedItem as InventoryStatistics;
            if (selectedItem != null)
            {
                var detailPage = new InventoryStatisticsDetailPage(selectedItem);
                // 부모 컨테이너에 상세 페이지 표시
                var parent = this.Parent as ContentControl;
                if (parent != null)
                {
                    parent.Content = detailPage;
                }
            }
        }
    }

    // 재고 통계 정보 클래스
    public class InventoryStatistics
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public string DailyChangeRate { get; set; }
        public string MonthlyChangeRate { get; set; }
        public string QuarterlyChangeRate { get; set; }
        public int CurrentStock { get; set; }
        public int SafetyStock { get; set; }
        public string Status { get; set; }
    }
}
