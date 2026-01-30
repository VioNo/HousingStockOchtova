using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity.Infrastructure;

namespace HousingStockVio
{
    public partial class AdminDashboardPage : Page
    {
        private HousingStock _context;

        public AdminDashboardPage()
        {
            InitializeComponent();
            Loaded += AdminDashboardPage_Loaded;
        }

        //private void AdminDashboardPage_Loaded(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        // Используем статический метод GetContext() как в других классах
        //        _context = HousingStock.GetContext();

        //        // Для Database First проверяем подключение иначе
        //        if (!CheckDatabaseConnection())
        //        {
        //            ShowErrorMessage("Невозможно подключиться к базе данных. Проверьте настройки подключения.");
        //            return;
        //        }

        //        LoadAllData();
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowErrorMessage($"Ошибка загрузки дашборда: {ex.Message}\n" +
        //                       $"Детали: {GetInnerExceptionMessage(ex)}");
        //    }
        //}

        private void AdminDashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Простой тест
                using (var testContext = HousingStock.GetContext())
                {
                    var test = testContext.Apartment.FirstOrDefault();
                    MessageBox.Show("Подключение успешно!", "Тест",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _context = HousingStock.GetContext();
                LoadAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Точная ошибка: {ex.GetType().Name}\n{ex.Message}\n\n" +
                               $"StackTrace: {ex.StackTrace}",
                               "Отладочная информация",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        private bool CheckDatabaseConnection()
        {
            try
            {
                // Для Database First проверяем, что можем выполнить простой запрос
                var test = _context.Apartment.FirstOrDefault();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetInnerExceptionMessage(Exception ex)
        {
            if (ex.InnerException != null)
            {
                return $"{ex.InnerException.Message}\n{GetInnerExceptionMessage(ex.InnerException)}";
            }
            return ex.Message;
        }

        private void LoadAllData()
        {
            try
            {
                // Сначала загружаем статистику
                LoadStatistics();

                // Затем загружаем данные для таблиц
                LoadHouses();
                LoadOwners();
                LoadPayments();
                LoadDebts();
                LoadApplications();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки данных: {ex.Message}", ex);
            }
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
                TotalAccruedText.Text = $"{totalAccrued:N2} ₽";

                // Всего оплачено
                var totalPaid = _context.Payment.Sum(p => p.Paid_for ?? 0);
                TotalPaidText.Text = $"{totalPaid:N2} ₽";

                // Месячный доход
                var currentMonth = DateTime.Now.ToString("MM.yyyy");
                var monthlyIncome = _context.Payment
                    .Where(p => p.Period != null && p.Period.Contains(currentMonth))
                    .Sum(p => p.Paid_for ?? 0);
                MonthlyIncomeText.Text = $"{monthlyIncome:N2} ₽";

                // Общая задолженность
                var totalDebt = _context.Debt.Sum(d => (d.Water ?? 0) + (d.Electric_power ?? 0));
                TotalDebtText.Text = $"{totalDebt:N2} ₽";

                // Средний платеж
                var paymentsWithPayment = _context.Payment.Where(p => p.Paid_for > 0).ToList();
                if (paymentsWithPayment.Any())
                {
                    var avgPayment = paymentsWithPayment.Average(p => p.Paid_for ?? 0);
                    AveragePaymentText.Text = $"{avgPayment:N2} ₽";
                }
                else
                {
                    AveragePaymentText.Text = "0 ₽";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка статистики: {ex.Message}", ex);
            }
        }

        private void LoadHouses()
        {
            try
            {
                var houses = _context.Apartment
                    .Take(100)
                    .Select(a => new
                    {
                        ID = a.ID,
                        Адрес = a.Adress,
                        Этажи = a.Floors,
                        Квартиры = a.Flats,
                        Площадь = a.Area
                    })
                    .ToList();

                HousesGrid.ItemsSource = houses;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки домов: {ex.Message}", ex);
            }
        }

        private void LoadOwners()
        {
            try
            {
                var owners = _context.Owners
                    .Include(o => o.Apartment)
                    .Take(100)
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
                throw new Exception($"Ошибка загрузки владельцев: {ex.Message}", ex);
            }
        }

        private void LoadPayments()
        {
            try
            {
                var payments = _context.Payment
                    .Include(p => p.Owners)
                    .Take(100)
                    .Select(p => new
                    {
                        ID = p.ID,
                        Владелец = p.Owners != null ? p.Owners.Name_owner : "Не указан",
                        Период = p.Period,
                        Начислено = p.Accrued,
                        Оплачено = p.Paid_for,
                        Статус = (p.Paid_for == null || p.Paid_for == 0) ? "Не оплачено" :
                                 (p.Paid_for < p.Accrued) ? "Частично оплачено" : "Оплачено полностью"
                    })
                    .ToList();

                PaymentsGrid.ItemsSource = payments;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки платежей: {ex.Message}", ex);
            }
        }

        private void LoadDebts()
        {
            try
            {
                var debts = _context.Debt
                    .Include(d => d.Owners)
                    .Take(100)
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
                throw new Exception($"Ошибка загрузки долгов: {ex.Message}", ex);
            }
        }

        private void LoadApplications()
        {
            try
            {
                var applications = _context.Applications
                    .Take(100)
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
                        Приоритет = a.Priority,
                        Исполнитель = a.AssignedEmployee
                    })
                    .ToList();

                ApplicationsGrid.ItemsSource = applications;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки заявок: {ex.Message}", ex);
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(
                message + "\n\n" +
                "Проверьте:\n" +
                "1. Работает ли сервер SQL Server (DESKTOP-B0P6L06)\n" +
                "2. Существует ли база данных HousingStock2\n" +
                "3. Есть ли таблицы в базе данных\n" +
                "4. Правильно ли настроен файл EDMX (Model.edmx)\n\n" +
                "Текущая строка подключения:\n" +
                "data source=DESKTOP-B0P6L06;initial catalog=HousingStock2;integrated security=True",
                "Ошибка подключения к базе данных",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void RefreshHousesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _context = HousingStock.GetContext();
                LoadHouses();
                MessageBox.Show("Данные о домах обновлены", "Обновлено",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления домов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshOwnersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _context = HousingStock.GetContext();
                LoadOwners();
                MessageBox.Show("Данные о владельцах обновлены", "Обновлено",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления владельцев: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshPaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _context = HousingStock.GetContext();
                LoadPayments();
                MessageBox.Show("Данные о платежах обновлены", "Обновлено",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления платежей: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshDebtsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _context = HousingStock.GetContext();
                LoadDebts();
                MessageBox.Show("Данные о долгах обновлены", "Обновлено",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления долгов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshApplicationsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _context = HousingStock.GetContext();
                LoadApplications();
                MessageBox.Show("Данные о заявках обновлены", "Обновлено",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления заявок: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateAccrualButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new CreateAccrualWindow();
                window.Owner = Window.GetWindow(this);
                window.ShowDialog();

                // Обновляем статистику после закрытия окна
                _context = HousingStock.GetContext();
                LoadStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания начисления: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new CreateScheduleWindow();
                window.Owner = Window.GetWindow(this);
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания графика: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var page = new FinancialReportsPage();
                this.NavigationService.Navigate(page);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перехода к отчетам: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для теста подключения
        private void TestDatabaseConnection()
        {
            try
            {
                using (var testContext = HousingStock.GetContext())
                {
                    // Пробуем выполнить простой запрос
                    var testData = testContext.Apartment.FirstOrDefault();

                    if (testData != null)
                    {
                        MessageBox.Show("Подключение к базе данных успешно!\n" +
                                      "EDMX модель работает корректно.",
                                      "Тест подключения",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Подключение успешно, но таблица Apartment пуста.",
                                      "Тест подключения",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}\n\n" +
                              "Возможно:\n" +
                              "1. Сервер SQL Server не запущен\n" +
                              "2. База данных HousingStock2 не существует\n" +
                              "3. Проблемы с правами доступа\n" +
                              "4. EDMX модель повреждена",
                              "Ошибка подключения",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
    }
}