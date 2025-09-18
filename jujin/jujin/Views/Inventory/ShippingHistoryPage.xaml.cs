using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.Inventory
{
    public partial class ShippingHistoryPage : UserControl
    {
        public ShippingHistoryPage()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // 샘플 데이터 로드 (실제로는 데이터베이스에서 가져옴)
            var sampleShippings = new ObservableCollection<ShippingInfo>
            {
                new ShippingInfo { ShippingId = "S001", ShippingDate = "2024-01-15", ItemCode = "ITEM001", ItemName = "스마트폰", Quantity = 20, Unit = "개", Customer = "삼성전자", Manager = "김철수", Reason = "정상 판매", Remarks = "신제품 출고" },
                new ShippingInfo { ShippingId = "S002", ShippingDate = "2024-01-16", ItemCode = "ITEM002", ItemName = "배터리", Quantity = 50, Unit = "개", Customer = "LG화학", Manager = "이영희", Reason = "샘플 제공", Remarks = "신제품 테스트용" },
                new ShippingInfo { ShippingId = "S003", ShippingDate = "2024-01-17", ItemCode = "ITEM003", ItemName = "LCD패널", Quantity = 100, Unit = "개", Customer = "LG디스플레이", Manager = "박민수", Reason = "정상 판매", Remarks = "대량 주문" },
                new ShippingInfo { ShippingId = "S004", ShippingDate = "2024-01-18", ItemCode = "ITEM004", ItemName = "케이스", Quantity = 200, Unit = "개", Customer = "케이스코리아", Manager = "최지영", Reason = "폐기 처리", Remarks = "불량품 폐기" },
                new ShippingInfo { ShippingId = "S005", ShippingDate = "2024-01-19", ItemCode = "ITEM005", ItemName = "충전기", Quantity = 30, Unit = "개", Customer = "애플", Manager = "정현우", Reason = "정상 판매", Remarks = "신제품 출시 대비" }
            };

            ShippingDataGrid.ItemsSource = sampleShippings;
        }
    }

    // 출고 정보 클래스
    public class ShippingInfo
    {
        public string ShippingId { get; set; }
        public string ShippingDate { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public string Customer { get; set; }
        public string Manager { get; set; }
        public string Reason { get; set; }
        public string Remarks { get; set; }
    }
}
