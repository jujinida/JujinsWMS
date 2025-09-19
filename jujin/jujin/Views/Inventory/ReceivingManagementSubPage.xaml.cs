using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.Inventory
{
    public partial class ReceivingManagementSubPage : UserControl
    {
        private readonly HttpClient httpClient;
        private ObservableCollection<ProductInfo> products;

        public ReceivingManagementSubPage()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            products = new ObservableCollection<ProductInfo>();
            Loaded += ReceivingManagementSubPage_Loaded;
        }

        private async void ReceivingManagementSubPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProducts();
            ShowReceivingListPage();
        }

        private async Task LoadProducts()
        {
            try
            {
                var response = await httpClient.GetAsync("http://localhost:5185/api/product/list");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var productDtos = JsonSerializer.Deserialize<ProductDto[]>(json, options);
                    
                    products.Clear();
                    if (productDtos != null && productDtos.Length > 0)
                    {
                        foreach (var dto in productDtos)
                        {
                            products.Add(new ProductInfo
                            {
                                ProductId = dto.ProductId,
                                ProductName = dto.ProductName ?? "이름 없음",
                                Category = dto.Category ?? "미분류",
                                Price = dto.Price,
                                StockQuantity = dto.StockQuantity,
                                ImageUrl = dto.ImageUrl
                            });
                        }
                    }
                    else
                    {
                        MessageBox.Show("등록된 제품이 없습니다.\n품목 등록 페이지에서 제품을 먼저 등록해주세요.", "알림", 
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"제품 목록을 불러오는데 실패했습니다.\n상태: {response.StatusCode}\n내용: {errorContent}", "오류", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"제품 목록을 불러오는 중 오류가 발생했습니다: {ex.Message}", 
                                "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReceivingListButton_Click(object sender, RoutedEventArgs e)
        {
            ShowReceivingListPage();
            UpdateButtonStates(true);
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHistoryPage();
            UpdateButtonStates(false);
        }

        private void ShowReceivingListPage()
        {
            var receivingListPage = new ReceivingListPage();
            receivingListPage.SetDataContext(products, httpClient, this);
            MainContentControl.Content = receivingListPage;
        }

        private void ShowHistoryPage()
        {
            var historyPage = new ReceivingHistoryPage();
            MainContentControl.Content = historyPage;
        }

        private void UpdateButtonStates(bool isReceivingListActive)
        {
            if (isReceivingListActive)
            {
                ReceivingListBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(94, 129, 172));
                HistoryBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
            }
            else
            {
                ReceivingListBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                HistoryBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(94, 129, 172));
            }
        }
    }
}
