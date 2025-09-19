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
    public partial class ShippingListPage : UserControl
    {
        private HttpClient httpClient;
        private ObservableCollection<ProductInfo> products;
        private ShippingManagementSubPage parentPage;

        public ShippingListPage()
        {
            InitializeComponent();
        }

        public void SetDataContext(ObservableCollection<ProductInfo> products, HttpClient httpClient, ShippingManagementSubPage parentPage)
        {
            this.products = products;
            this.httpClient = httpClient;
            this.parentPage = parentPage;
            ProductDataGrid.ItemsSource = products;
            
            // 데이터 로드
            _ = LoadProducts();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadProducts();
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
                        // 필터링 적용
                        var filteredProducts = ApplyFilters(productDtos);
                        
                        foreach (var dto in filteredProducts)
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

        private void ShipButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductInfo selectedProduct)
            {
                OpenShippingWindow(selectedProduct);
            }
        }

        private void OpenShippingWindow(ProductInfo product)
        {
            var shippingWindow = new ShippingDetailWindow(product);
            shippingWindow.ProductUpdated += async (s, e) => await LoadProducts();
            shippingWindow.ShowDialog();
        }

        private IEnumerable<ProductDto> ApplyFilters(ProductDto[] allProducts)
        {
            var filteredProducts = allProducts.AsEnumerable();

            // 품목코드 필터
            if (!string.IsNullOrWhiteSpace(ProductIdTextBox.Text) && 
                int.TryParse(ProductIdTextBox.Text, out int searchProductId))
            {
                filteredProducts = filteredProducts.Where(p => p.ProductId == searchProductId);
            }

            // 품목명 필터
            if (!string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
            {
                string searchName = ProductNameTextBox.Text.Trim();
                filteredProducts = filteredProducts.Where(p => 
                    !string.IsNullOrEmpty(p.ProductName) && 
                    p.ProductName.Contains(searchName, StringComparison.OrdinalIgnoreCase));
            }

            // 카테고리 필터
            if (CategoryComboBox.SelectedItem is ComboBoxItem selectedCategoryItem && 
                selectedCategoryItem.Content.ToString() != "전체")
            {
                string selectedCategory = selectedCategoryItem.Content.ToString();
                filteredProducts = filteredProducts.Where(p => p.Category == selectedCategory);
            }

            return filteredProducts.ToArray();
        }
    }
}
