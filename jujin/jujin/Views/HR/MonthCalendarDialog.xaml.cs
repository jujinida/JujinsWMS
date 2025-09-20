using System;
using System.Windows;
using System.Windows.Controls;

namespace jujin.Views.HR
{
    public partial class MonthCalendarDialog : Window
    {
        public DateTime SelectedDate { get; private set; }
        private DateTime currentYear;

        public MonthCalendarDialog(DateTime initialDate)
        {
            InitializeComponent();
            currentYear = new DateTime(initialDate.Year, 1, 1);
            SelectedDate = initialDate;
            UpdateYearDisplay();
        }

        private void UpdateYearDisplay()
        {
            CurrentYearText.Text = $"{currentYear.Year}년";
        }

        private void PreviousYearButton_Click(object sender, RoutedEventArgs e)
        {
            currentYear = currentYear.AddYears(-1);
            UpdateYearDisplay();
        }

        private void NextYearButton_Click(object sender, RoutedEventArgs e)
        {
            currentYear = currentYear.AddYears(1);
            UpdateYearDisplay();
        }

        private void MonthButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int month)
            {
                SelectedDate = new DateTime(currentYear.Year, month, 1);
                DialogResult = true;
                Close();
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
