using System;
using System.Windows;

namespace HousingStockVio
{
    public partial class CreateScheduleWindow : Window
    {
        public CreateScheduleWindow()
        {
            InitializeComponent();
            StartDatePicker.SelectedDate = DateTime.Now;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (PeriodComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите период графика", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StartDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату начала", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("График работ успешно создан", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
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