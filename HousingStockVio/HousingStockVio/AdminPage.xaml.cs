using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class AdminPage : Page
    {

        private HousingStock context;

        public class RecentApplication
        {
            public int Id { get; set; }
            public string Address { get; set; }
            public string Status { get; set; }
            public DateTime CreateDate { get; set; }
            public string ApplicantName { get; set; }
            public string Phone { get; set; }
            public string Description { get; set; }
        }

        public class FinancialInfo
        {
            public double MonthlyIncome { get; set; }  
            public double MonthlyExpenses { get; set; }
            public double NetProfit { get; set; }      
            public double Profitability { get; set; }
            public string Efficiency { get; set; }
        }

        public AdminPage()
        {
            InitializeComponent();
            Loaded += AdminPage_Loaded;
            Unloaded += AdminPage_Unloaded;
        }

        private void AdminPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            { 
                context = new HousingStock();
                LoadStatistics();
                LoadRecentApplications();
                LoadFinancialInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AdminPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Освобождение ресурсов контекста при разгрузке страницы
            context?.Dispose();
        }

        private void LoadStatistics()
        {
            try
            {
                // Проверка, что контекст инициализирован
                if (context == null) return;

                // Всего домов - используем Count() из Entity Framework
                int totalHouses = context.Apartment.Count();
                TotalHousesText.Text = totalHouses.ToString();

                // Всего владельцев
                int totalOwners = context.Owners.Count();
                TotalOwnersText.Text = totalOwners.ToString();

                // Активные заявки (статус "Открыта" или "В работе")
                int activeApplications = context.Applications
                    .Count(a => a.Status == "Открыта" || a.Status == "В работе");
                TotalApplicationsText.Text = activeApplications.ToString();

                // Общая задолженность
                // Используем Sum() с обработкой null-значений через ?? 0
                double totalDebt = context.Debt
                    .Sum(d => (d.Water ?? 0) + (d.Electric_power ?? 0));
                TotalDebtText.Text = $"{totalDebt:C}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRecentApplications()
        {
            try
            {
                if (context == null) return;

                // Получение 5 последних заявок через LINQ к Entity Framework
                var applications = context.Applications
                    .OrderByDescending(a => a.CreateDate)  // Сортировка по дате создания
                    .Take(5)  // Берем только 5 записей
                    .Select(a => new RecentApplication
                    {
                        Id = a.ID,
                        Address = a.Address,
                        ApplicantName = a.ApplicantName,
                        Phone = a.Phone,
                        Description = a.Description,
                        Status = a.Status,
                        CreateDate = a.CreateDate
                    })
                    .ToList();

                // Если нет данных, показываем информационное сообщение
                if (applications.Count == 0)
                {
                    applications.Add(new RecentApplication
                    {
                        Id = 0,
                        Address = "Нет данных",
                        Status = "-",
                        ApplicantName = "-",
                        Phone = "-",
                        Description = "Нет заявок в базе данных",
                        CreateDate = DateTime.Now
                    });
                }

                RecentApplicationsList.ItemsSource = applications;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки последних заявок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFinancialInfo()
        {
            try
            {
                if (context == null) return;

                FinancialInfo financialInfo = new FinancialInfo();

                // Получаем месячный доход из платежей
                string currentMonth = DateTime.Now.ToString("MM.yyyy");
                financialInfo.MonthlyIncome = context.Payment
                    .Where(p => p.Period.Contains(currentMonth))
                    .Sum(p => p.Paid_for ?? 0);

                // Расчет расходов
                financialInfo.MonthlyExpenses = CalculateMonthlyExpenses();

                // Рассчитываем чистую прибыль
                financialInfo.NetProfit = financialInfo.MonthlyIncome - financialInfo.MonthlyExpenses;

                // Рассчитываем рентабельность
                if (financialInfo.MonthlyExpenses > 0)
                {
                    financialInfo.Profitability = (financialInfo.NetProfit / financialInfo.MonthlyExpenses * 100);
                }
                else
                {
                    financialInfo.Profitability = 0;
                }

                // Определяем эффективность
                financialInfo.Efficiency = DetermineEfficiency(financialInfo.Profitability);

                // Отображаем данные
                MonthlyIncomeText.Text = $"{financialInfo.MonthlyIncome:C}";
                MonthlyExpensesText.Text = $"{financialInfo.MonthlyExpenses:C}";
                NetProfitText.Text = $"{financialInfo.NetProfit:C}";
                ProfitabilityText.Text = $"{financialInfo.Profitability:F1}%";
                EfficiencyText.Text = financialInfo.Efficiency;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки финансовой информации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private double CalculateMonthlyExpenses()
        {
            try
            {
                if (context == null) return 210000;  // Значение по умолчанию при ошибке

                double totalExpenses = 0;

                // 1. Зарплаты сотрудников (оценочно)
                // Считаем активных сотрудников и умножаем на среднюю зарплату
                int activeEmployees = context.Employees
                    .Count(e => e.Status == "Активен");
                double salaryExpenses = activeEmployees * 35000;
                totalExpenses += salaryExpenses;

                // 2. Коммунальные расходы (оценочно)
                // Каждый дом имеет примерные коммунальные расходы
                int totalHouses = context.Apartment.Count();
                double utilityExpenses = totalHouses * 5000;
                totalExpenses += utilityExpenses;

                // 3. Прочие расходы (оценочно)
                double otherExpenses = 150000;
                totalExpenses += otherExpenses;

                return totalExpenses;
            }
            catch
            {
                // В случае ошибки возвращаем фиксированное значение
                return 210000;
            }
        }

        private string DetermineEfficiency(double profitability)
        {
            // Метод для определения уровня эффективности на основе рентабельности
            // Пороговые значения можно настраивать в зависимости от бизнес-логики

            if (profitability >= 40)
                return "Высокая";
            else if (profitability >= 25)
                return "Средняя";
            else if (profitability >= 10)
                return "Низкая";
            else
                return "Критическая";
        }

        public void ShowSystemInfo()
        {
            // Метод для отображения информации о системе
            // Можно расширить, добавив данные из базы или конфигурации

            MessageBox.Show("Система управляющей компании\n\n" +
                          "Версия: 1.0.0\n" +
                          "Дата сборки: 2024\n" +
                          "Разработчик: HousingStock Team\n\n" +
                          "Используйте меню навигации для доступа к функциям.",
                          "Информация о системе",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        public void RefreshDashboard()
        {
            try
            {
                // Обновление контекста для получения свежих данных
                if (context != null)
                {
                    context.Dispose();
                }
                context = new HousingStock();

                // Перезагрузка всех данных
                LoadStatistics();
                LoadRecentApplications();
                LoadFinancialInfo();

                MessageBox.Show("Данные обновлены",
                              "Обновление",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void RecentApplicationsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                // Обработка двойного клика по элементу списка заявок
                var selectedItem = RecentApplicationsList.SelectedItem as RecentApplication;

                // Проверяем, что выбран реальный элемент (не заглушка)
                if (selectedItem != null && selectedItem.Id > 0)
                {
                    // Форматированное отображение информации о заявке
                    MessageBox.Show($"Заявка #{selectedItem.Id}\n\n" +
                                  $"Адрес: {selectedItem.Address}\n" +
                                  $"Заявитель: {selectedItem.ApplicantName}\n" +
                                  $"Телефон: {selectedItem.Phone}\n" +
                                  $"Статус: {selectedItem.Status}\n" +
                                  $"Дата создания: {selectedItem.CreateDate:dd.MM.yyyy HH:mm}\n" +
                                  $"Описание: {selectedItem.Description}",
                                  "Информация о заявке",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении заявки: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        // Метод для принудительного обновления данных
        public void ForceRefresh()
        {
            RefreshDashboard();
        }

        // Метод для безопасного освобождения ресурсов
        public void Cleanup()
        {
            context?.Dispose();
        }
    }
}