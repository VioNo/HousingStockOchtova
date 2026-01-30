using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HousingStockVio
{
    public partial class ApplicationHistoryPage : Page
    {
        private HousingStock _context;
        private List<string> executors;

        public class ApplicationHistory
        {
            public int ApplicationID { get; set; }
            public string Address { get; set; }
            public string ApplicantName { get; set; }
            public string Phone { get; set; }
            public string Status { get; set; }
            public string EmployeeName { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime? CompleteDate { get; set; }
            public int? DaysToComplete { get; set; }
            public string Description { get; set; }
        }

        public ApplicationHistoryPage()
        {
            InitializeComponent();
            Loaded += ApplicationHistoryPage_Loaded;
            _context = new HousingStock();
        }

        private void ApplicationHistoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllApplications();
            LoadExecutors();
        }

        private void LoadAllApplications()
        {
            try
            {
                var applications = _context.Applications
                    .Select(a => new ApplicationHistory
                    {
                        ApplicationID = a.ID,
                        Address = a.Address,
                        ApplicantName = a.ApplicantName,
                        Phone = a.Phone,
                        Status = a.Status,
                        EmployeeName = !string.IsNullOrEmpty(a.AssignedEmployee) ? a.AssignedEmployee : "Не назначен",
                        CreateDate = a.CreateDate,
                        CompleteDate = a.CompleteDate,
                        DaysToComplete = a.CompleteDate.HasValue ?
                            (int?)(a.CompleteDate.Value - a.CreateDate).Days :
                            (int?)(DateTime.Now - a.CreateDate).Days,
                        Description = a.Description
                    })
                    .OrderByDescending(a => a.CreateDate)
                    .ToList();

                HistoryList.ItemsSource = applications;
                StatusText.Text = $"Загружено заявок: {applications.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка загрузки данных";
            }
        }

        private void LoadExecutors()
        {
            try
            {
                // Получаем уникальных исполнителей из таблицы Applications
                executors = _context.Applications
                    .Where(a => !string.IsNullOrEmpty(a.AssignedEmployee))
                    .Select(a => a.AssignedEmployee)
                    .Distinct()
                    .OrderBy(e => e)
                    .ToList();

                // Если в базе нет данных, добавляем тестовые
                if (executors.Count == 0)
                {
                    executors = new List<string>
                    {
                        "Иванов И.И.",
                        "Петров П.П.",
                        "Сидоров С.С.",
                        "Козлов К.К.",
                        "Не назначен"
                    };
                }
                else
                {
                    // Добавляем опцию для заявок без исполнителя
                    executors.Add("Не назначен");
                }

                // Настраиваем ComboBox для отображения текста
                EmployeeCombo.ItemsSource = executors;
                EmployeeCombo.SelectedIndex = -1; // Сбрасываем выбор

                // Устанавливаем текст подсказки
                EmployeeCombo.Text = "Выберите исполнителя";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки исполнителей: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                // Добавляем тестовые данные при ошибке
                executors = new List<string>
                {
                    "Иванов И.И.",
                    "Петров П.П.",
                    "Сидоров С.С.",
                    "Козлов К.К.",
                    "Не назначен"
                };

                EmployeeCombo.ItemsSource = executors;
                EmployeeCombo.SelectedIndex = -1;
                EmployeeCombo.Text = "Выберите исполнителя";
            }
        }

        private void ReportTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReportTypeCombo == null || EmployeeSearchPanel == null || AddressSearchPanel == null)
                return;

            try
            {
                switch (ReportTypeCombo.SelectedIndex)
                {
                    case 0: // Все заявки
                        EmployeeSearchPanel.Visibility = Visibility.Collapsed;
                        AddressSearchPanel.Visibility = Visibility.Collapsed;
                        LoadAllApplications();
                        break;

                    case 1: // По исполнителям
                        EmployeeSearchPanel.Visibility = Visibility.Visible;
                        AddressSearchPanel.Visibility = Visibility.Collapsed;

                        // Сбрасываем выбор при переходе в этот режим
                        if (EmployeeCombo != null)
                        {
                            EmployeeCombo.SelectedIndex = -1;
                            EmployeeCombo.Text = "Выберите исполнителя";
                        }
                        break;

                    case 2: // По адресам
                        EmployeeSearchPanel.Visibility = Visibility.Collapsed;
                        AddressSearchPanel.Visibility = Visibility.Visible;

                        // Очищаем поле поиска при переходе на этот режим
                        if (AddressSearchBox != null)
                        {
                            AddressSearchBox.Text = "";
                            AddressSearchBox.Focus();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении типа отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowEmployeeHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EmployeeCombo == null || EmployeeCombo.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите исполнителя", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string selectedExecutor = EmployeeCombo.SelectedItem as string;
                if (!string.IsNullOrEmpty(selectedExecutor) && selectedExecutor != "Выберите исполнителя")
                {
                    if (selectedExecutor == "Не назначен")
                    {
                        LoadUnassignedApplications();
                    }
                    else
                    {
                        LoadExecutorApplications(selectedExecutor);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите исполнителя из списка", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowAddressHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AddressSearchBox == null || string.IsNullOrWhiteSpace(AddressSearchBox.Text))
                {
                    MessageBox.Show("Введите адрес для поиска", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                LoadAddressApplications(AddressSearchBox.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddressSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowAddressHistoryButton_Click(sender, e);
            }
        }

        private void LoadExecutorApplications(string executor)
        {
            try
            {
                var query = _context.Applications
                    .Where(a => a.AssignedEmployee == executor)
                    .Select(a => new ApplicationHistory
                    {
                        ApplicationID = a.ID,
                        Address = a.Address,
                        ApplicantName = a.ApplicantName,
                        Phone = a.Phone,
                        Status = a.Status,
                        EmployeeName = a.AssignedEmployee,
                        CreateDate = a.CreateDate,
                        CompleteDate = a.CompleteDate,
                        DaysToComplete = a.CompleteDate.HasValue ?
                            (int?)(a.CompleteDate.Value - a.CreateDate).Days :
                            (int?)(DateTime.Now - a.CreateDate).Days,
                        Description = a.Description
                    })
                    .OrderByDescending(a => a.CreateDate);

                var applications = query.ToList();

                HistoryList.ItemsSource = applications;
                StatusText.Text = $"Заявок у исполнителя {executor}: {applications.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок исполнителя: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка загрузки данных";
            }
        }

        private void LoadUnassignedApplications()
        {
            try
            {
                var applications = _context.Applications
                    .Where(a => string.IsNullOrEmpty(a.AssignedEmployee))
                    .Select(a => new ApplicationHistory
                    {
                        ApplicationID = a.ID,
                        Address = a.Address,
                        ApplicantName = a.ApplicantName,
                        Phone = a.Phone,
                        Status = a.Status,
                        EmployeeName = "Не назначен",
                        CreateDate = a.CreateDate,
                        CompleteDate = a.CompleteDate,
                        DaysToComplete = a.CompleteDate.HasValue ?
                            (int?)(a.CompleteDate.Value - a.CreateDate).Days :
                            (int?)(DateTime.Now - a.CreateDate).Days,
                        Description = a.Description
                    })
                    .OrderByDescending(a => a.CreateDate)
                    .ToList();

                HistoryList.ItemsSource = applications;
                StatusText.Text = $"Заявок без исполнителя: {applications.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок без исполнителя: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка загрузки данных";
            }
        }

        private void LoadAddressApplications(string address)
        {
            try
            {
                var applications = _context.Applications
                    .Where(a => a.Address.Contains(address))
                    .Select(a => new ApplicationHistory
                    {
                        ApplicationID = a.ID,
                        Address = a.Address,
                        ApplicantName = a.ApplicantName,
                        Phone = a.Phone,
                        Status = a.Status,
                        EmployeeName = !string.IsNullOrEmpty(a.AssignedEmployee) ? a.AssignedEmployee : "Не назначен",
                        CreateDate = a.CreateDate,
                        CompleteDate = a.CompleteDate,
                        DaysToComplete = a.CompleteDate.HasValue ?
                            (int?)(a.CompleteDate.Value - a.CreateDate).Days :
                            (int?)(DateTime.Now - a.CreateDate).Days,
                        Description = a.Description
                    })
                    .OrderByDescending(a => a.CreateDate)
                    .ToList();

                HistoryList.ItemsSource = applications;
                StatusText.Text = $"Заявок по адресу '{address}': {applications.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок по адресу: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка загрузки данных";
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Пересоздаем контекст для обновления данных
                _context?.Dispose();
                _context = new HousingStock();

                // Перезагружаем исполнителей
                LoadExecutors();

                // Загружаем заявки в зависимости от текущего режима
                if (ReportTypeCombo == null)
                {
                    LoadAllApplications();
                    MessageBox.Show("Данные обновлены", "Обновление",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                switch (ReportTypeCombo.SelectedIndex)
                {
                    case 0: // Все заявки
                        LoadAllApplications();
                        break;

                    case 1: // По исполнителям
                        if (EmployeeCombo != null && EmployeeCombo.SelectedIndex != -1)
                        {
                            string selectedExecutor = EmployeeCombo.SelectedItem as string;
                            if (!string.IsNullOrEmpty(selectedExecutor) && selectedExecutor != "Выберите исполнителя")
                            {
                                if (selectedExecutor == "Не назначен")
                                {
                                    LoadUnassignedApplications();
                                }
                                else
                                {
                                    LoadExecutorApplications(selectedExecutor);
                                }
                            }
                            else
                            {
                                LoadAllApplications();
                            }
                        }
                        else
                        {
                            LoadAllApplications();
                        }
                        break;

                    case 2: // По адресам
                        if (AddressSearchBox != null && !string.IsNullOrWhiteSpace(AddressSearchBox.Text))
                        {
                            LoadAddressApplications(AddressSearchBox.Text.Trim());
                        }
                        else
                        {
                            LoadAllApplications();
                        }
                        break;

                    default:
                        LoadAllApplications();
                        break;
                }

                MessageBox.Show("Данные обновлены", "Обновление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}