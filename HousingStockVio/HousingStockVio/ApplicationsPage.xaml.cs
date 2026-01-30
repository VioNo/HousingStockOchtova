using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class ApplicationsPage : Page
    {
        private HousingStock _context;
        private List<RepairApplication> applications;

        public class RepairApplication
        {
            public int Id { get; set; }
            public string Address { get; set; }
            public string ApplicantName { get; set; }
            public string Phone { get; set; }
            public string Description { get; set; }
            public string Responsible { get; set; }
            public string Status { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime? CompleteDate { get; set; }
            public string AssignedEmployee { get; set; }
        }

        public ApplicationsPage()
        {
            InitializeComponent();
            Loaded += ApplicationsPage_Loaded;
            _context = new HousingStock();

            // Скрываем кнопки в зависимости от роли
            ConfigureButtonsForRole();
        }

        private void ConfigureButtonsForRole()
        {
            string role = CurrentUser.RoleName.ToLower();

            // Для жителей и сотрудников скрываем кнопки удаления
            if (role == "житель" || role == "сотрудник")
            {
                DeleteButton.Visibility = Visibility.Collapsed;
            }

            // Для жителей показываем только кнопку добавления
            if (role == "житель")
            {
                EditButton.Visibility = Visibility.Collapsed;
                StatusFilter.Visibility = Visibility.Collapsed;
                StatusText.Text = "Мои заявки";
            }
        }

        private void ApplicationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadApplications();
        }

        private void LoadApplications()
        {
            try
            {
                // Проверяем, существует ли таблица Applications в контексте
                if (!_context.Database.Exists() || !_context.Database.CompatibleWithModel(false))
                {
                    // Базы данных нет или несовместима, используем тестовые данные
                    CreateMockApplications();
                    MessageBox.Show(
                        "База данных не доступна или несовместима.\n" +
                        "Используются тестовые данные для демонстрации.",
                        "Внимание",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    // База данных доступна, загружаем данные с фильтрацией по роли
                    LoadApplicationsFromDatabase();
                }

                UpdateApplicationsDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке заявок: {ex.Message}\n" +
                    "Будут использованы тестовые данные.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                CreateMockApplications();
                UpdateApplicationsDisplay();
            }
        }

        private void LoadApplicationsFromDatabase()
        {
            try
            {
                string role = CurrentUser.RoleName.ToLower();
                string currentUserName = CurrentUser.FullName;

                IQueryable<Applications> query = _context.Applications;

                // Добавляем фильтрацию в зависимости от роли
                if (role == "сотрудник")
                {
                    query = query.Where(a =>
                        a.AssignedEmployee.Contains(currentUserName) ||
                        a.Responsible.Contains(currentUserName));
                }
                else if (role == "житель")
                {
                    query = query.Where(a => a.ApplicantName.Contains(currentUserName));
                }

                // Загружаем данные
                applications = query
                    .OrderByDescending(a => a.CreateDate)
                    .Select(a => new RepairApplication
                    {
                        Id = a.ID,
                        Address = a.Address,
                        ApplicantName = a.ApplicantName,
                        Phone = a.Phone,
                        Description = a.Description,
                        Responsible = a.Responsible,
                        Status = a.Status,
                        CreateDate = a.CreateDate,
                        CompleteDate = a.CompleteDate,
                        AssignedEmployee = a.AssignedEmployee
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки данных из базы: {ex.Message}", ex);
            }
        }

        private void CreateMockApplications()
        {
            string role = CurrentUser.RoleName.ToLower();
            string currentUserName = CurrentUser.FullName;

            // Базовый список тестовых заявок
            var allApplications = new List<RepairApplication>
            {
                new RepairApplication
                {
                    Id = 1,
                    Address = "ул. Ленина, 10, кв. 5",
                    ApplicantName = "Иванов Иван Иванович",
                    Phone = "+7 (999) 111-11-11",
                    Description = "Протекает кран на кухне. Необходима замена смесителя.",
                    Responsible = "Петров П.П.",
                    AssignedEmployee = "Петров П.П.",
                    Status = "Открыта",
                    CreateDate = DateTime.Now.AddDays(-5)
                },
                new RepairApplication
                {
                    Id = 2,
                    Address = "ул. Советская, 25, кв. 42",
                    ApplicantName = "Сидоров Сергей Сергеевич",
                    Phone = "+7 (999) 222-22-22",
                    Description = "Не работает розетка в зале. Требуется замена электропроводки.",
                    Responsible = "Петров П.П.",
                    AssignedEmployee = "Петров П.П.",
                    Status = "В работе",
                    CreateDate = DateTime.Now.AddDays(-3)
                },
                new RepairApplication
                {
                    Id = 3,
                    Address = "пр. Мира, 15, кв. 17",
                    ApplicantName = "Козлова Мария Дмитриевна",
                    Phone = "+7 (999) 333-33-33",
                    Description = "Трещина в стене между кухней и гостиной. Требуется штукатурка и покраска.",
                    Responsible = "Сидоров С.С.",
                    AssignedEmployee = "Сидоров С.С.",
                    Status = "Завершена",
                    CreateDate = DateTime.Now.AddDays(-10),
                    CompleteDate = DateTime.Now.AddDays(-2)
                },
                new RepairApplication
                {
                    Id = 4,
                    Address = "ул. Пушкина, 5, кв. 8",
                    ApplicantName = "Николаев Николай Николаевич",
                    Phone = "+7 (999) 444-44-44",
                    Description = "Забита канализация в ванной комнате. Требуется прочистка труб.",
                    Responsible = "Иванов И.И.",
                    AssignedEmployee = "Иванов И.И.",
                    Status = "В работе",
                    CreateDate = DateTime.Now.AddDays(-2)
                }
            };

            // Фильтруем заявки в зависимости от роли
            if (role == "сотрудник")
            {
                // Для сотрудника показываем только его заявки
                applications = allApplications
                    .Where(app =>
                        (!string.IsNullOrEmpty(app.AssignedEmployee) && app.AssignedEmployee.Contains(currentUserName)) ||
                        (!string.IsNullOrEmpty(app.Responsible) && app.Responsible.Contains(currentUserName)))
                    .ToList();

                // Если у сотрудника нет заявок, показываем пример
                if (applications.Count == 0)
                {
                    applications.Add(new RepairApplication
                    {
                        Id = 5,
                        Address = "ул. Гагарина, 12",
                        ApplicantName = "Пример заявителя",
                        Phone = "+7 (999) 555-55-55",
                        Description = "Пример заявки для сотрудника " + currentUserName,
                        Responsible = currentUserName,
                        AssignedEmployee = currentUserName,
                        Status = "В работе",
                        CreateDate = DateTime.Now.AddDays(-1)
                    });
                }
            }
            else if (role == "житель")
            {
                // Для жителя показываем только его заявки
                applications = allApplications
                    .Where(app => !string.IsNullOrEmpty(app.ApplicantName) &&
                           app.ApplicantName.Contains(currentUserName))
                    .ToList();

                // Если у жителя нет заявок, показываем пример
                if (applications.Count == 0)
                {
                    applications.Add(new RepairApplication
                    {
                        Id = 6,
                        Address = "Ваш адрес",
                        ApplicantName = currentUserName,
                        Phone = "+7 (999) 000-00-00",
                        Description = "Пример вашей заявки",
                        Responsible = "Сотрудник компании",
                        AssignedEmployee = "Сотрудник компании",
                        Status = "Открыта",
                        CreateDate = DateTime.Now
                    });
                }
            }
            else
            {
                // Для администратора и руководителя показываем все заявки
                applications = allApplications;
            }
        }

        private void UpdateApplicationsDisplay()
        {
            try
            {
                ApplyCurrentFilter();
                UpdateStatusDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении отображения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyCurrentFilter()
        {
            try
            {
                if (ApplicationsList == null || applications == null)
                {
                    return;
                }

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

                ApplicationsList.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении фильтра: {ex.Message}",
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

        private void UpdateStatusDisplay()
        {
            try
            {
                if (StatusText == null || applications == null)
                {
                    return;
                }

                var displayedItems = ApplicationsList?.ItemsSource as IEnumerable<RepairApplication>;
                int displayedCount = displayedItems?.Count() ?? applications.Count;
                int totalCount = applications.Count;

                string role = CurrentUser.RoleName.ToLower();

                if (role == "житель")
                {
                    StatusText.Text = $"Мои заявки: {displayedCount}";
                }
                else if (role == "сотрудник")
                {
                    StatusText.Text = $"Мои заявки: {displayedCount} из {totalCount}";
                }
                else
                {
                    if (displayedCount == totalCount)
                    {
                        StatusText.Text = $"Всего заявок: {totalCount}";
                    }
                    else
                    {
                        StatusText.Text = $"Показано: {displayedCount} из {totalCount} заявок";
                    }
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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EditApplicationWindow addWindow = new EditApplicationWindow();
                addWindow.Owner = Window.GetWindow(this);
                addWindow.Title = "Добавление новой заявки";

                bool? result = addWindow.ShowDialog();

                if (result == true)
                {
                    // Обновляем контекст и загружаем данные
                    _context = new HousingStock();
                    LoadApplications();
                    MessageBox.Show("Новая заявка успешно добавлена!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении заявки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedApplication();
        }

        private void ApplicationsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditSelectedApplication();
        }

        private void EditSelectedApplication()
        {
            try
            {
                RepairApplication selectedApplication = ApplicationsList.SelectedItem as RepairApplication;

                if (selectedApplication == null)
                {
                    MessageBox.Show("Пожалуйста, выберите заявку для редактирования.",
                        "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем права доступа
                string role = CurrentUser.RoleName.ToLower();
                if (role == "житель")
                {
                    // Житель может редактировать только свои заявки
                    if (!selectedApplication.ApplicantName.Contains(CurrentUser.FullName))
                    {
                        MessageBox.Show("Вы можете редактировать только свои заявки.",
                            "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else if (role == "сотрудник")
                {
                    // Сотрудник может редактировать только свои заявки
                    if ((!string.IsNullOrEmpty(selectedApplication.AssignedEmployee) &&
                         !selectedApplication.AssignedEmployee.Contains(CurrentUser.FullName)) &&
                        (!string.IsNullOrEmpty(selectedApplication.Responsible) &&
                         !selectedApplication.Responsible.Contains(CurrentUser.FullName)))
                    {
                        MessageBox.Show("Вы можете редактировать только заявки, назначенные вам.",
                            "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                EditApplicationWindow editWindow = new EditApplicationWindow(selectedApplication);
                editWindow.Owner = Window.GetWindow(this);
                editWindow.Title = $"Редактирование заявки #{selectedApplication.Id}";

                bool? result = editWindow.ShowDialog();

                if (result == true)
                {
                    // Обновляем контекст и загружаем данные
                    _context = new HousingStock();
                    LoadApplications();
                    MessageBox.Show($"Заявка #{selectedApplication.Id} успешно обновлена!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании заявки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RepairApplication selectedApplication = ApplicationsList.SelectedItem as RepairApplication;

                if (selectedApplication == null)
                {
                    MessageBox.Show("Пожалуйста, выберите заявку для удаления.",
                        "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем права доступа
                string role = CurrentUser.RoleName.ToLower();
                if (role != "администратор" && role != "руководитель")
                {
                    MessageBox.Show("У вас нет прав для удаления заявок.",
                        "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBoxResult confirmation = MessageBox.Show(
                    $"Вы действительно хотите удалить заявку #{selectedApplication.Id}?\n\n" +
                    $"Адрес: {selectedApplication.Address}\n" +
                    $"Заявитель: {selectedApplication.ApplicantName}\n" +
                    $"Статус: {selectedApplication.Status}\n\n" +
                    "Это действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);

                if (confirmation == MessageBoxResult.Yes)
                {
                    // Удаляем из базы данных
                    DeleteApplicationFromDatabase(selectedApplication.Id);

                    // Удаляем из локального списка
                    applications.Remove(selectedApplication);

                    // Обновляем отображение
                    UpdateApplicationsDisplay();

                    MessageBox.Show($"Заявка #{selectedApplication.Id} успешно удалена!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении заявки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteApplicationFromDatabase(int applicationId)
        {
            try
            {
                var application = _context.Applications.Find(applicationId);
                if (application != null)
                {
                    _context.Applications.Remove(application);
                    _context.SaveChanges();
                }
                else
                {
                    throw new Exception("Заявка не найдена в базе данных");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при удалении заявки из базы данных: {ex.Message}", ex);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Обновляем контекст и загружаем данные
                _context?.Dispose();
                _context = new HousingStock();

                LoadApplications();
                MessageBox.Show("Список заявок успешно обновлен!",
                    "Обновление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка заявок: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UpdateApplicationsDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении фильтра: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
    }
}