using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class EmployeeMainWindow : Window
    {
        private HousingStock _context;

        public EmployeeMainWindow()
        {
            InitializeComponent();
            _context = new HousingStock();
            UserNameText.Text = CurrentUser.FullName;
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                string currentUserName = CurrentUser.FullName;

                // Загружаем статистику по заявкам сотрудника
                var myApplications = _context.Applications
                    .Where(a =>
                        (!string.IsNullOrEmpty(a.Responsible) && a.Responsible.Contains(currentUserName)) ||
                        (!string.IsNullOrEmpty(a.AssignedEmployee) && a.AssignedEmployee.Contains(currentUserName)))
                    .ToList();

                int totalApplications = myApplications.Count;
                int completedCount = myApplications.Count(a => a.Status == "Завершена");
                int inProgressCount = myApplications.Count(a => a.Status == "В работе");
                int openCount = myApplications.Count(a => a.Status == "Открыта");

                // Получаем последние 5 заявок
                var recentApplications = myApplications
                    .OrderByDescending(a => a.CreateDate)
                    .Take(5)
                    .ToList();

                // Формируем текст для дашборда
                string applicationsText = "";
                for (int i = 0; i < recentApplications.Count; i++)
                {
                    var app = recentApplications[i];
                    applicationsText += $"{i + 1}. Заявка #{app.ID} - {app.Address} ({app.Status})\n";
                }

                var textBlock = new TextBlock
                {
                    Text = $"Добро пожаловать, {CurrentUser.FullName}!\nВы вошли как сотрудник.\n\n" +
                          "Ваши заявки:\n" + applicationsText + "\n" +
                          $"Всего заявок: {totalApplications}\n" +
                          $"Выполнено: {completedCount}\n" +
                          $"В работе: {inProgressCount}\n" +
                          $"Открыто: {openCount}",
                    FontSize = 16,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var page = new Page();
                page.Content = textBlock;
                MainFrame.Navigate(page);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\nИспользуются тестовые данные.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoadMockDashboardData();
            }
        }

        private void LoadMockDashboardData()
        {
            var textBlock = new TextBlock
            {
                Text = $"Добро пожаловать, {CurrentUser.FullName}!\nВы вошли как сотрудник.\n\n" +
                      "Ваши заявки:\n" +
                      "1. Заявка #45 - ул. Ленина, 10 (В работе)\n" +
                      "2. Заявка #42 - пр. Мира, 15 (Открыта)\n" +
                      "3. Заявка #38 - ул. Советская, 25 (Завершена)\n\n" +
                      "Всего заявок: 3\nВыполнено: 1\nВ работе: 2",
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var page = new Page();
            page.Content = textBlock;
            MainFrame.Navigate(page);
        }

        private void MyApplicationsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Используем страницу с заявками сотрудника
                var applicationsPage = new EmployeeApplicationsPage();
                MainFrame.Navigate(applicationsPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WorkScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Загружаем реальные задачи сотрудника
                string currentUserName = CurrentUser.FullName;

                var currentTasks = _context.Applications
                    .Where(a =>
                        (!string.IsNullOrEmpty(a.AssignedEmployee) && a.AssignedEmployee.Contains(currentUserName)) &&
                        (a.Status == "Открыта" || a.Status == "В работе"))
                    .OrderBy(a => a.CreateDate)
                    .Take(10)
                    .ToList();

                string tasksText = "";
                int taskNumber = 1;
                foreach (var task in currentTasks)
                {
                    tasksText += $"{taskNumber}. {task.Description} - {task.Address}\n";
                    taskNumber++;
                }

                if (string.IsNullOrEmpty(tasksText))
                {
                    tasksText = "Нет текущих задач";
                }

                var textBlock = new TextBlock
                {
                    Text = "Ваш график работ:\n\n" +
                          "Понедельник: 9:00-18:00\n" +
                          "Вторник: 9:00-18:00\n" +
                          "Среда: 9:00-18:00\n" +
                          "Четверг: 9:00-18:00\n" +
                          "Пятница: 9:00-17:00\n\n" +
                          "Текущие задачи:\n" + tasksText,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var page = new Page();
                page.Content = textBlock;
                MainFrame.Navigate(page);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки графика работ: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                var textBlock = new TextBlock
                {
                    Text = "Ваш график работ:\n\n" +
                          "Понедельник: 9:00-18:00\n" +
                          "Вторник: 9:00-18:00\n" +
                          "Среда: 9:00-18:00\n" +
                          "Четверг: 9:00-18:00\n" +
                          "Пятница: 9:00-17:00\n\n" +
                          "Текущие задачи:\n" +
                          "1. Ремонт крана - ул. Ленина, 10\n" +
                          "2. Замена розетки - пр. Мира, 15",
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var page = new Page();
                page.Content = textBlock;
                MainFrame.Navigate(page);
            }
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string currentUserName = CurrentUser.FullName;

                var myApplications = _context.Applications
                    .Where(a =>
                        (!string.IsNullOrEmpty(a.Responsible) && a.Responsible.Contains(currentUserName)) ||
                        (!string.IsNullOrEmpty(a.AssignedEmployee) && a.AssignedEmployee.Contains(currentUserName)))
                    .ToList();

                if (!myApplications.Any())
                {
                    MessageBox.Show("У вас пока нет заявок для анализа статистики.",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                int totalApplications = myApplications.Count;
                int completedCount = myApplications.Count(a => a.Status == "Завершена");
                int inProgressCount = myApplications.Count(a => a.Status == "В работе");
                int openCount = myApplications.Count(a => a.Status == "Открыта");

                // Среднее время выполнения завершенных заявок
                var completedApplications = myApplications
                    .Where(a => a.Status == "Завершена" && a.CompleteDate.HasValue)
                    .ToList();

                double avgCompletionDays = completedApplications.Any() ?
                    completedApplications.Average(a => (a.CompleteDate.Value - a.CreateDate).TotalDays) : 0;

                // Распределение по категориям
                var categoryStats = myApplications
                    .Where(a => a.ServiceCategories != null)
                    .GroupBy(a => a.ServiceCategories.CategoryName)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                string categoryText = "";
                foreach (var stat in categoryStats)
                {
                    categoryText += $"- {stat.Category}: {stat.Count} заявок\n";
                }

                if (string.IsNullOrEmpty(categoryText))
                {
                    categoryText = "Нет данных по категориям";
                }

                MessageBox.Show(
                    $"Статистика по вашим заявкам:\n\n" +
                    $"Общая статистика:\n" +
                    $"• Всего заявок: {totalApplications}\n" +
                    $"• Завершено: {completedCount} ({(totalApplications > 0 ? (double)completedCount / totalApplications * 100 : 0):F1}%)\n" +
                    $"• В работе: {inProgressCount}\n" +
                    $"• Открыто: {openCount}\n\n" +
                    $"Эффективность:\n" +
                    $"• Среднее время выполнения: {avgCompletionDays:F1} дней\n\n" +
                    $"Распределение по категориям:\n{categoryText}",
                    "Ваша статистика",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Для изменения статуса заявки:\n" +
                           "1. Перейдите в раздел 'Мои заявки'\n" +
                           "2. Выберите заявку из списка\n" +
                           "3. Нажмите кнопку 'Изменить статус'\n" +
                           "4. Выберите новый статус в диалоговом окне\n\n" +
                           "Доступные статусы:\n" +
                           "• Открыта\n" +
                           "• В работе\n" +
                           "• Завершена\n" +
                           "• Отменена",
                           "Как изменить статус заявки",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы действительно хотите выйти из системы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CurrentUser.Clear();
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}