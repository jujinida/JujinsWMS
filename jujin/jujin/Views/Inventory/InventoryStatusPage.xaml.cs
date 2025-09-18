using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.Inventory
{
    public partial class InventoryStatusPage : UserControl
    {
        public InventoryStatusPage()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // 샘플 데이터 로드 (실제로는 데이터베이스에서 가져옴)
            var sampleInventory = new ObservableCollection<InventoryItem>
            {
                new InventoryItem { ItemCode = "ITEM001", ItemName = "스마트폰", Specification = "갤럭시 S24", Unit = "개", SafetyStock = 50, CurrentStock = 120, Category = "완제품", Supplier = "삼성전자", Location = "A-01-01", Status = "정상" },
                new InventoryItem { ItemCode = "ITEM002", ItemName = "배터리", Specification = "5000mAh", Unit = "개", SafetyStock = 100, CurrentStock = 45, Category = "원자재", Supplier = "LG화학", Location = "B-02-03", Status = "부족" },
                new InventoryItem { ItemCode = "ITEM003", ItemName = "LCD패널", Specification = "6.1인치", Unit = "개", SafetyStock = 20, CurrentStock = 15, Category = "원자재", Supplier = "LG디스플레이", Location = "C-01-05", Status = "부족" },
                new InventoryItem { ItemCode = "ITEM004", ItemName = "케이스", Specification = "실리콘", Unit = "개", SafetyStock = 200, CurrentStock = 350, Category = "소모품", Supplier = "케이스코리아", Location = "D-03-02", Status = "정상" },
                new InventoryItem { ItemCode = "ITEM005", ItemName = "충전기", Specification = "USB-C", Unit = "개", SafetyStock = 80, CurrentStock = 95, Category = "소모품", Supplier = "애플", Location = "E-01-04", Status = "정상" },
                new InventoryItem { ItemCode = "ITEM006", ItemName = "메인보드", Specification = "B550", Unit = "개", SafetyStock = 30, CurrentStock = 25, Category = "원자재", Supplier = "ASUS", Location = "F-02-01", Status = "부족" },
                new InventoryItem { ItemCode = "ITEM007", ItemName = "RAM", Specification = "16GB DDR4", Unit = "개", SafetyStock = 50, CurrentStock = 65, Category = "원자재", Supplier = "삼성전자", Location = "G-01-03", Status = "정상" },
                new InventoryItem { ItemCode = "ITEM008", ItemName = "SSD", Specification = "1TB NVMe", Unit = "개", SafetyStock = 40, CurrentStock = 38, Category = "원자재", Supplier = "삼성전자", Location = "H-02-05", Status = "부족" }
            };

            InventoryDataGrid.ItemsSource = sampleInventory;
        }
    }

    // 재고 정보 클래스
    public class InventoryItem
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Specification { get; set; }
        public string Unit { get; set; }
        public int SafetyStock { get; set; }
        public int CurrentStock { get; set; }
        public string Category { get; set; }
        public string Supplier { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
    }
}
