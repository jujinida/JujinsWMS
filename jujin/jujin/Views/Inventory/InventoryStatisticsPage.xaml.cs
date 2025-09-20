using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.Inventory
{
    public partial class InventoryStatisticsPage : UserControl
    {
        private readonly HttpClient httpClient;
        private ObservableCollection<InventoryStatistics> statisticsItems;

        public InventoryStatisticsPage()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            statisticsItems = new ObservableCollection<InventoryStatistics>();
            StatisticsDataGrid.ItemsSource = statisticsItems;
            LoadStatisticsData();
        }

        private async void LoadStatisticsData(string? productName = null, string? category = null)
        {
            try
            {
                // URL에 검색 조건 추가
                var url = "http://localhost:5185/api/product/inventory-statistics";
                var queryParams = new List<string>();
                
                if (!string.IsNullOrEmpty(productName))
                {
                    queryParams.Add($"productName={Uri.EscapeDataString(productName)}");
                }
                
                if (!string.IsNullOrEmpty(category) && category != "전체")
                {
                    queryParams.Add($"category={Uri.EscapeDataString(category)}");
                }
                
                if (queryParams.Count > 0)
                {
                    url += "?" + string.Join("&", queryParams);
                }
                
                var response = await httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var statisticsDtos = JsonSerializer.Deserialize<InventoryStatisticsDto[]>(json, options);
                    
                    statisticsItems.Clear();
                    if (statisticsDtos != null && statisticsDtos.Length > 0)
                    {
                        foreach (var dto in statisticsDtos)
                        {
                            var status = DetermineStatus(dto.InventoryDepletionIndex);
                            
                            statisticsItems.Add(new InventoryStatistics
                            {
                                ItemCode = dto.ProductId.ToString(),
                                ItemName = dto.ProductName ?? "이름 없음",
                                Category = dto.Category ?? "미분류",
                                DailyChangeRate = dto.InventoryTurnoverRate.ToString("F2"),
                                MonthlyChangeRate = dto.InventoryDepletionIndex.ToString("F2"),
                                QuarterlyChangeRate = dto.MonthlySalesVolume.ToString(),
                                CurrentStock = dto.MonthlySalesVolume,
                                SafetyStock = 0, // 안전재고량은 별도로 조회 필요
                                Status = status
                            });
                        }
                    }
                }
                else
                {
                    MessageBox.Show("재고 통계 데이터를 불러오는데 실패했습니다.", "오류", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"재고 통계 데이터 로드 중 오류가 발생했습니다: {ex.Message}", "오류", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string DetermineStatus(decimal inventoryDepletionIndex)
        {
            if (inventoryDepletionIndex >= 1.0m)
            {
                return "부족";
            }
            else if (inventoryDepletionIndex >= 0.8m)
            {
                return "주의";
            }
            else
            {
                return "정상";
            }
        }

        private void StatisticsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowDetailPage();
        }

        private void DetailButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is InventoryStatistics item)
            {
                var salesTrendWindow = new SalesTrendWindow(int.Parse(item.ItemCode), item.ItemName);
                salesTrendWindow.ShowDialog();
            }
        }

        private void ShowDetailPage()
        {
            var selectedItem = StatisticsDataGrid.SelectedItem as InventoryStatistics;
            if (selectedItem != null)
            {
                var salesTrendWindow = new SalesTrendWindow(int.Parse(selectedItem.ItemCode), selectedItem.ItemName);
                salesTrendWindow.ShowDialog();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var productName = ProductNameTextBox.Text?.Trim();
            var selectedCategory = CategoryComboBox.SelectedItem as ComboBoxItem;
            var category = selectedCategory?.Content?.ToString();
            
            LoadStatisticsData(productName, category);
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

    // 백엔드 DTO 클래스
    public class InventoryStatisticsDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal InventoryTurnoverRate { get; set; }
        public decimal InventoryDepletionIndex { get; set; }
        public int MonthlySalesVolume { get; set; }
    }
}
