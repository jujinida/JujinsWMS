using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.Inventory
{
    public partial class ReceivingHistoryPage : UserControl
    {
        private readonly HttpClient httpClient;
        private ObservableCollection<ReceivingHistoryInfo> receivingHistory;

        public ReceivingHistoryPage()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            receivingHistory = new ObservableCollection<ReceivingHistoryInfo>();
            ReceivingHistoryDataGrid.ItemsSource = receivingHistory;
            Loaded += ReceivingHistoryPage_Loaded;
        }

        private async void ReceivingHistoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadReceivingHistory();
        }

        private async Task LoadReceivingHistory()
        {
            try
            {
                var response = await httpClient.GetAsync("http://localhost:5185/api/product/receiving-history");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var historyDtos = JsonSerializer.Deserialize<ReceivingHistoryDto[]>(json, options);
                    
                    receivingHistory.Clear();
                    if (historyDtos != null && historyDtos.Length > 0)
                    {
                        foreach (var dto in historyDtos)
                        {
                            receivingHistory.Add(new ReceivingHistoryInfo
                            {
                                LogId = dto.LogId,
                                LogDate = dto.LogDate,
                                ProductId = dto.ProductId,
                                ProductName = dto.ProductName ?? "이름 없음",
                                QuantityChanged = dto.QuantityChanged,
                                CurrentQuantity = dto.CurrentQuantity
                            });
                        }
                    }
                    else
                    {
                        MessageBox.Show("입고내역이 없습니다.", "알림", 
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"입고내역을 불러오는데 실패했습니다.\n상태: {response.StatusCode}\n내용: {errorContent}", "오류", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"입고내역을 불러오는 중 오류가 발생했습니다: {ex.Message}", 
                                "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // 입고내역 정보 클래스 (UI 바인딩용)
    public class ReceivingHistoryInfo
    {
        public int LogId { get; set; }
        public DateTime LogDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantityChanged { get; set; }
        public int CurrentQuantity { get; set; }
    }

    // 백엔드 DTO 클래스 (API 통신용)
    public class ReceivingHistoryDto
    {
        public int LogId { get; set; }
        public DateTime LogDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantityChanged { get; set; }
        public int CurrentQuantity { get; set; }
    }
}