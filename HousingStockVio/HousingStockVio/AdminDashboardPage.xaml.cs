using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class AdminDashboardPage : Page
    {
        private HousingStock _context;

        public AdminDashboardPage()
        {
            InitializeComponent();
            Loaded += AdminDashboardPage_Loaded;
            _context = new HousingStock();
        }

        private void AdminDashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
            LoadHouses();
            LoadOwners();
            LoadPayments();
            LoadDebts();
            LoadApplications();
        }

        private void LoadStatistics()
        {
            try
            {
                // Всего домов
                var totalHouses = _context.Apartment.Count();
                TotalHousesText.Text = totalHouses.ToString();

                // Всего владельцев
                var totalOwners = _context.Owners.Count();
                TotalOwnersText.Text = totalOwners.ToString();

                // Всего начислено
                var totalAccrued = _context.Payment.Sum(p => p.Accrued ?? 0);
                TotalAccruedText.Text = $"{totalAccrued:C}";

                // Всего оплачено
                var totalPaid = _context.Payment.Sum(p => p.Paid_for ?? 0);
                TotalPaidText.Text = $"{totalPaid:C}";

                // Месячный доход (текущий месяц)
                var currentMonth = DateTime.Now.ToString("MM.yyyy");
                var monthlyIncome = _context.Payment
                    .Where(p => p.Period != null && p.Period.Contains(currentMonth))
                    .Sum(p => p.Paid_for ?? 0);
                MonthlyIncomeText.Text = $"{monthlyIncome:C}";

                // Общая задолженность
                var totalDebt = _context.Debt.Sum(d => (d.Water ?? 0) + (d.Electric_power ?? 0));
                TotalDebtText.Text = $"{totalDebt:C}";

                // Средний платеж
                var avgPayment = _context.Payment
                    .Where(p => p.Paid_for > 0)
                    .Average(p => p.Paid_for ?? 0);
                AveragePaymentText.Text = $"{avgPayment:C}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadHouses()
        {
            try
            {
                var houses = _context.Apartment
                    .Select(a => new
                    {
                        ID = a.ID,
                        Адрес = a.Adress,
                        Дата_начала = a.Beginning,
                        Этажи = a.Floors,
                        Квартиры = a.Flats,
                        Год_постройки = a.Year,
                        Площадь = a.Area
                    })
                    .ToList();

                HousesGrid.ItemsSource = houses;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки домов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOwners()
        {
            try
            {
                var owners = _context.Owners
                    .Include(o => o.Apartment) // Загружаем связанные данные о доме
                    .Select(o => new
                    {
                        ID = o.ID,
                        ФИО_владельца = o.Name_owner,
                        Квартира = o.Flat,
                        Телефон = o.Phone_number,
                        Адрес_дома = o.Apartment != null ? o.Apartment.Adress : "Не указан"
                    })
                    .ToList();

                OwnersGrid.ItemsSource = owners;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки владельцев: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPayments()
        {
            try
            {
                var payments = _context.Payment
                    .Include(p => p.Owners) // Загружаем связанные данные о владельце
                    .Select(p => new
                    {
                        ID = p.ID,
                        Владелец = p.Owners != null ? p.Owners.Name_owner : "Не указан",
                        Период = p.Period,
                        Начислено = p.Accrued,
                        Оплачено = p.Paid_for,
                        Статус = (p.Paid_for == null || p.Paid_for == 0) ? "Не оплачено" :
                                 (p.Paid_for < p.Accrued) ? "Частично оплачено" :
                                 "Оплачено полностью"
                    })
                    .ToList();

                PaymentsGrid.ItemsSource = payments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки платежей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDebts()
        {
            try
            {
                var debts = _context.Debt
                    .Include(d => d.Owners) // Загружаем связанные данные о владельце
                    .Select(d => new
                    {
                        ID = d.ID,
                        Владелец = d.Owners != null ? d.Owners.Name_owner : "Не указан",
                        Вода = d.Water,
                        Электричество = d.Electric_power,
                        Общая_задолженность = (d.Water ?? 0) + (d.Electric_power ?? 0)
                    })
                    .ToList();

                DebtsGrid.ItemsSource = debts;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки долгов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadApplications()
        {
            try
            {
                var applications = _context.Applications
                    .Select(a => new
                    {
                        ID = a.ID,
                        Адрес = a.Address,
                        Заявитель = a.ApplicantName,
                        Телефон = a.Phone,
                        Описание = a.Description,
                        Ответственный = a.Responsible,
                        Статус = a.Status,
                        Дата_создания = a.CreateDate,
                        Дата_завершения = a.CompleteDate,
                        Приоритет = a.Priority
                    })
                    .ToList();

                ApplicationsGrid.ItemsSource = applications;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Кнопки обновления для каждой вкладки
        private void RefreshHousesButton_Click(object sender, RoutedEventArgs e)
        {
            _context = new HousingStock(); // Создаем новый контекст для обновления данных
            LoadHouses();
        }

        private void RefreshOwnersButton_Click(object sender, RoutedEventArgs e)
        {
            _context = new HousingStock();
            LoadOwners();
        }

        private void RefreshPaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            _context = new HousingStock();
            LoadPayments();
        }

        private void RefreshDebtsButton_Click(object sender, RoutedEventArgs e)
        {
            _context = new HousingStock();
            LoadDebts();
        }

        private void RefreshApplicationsButton_Click(object sender, RoutedEventArgs e)
        {
            _context = new HousingStock();
            LoadApplications();
        }

        // Существующие методы
        private void CreateAccrualButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateAccrualWindow();
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
            _context = new HousingStock(); // Обновляем контекст после закрытия окна
            LoadStatistics();
        }

        private void CreateScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateScheduleWindow();
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }

        private void ViewReportButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new FinancialReportsPage();
            this.NavigationService.Navigate(page);
        }
       
    }
}