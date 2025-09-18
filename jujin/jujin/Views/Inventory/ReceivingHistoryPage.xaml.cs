using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.Inventory
{
    public partial class ReceivingHistoryPage : UserControl
    {
        public ReceivingHistoryPage()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // 샘플 데이터 로드 (실제로는 데이터베이스에서 가져옴)
            var sampleReceivings = new ObservableCollection<ReceivingInfo>
            {
                new ReceivingInfo { ReceivingId = "R001", ReceivingDate = "2024-01-15", ItemCode = "ITEM001", ItemName = "스마트폰", Quantity = 50, Unit = "개", Supplier = "삼성전자", Manager = "김철수", Reason = "정상 구매", Remarks = "신제품 입고" },
                new ReceivingInfo { ReceivingId = "R002", ReceivingDate = "2024-01-16", ItemCode = "ITEM002", ItemName = "배터리", Quantity = 100, Unit = "개", Supplier = "LG화학", Manager = "이영희", Reason = "재고 보충", Remarks = "안전재고 보충" },
                new ReceivingInfo { ReceivingId = "R003", ReceivingDate = "2024-01-17", ItemCode = "ITEM003", ItemName = "LCD패널", Quantity = 200, Unit = "개", Supplier = "LG디스플레이", Manager = "박민수", Reason = "정상 구매", Remarks = "대량 주문" },
                new ReceivingInfo { ReceivingId = "R004", ReceivingDate = "2024-01-18", ItemCode = "ITEM004", ItemName = "케이스", Quantity = 300, Unit = "개", Supplier = "케이스코리아", Manager = "최지영", Reason = "반품 입고", Remarks = "불량품 교체" },
                new ReceivingInfo { ReceivingId = "R005", ReceivingDate = "2024-01-19", ItemCode = "ITEM005", ItemName = "충전기", Quantity = 150, Unit = "개", Supplier = "애플", Manager = "정현우", Reason = "정상 구매", Remarks = "신제품 출시 대비" }
            };

            ReceivingDataGrid.ItemsSource = sampleReceivings;
        }
    }

    // 입고 정보 클래스
    public class ReceivingInfo
    {
        public string ReceivingId { get; set; }
        public string ReceivingDate { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public string Supplier { get; set; }
        public string Manager { get; set; }
        public string Reason { get; set; }
        public string Remarks { get; set; }
    }
}
