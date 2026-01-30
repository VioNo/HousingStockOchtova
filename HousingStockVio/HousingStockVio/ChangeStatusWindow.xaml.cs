using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class ChangeStatusWindow : Window
    {
        private HousingStock _context;
        private EmployeeApplicationsPage.EmployeeApplication application;

        public ChangeStatusWindow(EmployeeApplicationsPage.EmployeeApplication selectedApplication)
        {
            InitializeComponent();
            application = selectedApplication;
            _context = new HousingStock();
            LoadApplicationInfo();
            StatusComboBox.SelectedIndex = 0;
        }

        private void LoadApplicationInfo()
        {
            ApplicationInfoText.Text = $"Заявка #{application.Id}\n" +
                                     $"Адрес: {application.Address}\n" +
                                     $"Текущий статус: {application.Status}";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите новый статус", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newStatus = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string comment = CommentBox.Text.Trim();

            try
            {
                UpdateApplicationStatus(newStatus, comment);
                DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления статуса: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateApplicationStatus(string newStatus, string comment)
        {
            try
            {
                // Находим заявку в базе данных
                var applicationEntity = _context.Applications.Find(application.Id);

                if (applicationEntity == null)
                {
                    throw new Exception("Заявка не найдена в базе данных");
                }

                // Обновляем статус
                applicationEntity.Status = newStatus;

                // Если статус "Завершена", устанавливаем дату завершения
                if (newStatus == "Завершена")
                {
                    applicationEntity.CompleteDate = DateTime.Now;
                }

                

                // Сохраняем изменения
                _context.SaveChanges();

                // Обновляем локальный объект для отображения
                application.Status = newStatus;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обновлении статуса: {ex.Message}", ex);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}