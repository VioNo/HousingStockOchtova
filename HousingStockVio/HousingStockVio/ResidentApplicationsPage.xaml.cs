using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class ResidentApplicationsPage : Page
    {
        private HousingStock _context;
        private List<ResidentApplication> applications;

        public class ResidentApplication
        {
            public int Id { get; set; }
            public string Address { get; set; }
            public string CategoryName { get; set; }
            public string Description { get; set; }
            public string AssignedEmployee { get; set; }
            public string Status { get; set; }
            public DateTime CreateDate { get; set; }
            public int? DaysInWork { get; set; }
        }

        public ResidentApplicationsPage()
        {
            InitializeComponent();
            Loaded += ResidentApplicationsPage_Loaded;
            _context = new HousingStock();
        }

        private void ResidentApplicationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadResidentApplications();
        }

        private void LoadResidentApplications()
        {
            try
            {
                // Получаем имя текущего пользователя
                string currentUserName = CurrentUser.FullName;

                // Пытаемся загрузить данные из базы
                bool loadedFromDB = LoadFromDatabase(currentUserName);

                // Если из базы ничего не загрузилось, создаем тестовые данные
                if (!loadedFromDB)
                {
                    CreateTestApplications(currentUserName);
                }

                UpdateApplicationsDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}\nСоздаю тестовые данные.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);

                CreateTestApplications(CurrentUser.FullName);
                UpdateApplicationsDisplay();
            }
        }

        private bool LoadFromDatabase(string residentName)
        {
            try
            {
                // Проверяем доступность базы данных
                if (!_context.Database.Exists())
                {
                    return false;
                }

                // Проверяем, есть ли таблица Applications
                if (!_context.Applications.Any())
                {
                    // Таблица существует, но пуста
                    return false;
                }

                // Загружаем заявки текущего жителя
                var dbApplications = _context.Applications
                    .Where(app =>
                        app.ApplicantName != null &&
                        (app.ApplicantName.Equals(residentName, StringComparison.OrdinalIgnoreCase) ||
                         app.ApplicantName.Contains(residentName)))
                    .Select(app => new ResidentApplication
                    {
                        Id = app.ID,
                        Address = app.Address,
                        CategoryName = app.ServiceCategories != null ? app.ServiceCategories.CategoryName : "Не указана",
                        Description = app.Description,
                        AssignedEmployee = app.AssignedEmployee ?? "Не назначен",
                        Status = app.Status ?? "Открыта",
                        CreateDate = app.CreateDate,
                        DaysInWork = app.CompleteDate.HasValue ?
                            (int?)(app.CompleteDate.Value - app.CreateDate).TotalDays :
                            (int?)(DateTime.Now - app.CreateDate).TotalDays
                    })
                    .OrderByDescending(app => app.CreateDate)
                    .ToList();

                if (dbApplications.Any())
                {
                    applications = dbApplications;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки из базы: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void CreateTestApplications(string residentName)
        {
            applications = new List<ResidentApplication>
            {
                new ResidentApplication
                {
                    Id = 1001,
                    Address = "ул. Ленина, 10, кв. 5",
                    CategoryName = "Сантехника",
                    Description = "Протекает кран на кухне. Необходима замена смесителя.",
                    AssignedEmployee = "Иванов И.И.",
                    Status = "Открыта",
                    CreateDate = DateTime.Now.AddDays(-2),
                    DaysInWork = 2
                },
                new ResidentApplication
                {
                    Id = 1002,
                    Address = "ул. Советская, 25, кв. 12",
                    CategoryName = "Электрика",
                    Description = "Не работает розетка в зале. Требуется замена электропроводки.",
                    AssignedEmployee = "Петров П.П.",
                    Status = "В работе",
                    CreateDate = DateTime.Now.AddDays(-5),
                    DaysInWork = 5
                },
                new ResidentApplication
                {
                    Id = 1003,
                    Address = "пр. Мира, 15, кв. 8",
                    CategoryName = "Общедомовые работы",
                    Description = "Трещина в стене. Требуется штукатурка и покраска.",
                    AssignedEmployee = "Сидоров С.С.",
                    Status = "Завершена",
                    CreateDate = DateTime.Now.AddDays(-10),
                    DaysInWork = 3
                }
            };
        }

        private void UpdateApplicationsDisplay()
        {
            try
            {
                if (ApplicationsList == null)
                {
                    return;
                }

                // Получаем выбранный фильтр
                string selectedStatus = GetSelectedFilterStatus();

                // Фильтруем заявки
                IEnumerable<ResidentApplication> displayApplications;

                if (string.IsNullOrEmpty(selectedStatus) || selectedStatus == "Все")
                {
                    displayApplications = applications;
                }
                else
                {
                    displayApplications = applications
                        .Where(app => app.Status.Equals(selectedStatus, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // Устанавливаем источник данных
                ApplicationsList.ItemsSource = displayApplications;

                // Обновляем статус
                UpdateStatusText(displayApplications.Count());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отображения заявок: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

                if (StatusFilter.SelectedItem is ComboBoxItem comboBoxItem)
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

        private void UpdateStatusText(int displayedCount)
        {
            try
            {
                if (StatusText != null)
                {
                    StatusText.Text = $"Мои заявки: {displayedCount} (всего: {applications.Count})";
                }
            }
            catch
            {
                // Игнорируем ошибки при обновлении текста статуса
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editWindow = new EditApplicationWindow();
                editWindow.Owner = Window.GetWindow(this);
                editWindow.Title = "Новая заявка";

                if (editWindow.ShowDialog() == true)
                {
                    // Обновляем контекст для получения свежих данных
                    _context?.Dispose();
                    _context = new HousingStock();

                    LoadResidentApplications();
                    MessageBox.Show("Заявка успешно создана", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заявки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Обновляем контекст и загружаем данные
                _context?.Dispose();
                _context = new HousingStock();

                LoadResidentApplications();
                MessageBox.Show("Список заявок обновлен", "Обновление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            try
            {
                if (ApplicationsList.SelectedItem is ResidentApplication selectedApplication)
                {
                    string details = $"📋 Детали заявки #{selectedApplication.Id}\n\n";
                    details += $"🏠 Адрес: {selectedApplication.Address}\n";
                    details += $"🏷️ Категория: {selectedApplication.CategoryName}\n";
                    details += $"👷 Ответственный: {selectedApplication.AssignedEmployee}\n";
                    details += $"📊 Статус: {selectedApplication.Status}\n";
                    details += $"📅 Дата создания: {selectedApplication.CreateDate:dd.MM.yyyy HH:mm}\n";

                    if (selectedApplication.DaysInWork.HasValue)
                    {
                        details += $"⏱️ Дней в работе: {selectedApplication.DaysInWork}\n";
                    }

                    details += $"\n📝 Описание:\n{selectedApplication.Description}";

                    MessageBox.Show(details, "Детали заявки",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Выберите заявку для просмотра деталей",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateApplicationsDisplay();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedApplication();
        }

        private void EditSelectedApplication()
        {
            try
            {
                var selectedApplication = ApplicationsList.SelectedItem as ResidentApplication;
                if (selectedApplication == null)
                {
                    MessageBox.Show("Выберите заявку для редактирования",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Создаем объект заявки для редактирования
                var appForEdit = new ApplicationsPage.RepairApplication
                {
                    Id = selectedApplication.Id,
                    Address = selectedApplication.Address,
                    Description = selectedApplication.Description,
                    Status = selectedApplication.Status,
                    AssignedEmployee = selectedApplication.AssignedEmployee,
                    CreateDate = selectedApplication.CreateDate
                };

                var editWindow = new EditApplicationWindow(appForEdit);
                editWindow.Owner = Window.GetWindow(this);
                editWindow.Title = $"Редактирование заявки #{selectedApplication.Id}";

                if (editWindow.ShowDialog() == true)
                {
                    // Обновляем контекст и загружаем данные
                    _context?.Dispose();
                    _context = new HousingStock();

                    LoadResidentApplications();
                    MessageBox.Show($"Заявка #{selectedApplication.Id} обновлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка редактирования: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!applications.Any())
                {
                    MessageBox.Show("У вас пока нет заявок для анализа", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Рассчитываем статистику
                int totalApplications = applications.Count;
                int openCount = applications.Count(app => app.Status == "Открыта");
                int inProgressCount = applications.Count(app => app.Status == "В работе");
                int completedCount = applications.Count(app => app.Status == "Завершена");

                double averageDaysToComplete = applications
                    .Where(app => app.DaysInWork.HasValue && app.Status == "Завершена")
                    .Select(app => app.DaysInWork.Value)
                    .DefaultIfEmpty(0)
                    .Average();

                string stats = $"📊 Статистика ваших заявок\n\n";
                stats += $"📈 Всего заявок: {totalApplications}\n";
                stats += $"🟡 Открыто: {openCount}\n";
                stats += $"🟠 В работе: {inProgressCount}\n";
                stats += $"🟢 Завершено: {completedCount}\n";

                if (completedCount > 0)
                {
                    double completionRate = (double)completedCount / totalApplications * 100;
                    stats += $"📊 Процент завершения: {completionRate:F1}%\n";
                    stats += $"⏱️ Среднее время выполнения: {averageDaysToComplete:F1} дней\n";
                }

                stats += $"\n📋 По категориям:\n";
                var categories = applications
                    .GroupBy(app => app.CategoryName)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count);

                foreach (var category in categories)
                {
                    stats += $"  • {category.Category}: {category.Count}\n";
                }

                MessageBox.Show(stats, "Ваша статистика",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета статистики: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!applications.Any())
                {
                    MessageBox.Show("Нет данных для экспорта", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string exportText = $"Отчет по заявкам жителя: {CurrentUser.FullName}\n";
                exportText += $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n";

                foreach (var app in applications.OrderByDescending(a => a.CreateDate))
                {
                    exportText += $"Заявка #{app.Id}\n";
                    exportText += $"Адрес: {app.Address}\n";
                    exportText += $"Категория: {app.CategoryName}\n";
                    exportText += $"Ответственный: {app.AssignedEmployee}\n";
                    exportText += $"Статус: {app.Status}\n";
                    exportText += $"Дата создания: {app.CreateDate:dd.MM.yyyy HH:mm}\n";
                    if (app.DaysInWork.HasValue)
                    {
                        exportText += $"Дней в работе: {app.DaysInWork}\n";
                    }
                    exportText += $"Описание: {app.Description}\n";
                    exportText += new string('-', 40) + "\n";
                }

                // Сохраняем в файл
                string fileName = $"Мои_заявки_{DateTime.Now:yyyyMMdd_HHmm}.txt";
                System.IO.File.WriteAllText(fileName, exportText, System.Text.Encoding.UTF8);

                MessageBox.Show($"Отчет сохранен в файл: {fileName}", "Экспорт завершен",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
     }
}