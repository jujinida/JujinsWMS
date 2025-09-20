using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace jujin.Views.HR
{
    public partial class PayrollRecordDialog : Window, INotifyPropertyChanged
    {
        private string employeeId;
        private int currentYear;
        
        public SeriesCollection RatioSeriesCollection { get; set; }
        public string[] RatioLabels { get; set; }
        public Func<double, string> RatioYFormatter { get; set; }

        public PayrollRecordDialog(string employeeId, int year)
        {
            InitializeComponent();
            this.employeeId = employeeId;
            this.currentYear = year;
            
            InitializeChart();
            UpdateYearDisplay();
            LoadPayrollRecords();
        }

        private void InitializeChart()
        {
            RatioSeriesCollection = new SeriesCollection();
            RatioLabels = new[] { "기본급", "수당", "공제액", "실지급액" };
            RatioYFormatter = value => $"{value:N0}원";
            
            DataContext = this;
        }

        private void UpdateYearDisplay()
        {
            CurrentYearText.Text = $"{currentYear}년";
            TableTitleText.Text = $"{currentYear}년 통계";
        }

        private async Task LoadPayrollRecords()
        {
            try
            {
                using var httpClient = new HttpClient();
                var monthlyPayrollData = new List<MonthlyPayrollData>();

                // Fetch data for each month of the current year
                for (int month = 1; month <= 12; month++)
                {
                    string paymentMonth = $"{currentYear}-{month:D2}";
                    var response = await httpClient.GetAsync($"http://localhost:5185/api/hr/payroll?employeeId={employeeId}&paymentMonth={paymentMonth}");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        var payrollDtos = JsonSerializer.Deserialize<List<PayrollDto>>(json, options);

                        var employeeData = payrollDtos?.FirstOrDefault(p => p.EmployeeId.ToString() == employeeId);
                        if (employeeData != null)
                        {
                            monthlyPayrollData.Add(new MonthlyPayrollData
                            {
                                Month = month,
                                GrossPay = employeeData.GrossPay,
                                Allowance = employeeData.Allowance,
                                Deductions = employeeData.Deductions,
                                NetPay = employeeData.NetPay,
                                PaymentStatus = employeeData.PaymentStatus
                            });
                        }
                        else
                        {
                            monthlyPayrollData.Add(new MonthlyPayrollData
                            {
                                Month = month,
                                GrossPay = 0,
                                Allowance = 0,
                                Deductions = 0,
                                NetPay = 0,
                                PaymentStatus = "미지급"
                            });
                        }
                    }
                    else
                    {
                        monthlyPayrollData.Add(new MonthlyPayrollData
                        {
                            Month = month,
                            GrossPay = 0,
                            Allowance = 0,
                            Deductions = 0,
                            NetPay = 0,
                            PaymentStatus = "미지급"
                        });
                    }
                }
                
                UpdateTableAndChart(monthlyPayrollData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"급여 기록을 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTableAndChart(List<MonthlyPayrollData> payrollData)
        {
            // 테이블 데이터 업데이트
            PayrollDataGrid.ItemsSource = payrollData;

            // 총합 계산
            decimal totalGrossPay = payrollData.Sum(p => p.GrossPay);
            decimal totalAllowance = payrollData.Sum(p => p.Allowance);
            decimal totalDeductions = payrollData.Sum(p => p.Deductions);
            decimal totalNetPay = payrollData.Sum(p => p.NetPay);

            // 비율 차트 업데이트
            RatioSeriesCollection.Clear();

            var values = new ChartValues<double>
            {
                (double)totalGrossPay,
                (double)totalAllowance,
                (double)totalDeductions,
                (double)totalNetPay
            };

            // 메인 차트 시리즈 (바 위에 라벨)
            var mainSeries = new ColumnSeries
            {
                Title = "총합",
                Values = values,
                DataLabels = true,
                LabelPoint = point => $"{point.Y:N0}원 ({GetPercentage(point.Y, totalNetPay)}%)",
                FontSize = 10
            };

            // 각 항목별 색상 설정
            mainSeries.Fill = System.Windows.Media.Brushes.LightBlue;
            mainSeries.Stroke = System.Windows.Media.Brushes.DodgerBlue;
            mainSeries.StrokeThickness = 2;

            RatioSeriesCollection.Add(mainSeries);

            // X축 라벨 업데이트
            RatioLabels = new[] { "기본급", "수당", "공제액", "실지급액" };

            OnPropertyChanged(nameof(RatioSeriesCollection));
            OnPropertyChanged(nameof(RatioLabels));
        }

        private string GetPercentage(double value, decimal total)
        {
            if (total == 0) return "0%";
            return $"{(value / (double)total * 100):F1}%";
        }


        private void PreviousYearButton_Click(object sender, RoutedEventArgs e)
        {
            currentYear--;
            UpdateYearDisplay();
            LoadPayrollRecords();
        }

        private void NextYearButton_Click(object sender, RoutedEventArgs e)
        {
            currentYear++;
            UpdateYearDisplay();
            LoadPayrollRecords();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPayrollRecords();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Class to hold monthly payroll data
    public class MonthlyPayrollData
    {
        public int Month { get; set; }
        public decimal GrossPay { get; set; }
        public decimal Allowance { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }
        public string PaymentStatus { get; set; }
    }

}