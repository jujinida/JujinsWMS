using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.Inventory
{
    public partial class ShippingHistoryPage : UserControl
    {
        private readonly HttpClient httpClient;
        private ObservableCollection<ShippingHistoryInfo> shippingHistory;

        public ShippingHistoryPage()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            shippingHistory = new ObservableCollection<ShippingHistoryInfo>();
            ShippingHistoryDataGrid.ItemsSource = shippingHistory;
            Loaded += ShippingHistoryPage_Loaded;
        }

        private async void ShippingHistoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadShippingHistory();
        }

        private async Task LoadShippingHistory()
        {
            try
            {
                var response = await httpClient.GetAsync("http://localhost:5185/api/product/shipping-history");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var historyDtos = JsonSerializer.Deserialize<ShippingHistoryDto[]>(json, options);
                    
                    shippingHistory.Clear();
                    if (historyDtos != null && historyDtos.Length > 0)
                    {
                        // 필터링 적용
                        var filteredHistory = ApplyFilters(historyDtos);
                        
                        foreach (var dto in filteredHistory)
                        {
                            shippingHistory.Add(new ShippingHistoryInfo
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
                        MessageBox.Show("출고내역이 없습니다.", "알림", 
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"출고내역을 불러오는데 실패했습니다.\n상태: {response.StatusCode}\n내용: {errorContent}", "오류", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"출고내역을 불러오는 중 오류가 발생했습니다: {ex.Message}", 
                                "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadShippingHistory();
        }

        private IEnumerable<ShippingHistoryDto> ApplyFilters(ShippingHistoryDto[] allHistory)
        {
            var filteredHistory = allHistory.AsEnumerable();

            // 품목코드 필터
            if (!string.IsNullOrWhiteSpace(ProductIdTextBox.Text) && 
                int.TryParse(ProductIdTextBox.Text, out int searchProductId))
            {
                filteredHistory = filteredHistory.Where(h => h.ProductId == searchProductId);
            }

            // 품목명 필터
            if (!string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
            {
                string searchName = ProductNameTextBox.Text.Trim();
                filteredHistory = filteredHistory.Where(h => 
                    !string.IsNullOrEmpty(h.ProductName) && 
                    h.ProductName.Contains(searchName, StringComparison.OrdinalIgnoreCase));
            }

            // 날짜 필터
            if (SearchDatePicker.SelectedDate.HasValue)
            {
                DateTime selectedDate = SearchDatePicker.SelectedDate.Value.Date;
                filteredHistory = filteredHistory.Where(h => h.LogDate.Date == selectedDate);
            }

            return filteredHistory.ToArray();
        }
    }

    // 출고내역 정보 클래스 (UI 바인딩용)
    public class ShippingHistoryInfo
    {
        public int LogId { get; set; }
        public DateTime LogDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantityChanged { get; set; }
        public int CurrentQuantity { get; set; }
    }

    // 백엔드 DTO 클래스 (API 통신용)
    public class ShippingHistoryDto
    {
        public int LogId { get; set; }
        public DateTime LogDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantityChanged { get; set; }
        public int CurrentQuantity { get; set; }
    }
}