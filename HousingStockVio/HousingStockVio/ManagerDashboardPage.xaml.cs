using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class ManagerDashboardPage : Page
    {
        private HousingStock _context;

        public ManagerDashboardPage()
        {
            InitializeComponent();
            Loaded += ManagerDashboardPage_Loaded;
            _context = new HousingStock();
        }

        private void ManagerDashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFinancialData();
            LoadStaffData();
            LoadTasksData();
            CalculateEfficiency();
        }

        private void LoadFinancialData()
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

                // Предполагаемые расходы: зарплаты, обслуживание домов, коммунальные
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

                MonthlyIncomeText.Text = $"{monthlyIncome:N0} руб.";
                MonthlyExpensesText.Text = $"{monthlyExpenses:N0} руб.";
                NetProfitText.Text = $"{netProfit:N0} руб.";
                ProfitabilityText.Text = $"{profitability:F1}%";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки финансовых данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStaffData()
        {
            try
            {
                // Всего сотрудников
                var totalStaff = _context.Employees.Count();

                // Активных сотрудников
                var activeStaff = _context.Employees.Count(emp => emp.Status == "Активен");

                // Средний стаж (в годах)
                var avgExperience = activeStaff > 0 ?
                    _context.Employees
                        .Where(emp => emp.Status == "Активен" && emp.HireDate != null)
                        .Average(emp => (DateTime.Now - emp.HireDate.Value).TotalDays / 365.25) : 0;

                TotalStaffText.Text = totalStaff.ToString();
                ActiveStaffText.Text = activeStaff.ToString();
                AvgExperienceText.Text = $"{avgExperience:F1} лет";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных персонала: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTasksData()
        {
            try
            {
                // Всего заявок
                var totalTasks = _context.Applications.Count();

                // Заявок в работе (статусы "В работе" и "Открыта")
                var inProgressTasks = _context.Applications
                    .Count(app => app.Status == "В работе" || app.Status == "Открыта");

                // Завершенных заявок
                var completedTasks = _context.Applications.Count(app => app.Status == "Завершена");

                TotalTasksText.Text = totalTasks.ToString();
                InProgressTasksText.Text = inProgressTasks.ToString();
                CompletedTasksText.Text = completedTasks.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных задач: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateEfficiency()
        {
            try
            {
                // Для примера используем фиксированные значения
                // В реальном приложении эти данные должны рассчитываться на основе реальных данных
                double resourceProfit = 1200000;
                double resourceCosts = 850000;
                double resourceProfitability = 0;

                if (resourceCosts > 0)
                {
                    resourceProfitability = (resourceProfit / resourceCosts * 100);
                }

                // Можно добавить реальные расчеты, если есть соответствующие данные в БД
                // Например:
                // var resourceProfit = _context.Payment.Sum(p => p.Paid_for ?? 0);
                // var resourceCosts = CalculateResourceCosts();

                ResourceProfitText.Text = $"{resourceProfit:N0} руб.";
                ResourceCostsText.Text = $"{resourceCosts:N0} руб.";
                ResourceProfitabilityText.Text = $"{resourceProfitability:F1}%";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета эффективности: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private double CalculateResourceCosts()
        {
            // Пример расчета затрат на ресурсы
            try
            {
                // Затраты на воду (предположим, что есть таблица WaterConsumption)
                // double waterCosts = _context.WaterConsumption.Sum(w => w.Cost ?? 0);

                // Затраты на электричество
                // double electricityCosts = _context.ElectricityConsumption.Sum(e => e.Cost ?? 0);

                // Затраты на материалы
                // double materialCosts = _context.Materials.Sum(m => m.Cost ?? 0);

                // return waterCosts + electricityCosts + materialCosts;

                return 850000; // Временное значение
            }
            catch
            {
                return 850000;
            }
        }

        private void ManageStaffButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //// Переход на страницу управления персоналом
                //var staffPage = new StaffManagementPage();
                //this.NavigationService.Navigate(staffPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия управления персоналом: {ex.Message}\n\n" +
                               "Функция находится в разработке.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PlanWorkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //// Переход на страницу планирования работ
                //var workPlanPage = new WorkPlanPage();
                //this.NavigationService.Navigate(workPlanPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия планирования работ: {ex.Message}\n\n" +
                               "Функция находится в разработке.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AnalyticalReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Загружаем дополнительные данные для отчета
                var currentMonth = DateTime.Now.ToString("MM.yyyy");

                // Доход за последние 6 месяцев
                var lastSixMonths = DateTime.Now.AddMonths(-6);
                var monthlyIncomeTrend = _context.Payment
                    .Where(p => p.Period != null)
                    .AsEnumerable()
                    .Where(p => IsDateInRange(p.Period, lastSixMonths, DateTime.Now))
                    .GroupBy(p => ExtractMonthYear(p.Period))
                    .Select(g => new
                    {
                        Period = g.Key,
                        Income = g.Sum(p => p.Paid_for ?? 0)
                    })
                    .OrderBy(x => x.Period)
                    .ToList();

                // Среднее время выполнения заявок
                var avgCompletionTime = _context.Applications
                    .Where(app => app.Status == "Завершена" && app.CompleteDate.HasValue)
                    .Select(app => (app.CompleteDate.Value - app.CreateDate).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average();

                // Процент завершенных заявок
                var totalApplications = _context.Applications.Count();
                var completedApplications = _context.Applications.Count(app => app.Status == "Завершена");
                var completionRate = totalApplications > 0 ? (double)completedApplications / totalApplications * 100 : 0;

                string report = "АНАЛИТИЧЕСКИЙ ОТЧЕТ\n\n" +
                               $"Дата: {DateTime.Now:dd.MM.yyyy}\n" +
                               $"Пользователь: {CurrentUser.FullName}\n\n" +
                               "ФИНАНСОВЫЕ ПОКАЗАТЕЛИ:\n" +
                               $"• Месячный доход: {MonthlyIncomeText.Text}\n" +
                               $"• Месячные расходы: {MonthlyExpensesText.Text}\n" +
                               $"• Чистая прибыль: {NetProfitText.Text}\n" +
                               $"• Рентабельность: {ProfitabilityText.Text}\n\n" +
                               "ЭФФЕКТИВНОСТЬ КОМПАНИИ:\n" +
                               $"• Прибыль от ресурсов: {ResourceProfitText.Text}\n" +
                               $"• Затраты на ресурсы: {ResourceCostsText.Text}\n" +
                               $"• Рентабельность ресурсов: {ResourceProfitabilityText.Text}\n\n" +
                               "ПЕРСОНАЛ:\n" +
                               $"• Всего сотрудников: {TotalStaffText.Text}\n" +
                               $"• Активных: {ActiveStaffText.Text}\n" +
                               $"• Средний стаж: {AvgExperienceText.Text}\n\n" +
                               "ПРОИЗВОДИТЕЛЬНОСТЬ:\n" +
                               $"• Всего заявок: {TotalTasksText.Text}\n" +
                               $"• В работе: {InProgressTasksText.Text}\n" +
                               $"• Завершено: {CompletedTasksText.Text}\n" +
                               $"• Процент завершения: {completionRate:F1}%\n" +
                               $"• Среднее время выполнения: {avgCompletionTime:F1} дней\n\n" +
                               "ВЫВОДЫ:\n" +
                               "• Показатели эффективности в пределах нормы\n" +
                               "• Рекомендуется оптимизация расходов\n" +
                               "• Необходимо увеличить сбор платежей\n" +
                               "• Улучшить скорость обработки заявок";

                MessageBox.Show(report, "Аналитический отчет",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void RefreshDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Обновляем контекст для получения свежих данных
                _context?.Dispose();
                _context = new HousingStock();

                LoadFinancialData();
                LoadStaffData();
                LoadTasksData();
                CalculateEfficiency();

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