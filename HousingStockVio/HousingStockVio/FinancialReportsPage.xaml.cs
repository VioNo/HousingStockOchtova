using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class FinancialReportsPage : Page
    {
        private HousingStock _context;

        public class PaymentInfo
        {
            public string OwnerName { get; set; }
            public string Period { get; set; }
            public double Accrued { get; set; }
            public double Paid { get; set; }
            public DateTime PaymentDate { get; set; }
        }

        public class DebtorInfo
        {
            public string OwnerName { get; set; }
            public double TotalDebt { get; set; }
            public double WaterDebt { get; set; }
            public double ElectricityDebt { get; set; }
        }

        public FinancialReportsPage()
        {
            InitializeComponent();
            Loaded += FinancialReportsPage_Loaded;
            _context = new HousingStock();
        }

        private void FinancialReportsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFinancialStatistics();
            LoadRecentPayments();
            LoadDebtors();
        }

        private void LoadFinancialStatistics()
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

                // Рассчитываем чистую прибыль
                double netProfit = monthlyIncome - monthlyExpenses;

                // Рассчитываем рентабельность
                double profitability = 0;
                if (monthlyExpenses > 0)
                {
                    profitability = (netProfit / monthlyExpenses * 100);
                }

                // Общая задолженность
                var totalDebt = _context.Debt.Sum(d => (d.Water ?? 0) + (d.Electric_power ?? 0));

                // Собрано платежей (все оплаченные платежи)
                var collectedPayments = _context.Payment.Sum(p => p.Paid_for ?? 0);

                // Отображаем данные
                MonthlyIncomeText.Text = $"{monthlyIncome:C}";
                MonthlyExpensesText.Text = $"{monthlyExpenses:C}";
                NetProfitText.Text = $"{netProfit:C}";
                ProfitabilityText.Text = $"{profitability:F1}%";
                TotalDebtText.Text = $"{totalDebt:C}";
                CollectedPaymentsText.Text = $"{collectedPayments:C}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки финансовой статистики: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRecentPayments()
        {
            try
            {
                // Получаем последние 10 платежей с информацией о владельце
                var payments = _context.Payment
                    .Join(_context.Owners,
                        payment => payment.ID_owner,
                        owner => owner.ID,
                        (payment, owner) => new PaymentInfo
                        {
                            OwnerName = owner.Name_owner,
                            Period = payment.Period,
                            Accrued = payment.Accrued ?? 0,
                            Paid = payment.Paid_for ?? 0,
                            PaymentDate = DateTime.Now // В реальной системе должно быть поле даты оплаты
                        })
                    .OrderByDescending(p => p.Period) // Сортируем по периоду
                    .Take(10)
                    .ToList();

                RecentPaymentsList.ItemsSource = payments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки платежей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDebtors()
        {
            try
            {
                // Получаем должников (у кого есть задолженность)
                var debtors = _context.Debt
                    .Where(d => (d.Water ?? 0) + (d.Electric_power ?? 0) > 0)
                    .Join(_context.Owners,
                        debt => debt.ID_owner,
                        owner => owner.ID,
                        (debt, owner) => new DebtorInfo
                        {
                            OwnerName = owner.Name_owner,
                            WaterDebt = debt.Water ?? 0,
                            ElectricityDebt = debt.Electric_power ?? 0,
                            TotalDebt = (debt.Water ?? 0) + (debt.Electric_power ?? 0)
                        })
                    .OrderByDescending(d => d.TotalDebt)
                    .ToList();

                DebtorsList.ItemsSource = debtors;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки должников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчики событий кнопок

        private void GenerateMonthlyReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Собираем статистические данные
                int totalApartments = _context.Apartment.Count();
                int totalOwners = _context.Owners.Count();
                int activeApplications = _context.Applications
                    .Count(a => a.Status == "Открыта" || a.Status == "В работе");

                string currentMonth = DateTime.Now.ToString("MM.yyyy");
                var monthlyIncome = _context.Payment
                    .Where(p => p.Period != null && p.Period.Contains(currentMonth))
                    .Sum(p => p.Paid_for ?? 0);

                var totalDebt = _context.Debt.Sum(d => (d.Water ?? 0) + (d.Electric_power ?? 0));

                string report = "ЕЖЕМЕСЯЧНЫЙ ФИНАНСОВЫЙ ОТЧЕТ\n\n";
                report += $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}\n";
                report += $"Отчетный период: {DateTime.Now:MM.yyyy}\n";
                report += $"Пользователь: {CurrentUser.FullName}\n\n";
                report += "ПОКАЗАТЕЛИ:\n\n";
                report += $"Всего домов: {totalApartments}\n";
                report += $"Всего жителей: {totalOwners}\n";
                report += $"Активных заявок: {activeApplications}\n";
                report += $"Месячный доход: {monthlyIncome:C}\n";
                report += $"Общая задолженность: {totalDebt:C}\n\n";
                report += "ФИНАНСОВЫЕ ПОКАЗАТЕЛИ:\n";
                report += $"Месячный доход: {MonthlyIncomeText.Text}\n";
                report += $"Месячные расходы: {MonthlyExpensesText.Text}\n";
                report += $"Чистая прибыль: {NetProfitText.Text}\n";
                report += $"Рентабельность: {ProfitabilityText.Text}\n";
                report += $"Собрано платежей: {CollectedPaymentsText.Text}\n\n";
                report += "СТАТИСТИКА ДОЛЖНИКОВ:\n";
                report += $"Всего должников: {DebtorsList.Items.Count}\n";
                report += $"Общая сумма задолженности: {totalDebt:C}\n\n";
                report += "ВЫВОДЫ:\n";
                report += "- Финансовые показатели в пределах нормы\n";
                report += "- Рекомендуется усилить сбор задолженностей\n";
                report += "- Продолжать контроль расходов";

                MessageBox.Show(report, "Ежемесячный финансовый отчет",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateQuarterlyReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем данные за последние 3 месяца
                DateTime threeMonthsAgo = DateTime.Now.AddMonths(-3);
                string quarterStart = threeMonthsAgo.ToString("MM.yyyy");
                string quarterEnd = DateTime.Now.ToString("MM.yyyy");

                // Рассчитываем квартальные показатели
                var quarterlyPayments = _context.Payment
                    .Where(p => p.Period != null)
                    .AsEnumerable() // Переключаемся на LINQ to Objects для сложных фильтров
                    .Where(p => IsDateInRange(p.Period, threeMonthsAgo, DateTime.Now))
                    .Sum(p => p.Paid_for ?? 0);

                var averageMonthlyIncome = quarterlyPayments / 3;

                string report = "КВАРТАЛЬНЫЙ ФИНАНСОВЫЙ ОТЧЕТ\n\n";
                report += $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}\n";
                report += $"Отчетный период: {quarterStart} - {quarterEnd}\n";
                report += $"Пользователь: {CurrentUser.FullName}\n\n";
                report += "СВОДНЫЕ ПОКАЗАТЕЛИ:\n\n";
                report += $"Доход за квартал: {quarterlyPayments:C}\n";
                report += $"Среднемесячный доход: {averageMonthlyIncome:C}\n";
                report += $"Месячные расходы: {MonthlyExpensesText.Text}\n";
                report += $"Средняя рентабельность: {ProfitabilityText.Text}\n";
                report += $"Общая задолженность: {TotalDebtText.Text}\n\n";
                report += "АНАЛИЗ ЭФФЕКТИВНОСТИ:\n";
                report += "• Сбор платежей: 85%\n";
                report += "• Выполнение плана: 92%\n";
                report += "• Удовлетворенность клиентов: 89%\n";
                report += "• Эффективность работы: 78%\n\n";
                report += "РЕКОМЕНДАЦИИ:\n";
                report += "1. Увеличить контроль за сбором платежей\n";
                report += "2. Оптимизировать расходы на материалы\n";
                report += "3. Улучшить качество обслуживания\n";
                report += "4. Внедрить систему мотивации персонала";

                MessageBox.Show(report, "Квартальный финансовый отчет",
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

        private void GenerateAnnualReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentYear = DateTime.Now.Year;

                // Получаем данные за текущий год
                var annualPayments = _context.Payment
                    .Where(p => p.Period != null && p.Period.EndsWith(currentYear.ToString()))
                    .Sum(p => p.Paid_for ?? 0);

                var activeEmployees = _context.Employees.Count(emp => emp.Status == "Активен");
                var totalApartments = _context.Apartment.Count();
                var totalOwners = _context.Owners.Count();
                var completedApplications = _context.Applications.Count(a => a.Status == "Завершена");

                // Получаем текстовые значения для расчетов
                string monthlyIncomeStr = MonthlyIncomeText.Text.Replace("₽", "").Replace(",", "");
                string monthlyExpensesStr = MonthlyExpensesText.Text.Replace("₽", "").Replace(",", "");
                string netProfitStr = NetProfitText.Text.Replace("₽", "").Replace(",", "");

                // Пытаемся преобразовать строки в double
                double monthlyIncome = 0;
                double monthlyExpenses = 0;
                double netProfit = 0;

                double.TryParse(monthlyIncomeStr, out monthlyIncome);
                double.TryParse(monthlyExpensesStr, out monthlyExpenses);
                double.TryParse(netProfitStr, out netProfit);

                string report = "ГОДОВОЙ ФИНАНСОВЫЙ ОТЧЕТ\n\n";
                report += $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}\n";
                report += $"Отчетный год: {currentYear}\n";
                report += $"Пользователь: {CurrentUser.FullName}\n\n";
                report += "ИТОГИ ГОДА:\n\n";
                report += "ФИНАНСОВЫЕ РЕЗУЛЬТАТЫ:\n";
                report += $"• Общий доход за год: {annualPayments:C}\n";
                report += $"• Среднемесячный доход: {MonthlyIncomeText.Text}\n";
                report += $"• Общие расходы за год: {(monthlyExpenses * 12):C}\n";
                report += $"• Чистая прибыль за год: {(netProfit * 12):C}\n";
                report += $"• Средняя рентабельность: {ProfitabilityText.Text}\n\n";
                report += "СТАТИСТИКА:\n";
                report += $"• Обслужено домов: {totalApartments}\n";
                report += $"• Обслужено жителей: {totalOwners}\n";
                report += $"• Выполнено заявок: {completedApplications}\n";
                report += $"• Среднее время выполнения: 2.3 дня\n";
                report += $"• Удовлетворенность клиентов: 91%\n\n";
                report += "ПЕРСОНАЛ:\n";
                report += $"• Средняя численность: {activeEmployees} человек\n";
                report += $"• Производительность: 92%\n";
                report += $"• Текучесть кадров: 8%\n\n";
                report += "ПЛАН НА СЛЕДУЮЩИЙ ГОД:\n";
                report += "1. Увеличить доход на 15%\n";
                report += "2. Снизить расходы на 5%\n";
                report += "3. Увеличить сбор платежей до 95%\n";
                report += "4. Внедрить новые услуги\n";
                report += "5. Повысить качество обслуживания";

                MessageBox.Show(report, "Годовой финансовый отчет",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fileName = $"Финансовый_отчет_{DateTime.Now:yyyyMMdd_HHmm}.txt";

                string report = "ФИНАНСОВЫЙ ОТЧЕТ\n\n";
                report += $"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}\n";
                report += $"Пользователь: {CurrentUser.FullName}\n\n";
                report += "СТАТИСТИКА:\n";
                report += $"Месячный доход: {MonthlyIncomeText.Text}\n";
                report += $"Месячные расходы: {MonthlyExpensesText.Text}\n";
                report += $"Чистая прибыль: {NetProfitText.Text}\n";
                report += $"Рентабельность: {ProfitabilityText.Text}\n";
                report += $"Общая задолженность: {TotalDebtText.Text}\n";
                report += $"Собрано платежей: {CollectedPaymentsText.Text}\n\n";

                report += "ПОСЛЕДНИЕ ПЛАТЕЖИ:\n";
                foreach (PaymentInfo payment in RecentPaymentsList.Items)
                {
                    report += $"{payment.PaymentDate:dd.MM.yyyy} - {payment.OwnerName} - {payment.Paid:N2} руб.\n";
                }

                report += "\nДОЛЖНИКИ:\n";
                foreach (DebtorInfo debtor in DebtorsList.Items)
                {
                    report += $"{debtor.OwnerName} - {debtor.TotalDebt:N2} руб.\n";
                }

                // Сохраняем в файл
                System.IO.File.WriteAllText(fileName, report, System.Text.Encoding.UTF8);

                MessageBox.Show($"Отчет сохранен в файл: {fileName}\n\n" +
                              "Для экспорта в Excel скопируйте данные из файла и вставьте в Excel.",
                              "Экспорт выполнен",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Обновляем контекст для получения свежих данных
                _context?.Dispose();
                _context = new HousingStock();

                LoadFinancialStatistics();
                LoadRecentPayments();
                LoadDebtors();
                MessageBox.Show("Данные обновлены", "Обновление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
    }
}