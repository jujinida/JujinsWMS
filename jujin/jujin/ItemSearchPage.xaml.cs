using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace jujin
{
    public partial class ItemSearchPage : UserControl
    {
        public ItemSearchPage()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // 샘플 데이터 로드 (실제로는 데이터베이스에서 가져옴)
            var sampleItems = new ObservableCollection<ItemInfo>
            {
                new ItemInfo { ItemCode = "ITEM001", ItemName = "스마트폰", Specification = "6.1인치", Unit = "개", SafetyStock = 50, Category = "완제품", Supplier = "삼성전자" },
                new ItemInfo { ItemCode = "ITEM002", ItemName = "배터리", Specification = "3000mAh", Unit = "개", SafetyStock = 100, Category = "원자재", Supplier = "LG화학" },
                new ItemInfo { ItemCode = "ITEM003", ItemName = "LCD패널", Specification = "OLED", Unit = "개", SafetyStock = 200, Category = "반제품", Supplier = "LG디스플레이" },
                new ItemInfo { ItemCode = "ITEM004", ItemName = "케이스", Specification = "실리콘", Unit = "개", SafetyStock = 300, Category = "소모품", Supplier = "케이스코리아" },
                new ItemInfo { ItemCode = "ITEM005", ItemName = "충전기", Specification = "USB-C", Unit = "개", SafetyStock = 150, Category = "소모품", Supplier = "애플" }
            };

            ItemDataGrid.ItemsSource = sampleItems;
        }
    }

    // 품목 정보 클래스
    public class ItemInfo
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Specification { get; set; }
        public string Unit { get; set; }
        public int SafetyStock { get; set; }
        public string Category { get; set; }
        public string Supplier { get; set; }
    }
}
