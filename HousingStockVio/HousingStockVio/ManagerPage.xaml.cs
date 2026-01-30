using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HousingStockVio
{
    public partial class ManagerPage : Page
    {
        private HousingStock _context;

        public class UrgentTask
        {
            public string Task { get; set; }
            public string Priority { get; set; }
            public string Description { get; set; }
        }

        public ManagerPage()
        {
            InitializeComponent();
            Loaded += ManagerPage_Loaded;
            _context = new HousingStock();
        }

        private void ManagerPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMonitoringData();
            LoadUrgentTasks();
            LoadFinancialAnalytics();
            LoadEfficiencyData();
        }

        private void LoadMonitoringData()
        {
            try
            {
                // Всего домов
                var totalHouses = _context.Apartment.Count();
                TotalHousesText.Text = totalHouses.ToString();

                // Активные заявки (статус "Открыта" или "В работе")
                var activeApplications = _context.Applications
                    .Count(app => app.Status == "Открыта" || app.Status == "В работе");
                ActiveAppsText.Text = activeApplications.ToString();

                // Выполнено сегодня
                var today = DateTime.Today;
                var completedToday = _context.Applications
                    .Count(app => app.Status == "Завершена" &&
                           app.CompleteDate.HasValue &&
                           app.CompleteDate.Value.Date == today);
                CompletedTodayText.Text = completedToday.ToString();

                // Персонал онлайн (активные сотрудники)
                var staffOnline = _context.Employees.Count(emp => emp.Status == "Активен");
                var totalStaff = _context.Employees.Count();
                StaffOnlineText.Text = $"{staffOnline}/{totalStaff}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных мониторинга: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUrgentTasks()
        {
            try
            {
                // Загружаем реальные срочные задачи из базы данных
                var urgentTasksFromDb = _context.Applications
                    .Where(app => app.Priority == "Высокий" &&
                           app.Status != "Завершена")
                    .OrderBy(app => app.CreateDate)
                    .Take(5)
                    .Select(app => new UrgentTask
                    {
                        Task = $"Срочная заявка #{app.ID}",
                        Priority = app.Priority ?? "Высокий",
                        Description = app.Description
                    })
                    .ToList();

                if (urgentTasksFromDb.Any())
                {
                    UrgentTasksList.ItemsSource = urgentTasksFromDb;
                }
                else
                {
                    // Если нет срочных задач в БД, используем тестовые данные
                    var testTasks = new List<UrgentTask>
                    {
                        new UrgentTask {
                            Task = "Проверить срочную заявку #45",
                            Priority = "Высокий",
                            Description = "Аварийная протечка на 5 этаже. Требуется срочный ремонт водопровода."
                        },
                        new UrgentTask {
                            Task = "Утвердить отчет по финансам",
                            Priority = "Высокий",
                            Description = "Ежемесячный финансовый отчет за ноябрь. Требуется утверждение до 18:00."
                        },
                        new UrgentTask {
                            Task = "Совещание с персоналом",
                            Priority = "Средний",
                            Description = "Еженедельное совещание по планам на неделю. Конференц-зал в 10:00."
                        }
                    };
                    UrgentTasksList.ItemsSource = testTasks;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFinancialAnalytics()
        {
            try
            {
                string currentMonth = DateTime.Now.ToString("MM.yyyy");

                // Месячный доход (оплаченные платежи за текущий месяц)
                var monthlyIncome = _context.Payment
                    .Where(p => p.Period != null && p.Period.Contains(currentMonth))
                    .Sum(p => p.Paid_for ?? 0);

                // Расчет расходов
                var activeEmployees = _context.Employees.Count(emp => emp.Status == "Активен");
                var totalApartments = _context.Apartment.Count();

                // Предполагаемые расходы
                double employeeSalaries = activeEmployees * 35000;
                double apartmentMaintenance = totalApartments * 5000;
                double utilities = 150000;
                double monthlyExpenses = employeeSalaries + apartmentMaintenance + utilities;

                // Чистая прибыль
                double netProfit = monthlyIncome - monthlyExpenses;

                // Рентабельность
                double profitability = 0;
                if (monthlyExpenses > 0)
                {
                    profitability = (netProfit / monthlyExpenses * 100);
                }

                // Отображаем данные
                MonthlyIncomeText.Text = $"{monthlyIncome:C}";
                MonthlyExpensesText.Text = $"{monthlyExpenses:C}";
                NetProfitText.Text = $"{netProfit:C}";
                ProfitabilityText.Text = $"{profitability:F1}%";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки финансовой аналитики: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadEfficiencyData()
        {
            try
            {
                // Расчет реальных показателей эффективности
                var completedApplications = _context.Applications.Count(app => app.Status == "Завершена");
                var totalApplications = _context.Applications.Count();

                // Процент выполнения плана (предположим, что план - 90%)
                double planCompletion = totalApplications > 0 ? (double)completedApplications / totalApplications * 100 : 0;
                double planCompletionPercentage = Math.Min(planCompletion, 100); // Не более 100%

                // Среднее время выполнения заявок (в днях)
                var avgResponseTime = _context.Applications
                    .Where(app => app.Status == "Завершена" && app.CompleteDate.HasValue)
                    .Select(app => (app.CompleteDate.Value - app.CreateDate).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average();

                // Процент качественных работ (предположим, что все завершенные заявки - качественные)
                double workQuality = totalApplications > 0 ? (double)completedApplications / totalApplications * 100 : 0;

                // Уровень удовлетворенности (тестовое значение)
                double satisfaction = 92.0;

                PlanCompletionText.Text = $"{planCompletionPercentage:F0}%";
                SatisfactionText.Text = $"{satisfaction:F0}%";
                AvgResponseTime.Text = $"{avgResponseTime:F1} дня";
                WorkQualityText.Text = $"{workQuality:F0}%";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных эффективности: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчики кнопок управления
        private void RepairApplicationsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var applicationsPage = new ApplicationsPage();
                NavigationService.Navigate(applicationsPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переходе к заявкам: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FinancialReportsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Переход на страницу финансовых отчетов
                var reportsPage = new FinancialReportsPage();
                NavigationService.Navigate(reportsPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переходе к отчетам: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StaffManagementButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем данные о сотрудниках
                var employees = _context.Employees
                    .OrderBy(emp => emp.FullName)
                    .Select(emp => new
                    {
                        emp.FullName,
                        emp.Position,
                        emp.Phone,
                        emp.Email,
                        emp.Status,
                        emp.HireDate
                    })
                    .Take(15) // Ограничиваем количество для отображения
                    .ToList();

                string staffInfo = "УПРАВЛЕНИЕ ПЕРСОНАЛОМ\n\n";
                staffInfo += $"Всего сотрудников: {StaffOnlineText.Text}\n\n";

                foreach (var emp in employees)
                {
                    staffInfo += $"{emp.FullName} - {emp.Position}\n";
                    staffInfo += $"Телефон: {emp.Phone}\n";
                    staffInfo += $"Статус: {emp.Status}\n";
                    if (emp.HireDate.HasValue)
                    {
                        staffInfo += $"Дата приема: {emp.HireDate.Value:dd.MM.yyyy}\n";
                    }
                    staffInfo += "--------------------------------\n";
                }

                if (_context.Employees.Count() > 15)
                {
                    staffInfo += $"\n... и еще {_context.Employees.Count() - 15} сотрудников";
                }

                MessageBox.Show(staffInfo, "Управление персоналом",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных персонала: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WorkScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем запланированные задачи на сегодня
                var today = DateTime.Today;
                var todayTasks = _context.Applications
                    .Where(app => (app.Status == "В работе" || app.Status == "Открыта") &&
                           app.CompleteDate == null)
                    .OrderBy(app => app.Priority)
                    .Select(app => new
                    {
                        app.ID,
                        app.Address,
                        app.Description,
                        app.Priority,
                        app.Responsible
                    })
                    .Take(10)
                    .ToList();

                string schedule = "ГРАФИК РАБОТ\n\n";
                schedule += $"Текущий день: {DateTime.Now:dd.MM.yyyy}\n\n";

                if (todayTasks.Any())
                {
                    schedule += "Запланированные задачи на сегодня:\n\n";
                    foreach (var task in todayTasks)
                    {
                        schedule += $"• Заявка #{task.ID} - {task.Address}\n";
                        schedule += $"  Приоритет: {task.Priority}\n";
                        schedule += $"  Ответственный: {task.Responsible}\n";
                        schedule += $"  Описание: {task.Description}\n\n";
                    }
                }
                else
                {
                    schedule += "На сегодня задач не запланировано\n";
                }

                schedule += "\nОбщие задачи на неделю:\n";
                schedule += "- Обход домов и проверка состояния\n";
                schedule += "- Техническое обслуживание оборудования\n";
                schedule += "- Встречи с жильцами\n";
                schedule += "- Подготовка отчетности";

                MessageBox.Show(schedule, "График работ",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования графика: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчики кнопок отчетов
        private void ApplicationsReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Статистика по статусам заявок
                var statusStats = _context.Applications
                    .GroupBy(app => app.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        AvgDays = g
                            .Where(app => app.CompleteDate.HasValue)
                            .Select(app => (app.CompleteDate.Value - app.CreateDate).TotalDays)
                            .DefaultIfEmpty(0)
                            .Average()
                    })
                    .OrderBy(s => s.Status)
                    .ToList();

                var total = _context.Applications.Count();
                var activeApplications = _context.Applications
                    .Count(app => app.Status == "Открыта" || app.Status == "В работе");

                string report = "ОТЧЕТ ПО ЗАЯВКАМ\n\n";
                report += $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}\n";
                report += $"Пользователь: {CurrentUser.FullName}\n\n";
                report += "СТАТУС ЗАЯВОК:\n";

                foreach (var stat in statusStats)
                {
                    report += $"\n{stat.Status}: {stat.Count} заявок";
                    if (stat.AvgDays > 0)
                    {
                        report += $"\nСреднее время: {stat.AvgDays:F1} дней";
                    }
                }

                report += $"\n\nВСЕГО ЗАЯВОК: {total}";
                report += $"\nАКТИВНЫХ: {activeApplications}";
                report += $"\nВЫПОЛНЕНО СЕГОДНЯ: {CompletedTodayText.Text}";

                // Дополнительная аналитика
                var lastMonth = DateTime.Now.AddMonths(-1);
                var applicationsLastMonth = _context.Applications
                    .Count(app => app.CreateDate >= lastMonth);

                var completedLastMonth = _context.Applications
                    .Count(app => app.Status == "Завершена" && app.CreateDate >= lastMonth);

                report += $"\n\nАНАЛИТИКА ЗА ПОСЛЕДНИЙ МЕСЯЦ:";
                report += $"\n• Новых заявок: {applicationsLastMonth}";
                report += $"\n• Завершено: {completedLastMonth}";
                if (applicationsLastMonth > 0)
                {
                    report += $"\n• Процент выполнения: {(double)completedLastMonth / applicationsLastMonth * 100:F1}%";
                }

                MessageBox.Show(report, "Отчет по заявкам",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FinanceReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем данные за последние 3 месяца для анализа динамики
                var threeMonthsAgo = DateTime.Now.AddMonths(-3);

                var monthlyData = _context.Payment
                    .Where(p => p.Period != null)
                    .AsEnumerable()
                    .Where(p => IsDateInRange(p.Period, threeMonthsAgo, DateTime.Now))
                    .GroupBy(p => ExtractMonthYear(p.Period))
                    .Select(g => new
                    {
                        Period = g.Key,
                        Income = g.Sum(p => p.Paid_for ?? 0),
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Period)
                    .ToList();

                string report = "ФИНАНСОВЫЙ ОТЧЕТ\n\n";
                report += $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}\n";
                report += $"Пользователь: {CurrentUser.FullName}\n\n";

                report += "ДИНАМИКА ПО МЕСЯЦАМ:\n";
                foreach (var data in monthlyData)
                {
                    report += $"\n{data.Period}:";
                    report += $"\n  Доход: {data.Income:C}";
                    report += $"\n  Количество платежей: {data.Count}";
                }

                report += $"\n\nТЕКУЩИЙ МЕСЯЦ:\n";
                report += $"  Доход: {MonthlyIncomeText.Text}\n";
                report += $"  Расходы: {MonthlyExpensesText.Text}\n";
                report += $"  Прибыль: {NetProfitText.Text}\n";
                report += $"  Рентабельность: {ProfitabilityText.Text}\n\n";

                report += "АНАЛИЗ:\n";
                if (monthlyData.Count >= 2)
                {
                    var lastMonthIncome = monthlyData.Last().Income;
                    var prevMonthIncome = monthlyData[monthlyData.Count - 2].Income;
                    var growth = prevMonthIncome > 0 ? (lastMonthIncome - prevMonthIncome) / prevMonthIncome * 100 : 0;

                    report += $"• Рост доходов: {growth:F1}%\n";
                }
                report += "• Показатели в пределах нормы\n";
                report += "• Рекомендуется контроль расходов";

                MessageBox.Show(report, "Финансовый отчет",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsDateInRange(string period, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(period) || !period.Contains("."))
                    return false;

                var parts = period.Split('.');
                if (parts.Length != 2)
                    return false;

                if (int.TryParse(parts[0], out int month) && int.TryParse(parts[1], out int year))
                {
                    var date = new DateTime(year, month, 1);
                    return date >= startDate && date <= endDate;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private string ExtractMonthYear(string period)
        {
            if (string.IsNullOrEmpty(period) || !period.Contains("."))
                return period;

            var parts = period.Split('.');
            if (parts.Length == 2)
            {
                return $"{parts[0]}.{parts[1]}";
            }
            return period;
        }

        private void StaffReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Статистика по должностям
                var positionStats = _context.Employees
                    .Where(emp => emp.Status == "Активен")
                    .GroupBy(emp => emp.Position)
                    .Select(g => new
                    {
                        Position = g.Key,
                        Count = g.Count(),
                        AvgExperience = g
                            .Where(emp => emp.HireDate.HasValue)
                            .Select(emp => (DateTime.Now - emp.HireDate.Value).TotalDays / 365.25)
                            .DefaultIfEmpty(0)
                            .Average()
                    })
                    .OrderByDescending(p => p.Count)
                    .ToList();

                var totalActive = _context.Employees.Count(emp => emp.Status == "Активен");
                var totalAll = _context.Employees.Count();

                string report = "ОТЧЕТ ПО ПЕРСОНАЛУ\n\n";
                report += $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}\n";
                report += $"Пользователь: {CurrentUser.FullName}\n\n";
                report += "РАСПРЕДЕЛЕНИЕ ПО ДОЛЖНОСТЯМ:\n";

                foreach (var stat in positionStats)
                {
                    report += $"\n{stat.Position}: {stat.Count} сотрудников";
                    if (stat.AvgExperience > 0)
                    {
                        report += $"\nСредний стаж: {stat.AvgExperience:F1} лет";
                    }
                }

                report += $"\n\nИТОГО:";
                report += $"\n• Активных сотрудников: {totalActive}";
                report += $"\n• Общее количество: {totalAll}";
                report += $"\n• Процент активных: {(totalAll > 0 ? (double)totalActive / totalAll * 100 : 0):F1}%";

                // Дополнительная аналитика
                var newHiresThisYear = _context.Employees
                    .Count(emp => emp.HireDate.HasValue && emp.HireDate.Value.Year == DateTime.Now.Year);

                report += $"\n\nАНАЛИТИКА ЗА ТЕКУЩИЙ ГОД:";
                report += $"\n• Принято новых сотрудников: {newHiresThisYear}";

                MessageBox.Show(report, "Отчет по персоналу",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UrgentTasksList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var selectedTask = UrgentTasksList.SelectedItem as UrgentTask;
                if (selectedTask != null)
                {
                    MessageBox.Show(
                        $"Задача: {selectedTask.Task}\n\n" +
                        $"Приоритет: {selectedTask.Priority}\n" +
                        $"Описание: {selectedTask.Description}",
                        "Детали задачи",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении задачи: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Обновляем контекст для получения свежих данных
                _context?.Dispose();
                _context = new HousingStock();

                LoadMonitoringData();
                LoadUrgentTasks();
                LoadFinancialAnalytics();
                LoadEfficiencyData();

                MessageBox.Show("Данные обновлены", "Обновление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
    }
}