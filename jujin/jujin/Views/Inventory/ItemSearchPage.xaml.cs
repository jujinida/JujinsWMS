using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace jujin.Views.Inventory
{
    public partial class ItemSearchPage : UserControl
    {
        private readonly HttpClient httpClient;
        private ObservableCollection<ProductInfo> products;

        public ItemSearchPage()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            products = new ObservableCollection<ProductInfo>();
            ItemDataGrid.ItemsSource = products;
            Loaded += ItemSearchPage_Loaded;
        }

        private async void ItemSearchPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 테스트를 위해 먼저 테스트 데이터 생성
            await CreateTestData();
            await LoadProducts();
        }

        private async Task CreateTestData()
        {
            try
            {
                var response = await httpClient.PostAsync("http://localhost:5185/api/product/seed", null);
                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("테스트 데이터 생성 성공");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"테스트 데이터 생성 실패: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"테스트 데이터 생성 중 오류: {ex.Message}");
            }
        }

        private async Task LoadProducts()
        {
            try
            {
                var response = await httpClient.GetAsync("http://localhost:5185/api/product/list");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    
                    // 디버깅을 위한 로그
                    System.Diagnostics.Debug.WriteLine($"받은 JSON: {json}");
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var productDtos = JsonSerializer.Deserialize<ProductDto[]>(json, options);
                    
                    System.Diagnostics.Debug.WriteLine($"역직렬화된 제품 수: {productDtos?.Length ?? 0}");
                    
                    products.Clear();
                    if (productDtos != null && productDtos.Length > 0)
                    {
                        foreach (var dto in productDtos)
                        {
                            System.Diagnostics.Debug.WriteLine($"제품: ID={dto.ProductId}, Name={dto.ProductName}, Price={dto.Price}");
                            
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
                        
                        System.Diagnostics.Debug.WriteLine($"UI 컬렉션에 추가된 제품 수: {products.Count}");
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

        private void ItemDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ItemDataGrid.SelectedItem is ProductInfo selectedProduct)
            {
                OpenEditWindow(selectedProduct);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductInfo selectedProduct)
            {
                OpenEditWindow(selectedProduct);
            }
        }

        private void OpenEditWindow(ProductInfo product)
        {
            var editWindow = new ProductEditWindow(product);
            editWindow.ProductUpdated += async (s, e) => await LoadProducts();
            editWindow.ShowDialog();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadProducts();
        }
    }

    // 제품 정보 클래스
    public class ProductInfo
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
    }

    // 백엔드 DTO 클래스
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
    }
}

