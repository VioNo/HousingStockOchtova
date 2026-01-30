using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class EmployeeApplicationsPage : Page
    {
        private HousingStock _context;
        private List<EmployeeApplication> applications;

        public class EmployeeApplication
        {
            public int Id { get; set; }
            public string Address { get; set; }
            public string ApplicantName { get; set; }
            public string Phone { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime? CompleteDate { get; set; }
            public string AssignedEmployee { get; set; }
            public string CategoryName { get; set; }
            public int? DaysInWork { get; set; }
        }

        public EmployeeApplicationsPage()
        {
            InitializeComponent();
            Loaded += EmployeeApplicationsPage_Loaded;
            _context = new HousingStock();
        }

        private void EmployeeApplicationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMyApplications();
        }

        private void LoadMyApplications()
        {
            try
            {
                string currentUserName = CurrentUser.FullName;

                // Получаем заявки, где сотрудник является ответственным или назначенным исполнителем
                var query = _context.Applications
                    .Where(a =>
                        (!string.IsNullOrEmpty(a.Responsible) && a.Responsible.Contains(currentUserName)) ||
                        (!string.IsNullOrEmpty(a.AssignedEmployee) && a.AssignedEmployee.Contains(currentUserName)))
                    .Select(a => new EmployeeApplication
                    {
                        Id = a.ID,
                        Address = a.Address,
                        ApplicantName = a.ApplicantName,
                        Phone = a.Phone,
                        Description = a.Description,
                        Status = a.Status,
                        CreateDate = a.CreateDate,
                        CompleteDate = a.CompleteDate,
                        AssignedEmployee = a.AssignedEmployee,
                        CategoryName = a.ServiceCategories != null ? a.ServiceCategories.CategoryName : "Не указана",
                        DaysInWork = a.CompleteDate.HasValue ?
                            (int?)(a.CompleteDate.Value - a.CreateDate).Days :
                            (int?)(DateTime.Now - a.CreateDate).Days
                    })
                    .OrderByDescending(a => a.CreateDate);

                applications = query.ToList();

                UpdateApplicationsDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // В случае ошибки используем тестовые данные
                CreateMockApplications();
                UpdateApplicationsDisplay();
            }
        }

        private void CreateMockApplications()
        {
            // Тестовые данные для демонстрации
            applications = new List<EmployeeApplication>
            {
                new EmployeeApplication
                {
                    Id = 1,
                    Address = "ул. Ленина, 10, кв. 5",
                    ApplicantName = "Иванов Иван Иванович",
                    Phone = "+7 (999) 111-11-11",
                    Description = "Протекает кран на кухне. Необходима замена смесителя.",
                    AssignedEmployee = CurrentUser.FullName,
                    CategoryName = "Сантехника",
                    Status = "Открыта",
                    CreateDate = DateTime.Now.AddDays(-5),
                    DaysInWork = 5
                },
                new EmployeeApplication
                {
                    Id = 2,
                    Address = "ул. Советская, 25, кв. 42",
                    ApplicantName = "Сидоров Сергей Сергеевич",
                    Phone = "+7 (999) 222-22-22",
                    Description = "Не работает розетка в зале. Требуется замена электропроводки.",
                    AssignedEmployee = CurrentUser.FullName,
                    CategoryName = "Электрика",
                    Status = "В работе",
                    CreateDate = DateTime.Now.AddDays(-3),
                    DaysInWork = 3
                },
                new EmployeeApplication
                {
                    Id = 3,
                    Address = "пр. Мира, 15, кв. 17",
                    ApplicantName = "Козлова Мария Дмитриевна",
                    Phone = "+7 (999) 333-33-33",
                    Description = "Трещина в стене между кухней и гостиной. Требуется штукатурка и покраска.",
                    AssignedEmployee = CurrentUser.FullName,
                    CategoryName = "Ремонт",
                    Status = "Завершена",
                    CreateDate = DateTime.Now.AddDays(-10),
                    CompleteDate = DateTime.Now.AddDays(-2),
                    DaysInWork = 8
                }
            };
        }

        private void UpdateApplicationsDisplay()
        {
            try
            {
                string selectedStatus = GetSelectedFilterStatus();

                if (string.IsNullOrEmpty(selectedStatus) || selectedStatus == "Все")
                {
                    ApplicationsList.ItemsSource = applications;
                }
                else
                {
                    var filteredApplications = applications
                        .Where(app => app.Status == selectedStatus)
                        .ToList();

                    ApplicationsList.ItemsSource = filteredApplications;
                }

                UpdateStatusText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления отображения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetSelectedFilterStatus()
        {
            try
            {
                if (StatusFilter == null || StatusFilter.SelectedItem == null)
                {
                    return "Все";
                }

                var selectedItem = StatusFilter.SelectedItem;
                if (selectedItem is ComboBoxItem comboBoxItem)
                {
                    return comboBoxItem.Content?.ToString() ?? "Все";
                }

                return "Все";
            }
            catch
            {
                return "Все";
            }
        }

        private void UpdateStatusText()
        {
            try
            {
                var displayedItems = ApplicationsList?.ItemsSource as IEnumerable<EmployeeApplication>;
                int displayedCount = displayedItems?.Count() ?? applications.Count;
                int totalCount = applications.Count;

                if (StatusText != null)
                {
                    StatusText.Text = $"Показано: {displayedCount} из {totalCount} заявок";
                }
            }
            catch
            {
                if (StatusText != null)
                {
                    StatusText.Text = "Ошибка обновления статуса";
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Обновляем контекст для получения свежих данных
                _context?.Dispose();
                _context = new HousingStock();

                LoadMyApplications();
                MessageBox.Show("Список заявок обновлен", "Обновление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления заявок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedApplication = ApplicationsList.SelectedItem as EmployeeApplication;
            if (selectedApplication == null)
            {
                MessageBox.Show("Выберите заявку для изменения статуса", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var statusWindow = new ChangeStatusWindow(selectedApplication);
            statusWindow.Owner = Window.GetWindow(this);

            if (statusWindow.ShowDialog() == true)
            {
                // Обновляем контекст и загружаем данные
                _context?.Dispose();
                _context = new HousingStock();
                LoadMyApplications();
            }
        }

        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowApplicationDetails();
        }

        private void ApplicationsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowApplicationDetails();
        }

        private void ShowApplicationDetails()
        {
            var selectedApplication = ApplicationsList.SelectedItem as EmployeeApplication;
            if (selectedApplication != null)
            {
                MessageBox.Show(
                    $"Детали заявки #{selectedApplication.Id}\n\n" +
                    $"Адрес: {selectedApplication.Address}\n" +
                    $"Заявитель: {selectedApplication.ApplicantName}\n" +
                    $"Телефон: {selectedApplication.Phone}\n" +
                    $"Категория: {selectedApplication.CategoryName}\n" +
                    $"Статус: {selectedApplication.Status}\n" +
                    $"Дата создания: {selectedApplication.CreateDate:dd.MM.yyyy HH:mm}\n" +
                    (selectedApplication.CompleteDate.HasValue ?
                        $"Дата завершения: {selectedApplication.CompleteDate.Value:dd.MM.yyyy}\n" : "") +
                    (selectedApplication.DaysInWork.HasValue ?
                        $"Дней в работе: {selectedApplication.DaysInWork}\n" : "") +
                    $"Описание: {selectedApplication.Description}",
                    "Детали заявки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateApplicationsDisplay();
        }
    }
}