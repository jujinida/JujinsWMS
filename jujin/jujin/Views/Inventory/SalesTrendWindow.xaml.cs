using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;

namespace jujin.Views.Inventory
{
    public partial class SalesTrendWindow : Window, INotifyPropertyChanged
    {
        private readonly HttpClient httpClient;
        private readonly int productId;
        private readonly string productName;

        public SeriesCollection SeriesCollection { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public SalesTrendWindow(int productId, string productName)
        {
            InitializeComponent();
            httpClient = new HttpClient();
            this.productId = productId;
            this.productName = productName;
            
            TitleTextBlock.Text = $"{productName} - 판매 추이 분석";
            
            InitializeChart();
            LoadSalesData();
        }

        private void InitializeChart()
        {
            SeriesCollection = new SeriesCollection();
            DataContext = this;
        }

        private string[] GenerateDayLabels()
        {
            var labels = new List<string>();
            for (int i = 1; i <= 31; i++)
            {
                labels.Add(i.ToString());
            }
            return labels.ToArray();
        }

        private async void LoadSalesData()
        {
            try
            {
                var response = await httpClient.GetAsync($"http://localhost:5185/api/product/sales-trend/{productId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var salesTrendDtos = JsonSerializer.Deserialize<SalesTrendDto[]>(json, options);
                    
                    if (salesTrendDtos != null && salesTrendDtos.Length > 0)
                    {
                        // 데이터를 차트에 맞게 변환
                        var chartValues = new ChartValues<ObservablePoint>();
                        
                        foreach (var dto in salesTrendDtos)
                        {
                            chartValues.Add(new ObservablePoint(dto.DayOfMonth, dto.DailySales));
                        }

                        // 기존 시리즈 제거
                        SeriesCollection.Clear();

                        // 새로운 시리즈 추가
                        SeriesCollection.Add(new LineSeries
                        {
                            Title = "일별 판매량",
                            Values = chartValues,
                            PointGeometry = DefaultGeometries.Circle,
                            PointGeometrySize = 8,
                            LineSmoothness = 0.2,
                            Fill = System.Windows.Media.Brushes.Transparent,
                            Stroke = System.Windows.Media.Brushes.DodgerBlue,
                            StrokeThickness = 3
                        });
                    }
                    else
                    {
                        MessageBox.Show("해당 제품의 판매 데이터가 없습니다.", "정보", 
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("판매 추이 데이터를 불러오는데 실패했습니다.", "오류", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"판매 추이 데이터 로드 중 오류가 발생했습니다: {ex.Message}", "오류", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // 백엔드 DTO 클래스
    public class SalesTrendDto
    {
        public int DayOfMonth { get; set; }
        public int DailySales { get; set; }
    }
}
