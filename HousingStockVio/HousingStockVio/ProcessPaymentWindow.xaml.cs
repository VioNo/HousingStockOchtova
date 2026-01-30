using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class ProcessPaymentWindow : Window
    {
        private HousingStock _context;

        public class DebtItem
        {
            public int DebtID { get; set; }
            public string OwnerName { get; set; }
            public string Address { get; set; }
            public double WaterDebt { get; set; }
            public double ElectricityDebt { get; set; }
            public double TotalDebt { get; set; }
            public int OwnerId { get; set; }
        }

        public ProcessPaymentWindow()
        {
            InitializeComponent();
            DatePicker.SelectedDate = DateTime.Now;
            _context = new HousingStock();
            LoadDebts();
        }

        private void LoadDebts()
        {
            try
            {
                // Загружаем задолженности с информацией о владельцах и адресах
                var debts = _context.Debt
                    .Where(d => (d.Water ?? 0) + (d.Electric_power ?? 0) > 0)
                    .Join(_context.Owners,
                        debt => debt.ID_owner,
                        owner => owner.ID,
                        (debt, owner) => new { Debt = debt, Owner = owner })
                    .Join(_context.Apartment.DefaultIfEmpty(),
                        combined => combined.Owner.Adress,
                        apartment => apartment.ID,
                        (combined, apartment) => new DebtItem
                        {
                            DebtID = combined.Debt.ID,
                            OwnerId = combined.Owner.ID,
                            OwnerName = combined.Owner.Name_owner,
                            Address = apartment != null ? apartment.Adress : "Не указан",
                            WaterDebt = combined.Debt.Water ?? 0,
                            ElectricityDebt = combined.Debt.Electric_power ?? 0,
                            TotalDebt = (combined.Debt.Water ?? 0) + (combined.Debt.Electric_power ?? 0)
                        })
                    .OrderByDescending(d => d.TotalDebt)
                    .ToList();

                DebtsList.ItemsSource = debts;

                if (debts.Count > 0)
                {
                    DebtsList.SelectedIndex = 0;
                    UpdateSelectedInfo();
                }
                else
                {
                    SelectedDebtor.Text = "Нет задолженностей";
                    TotalDebt.Text = "0 руб.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задолженностей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSelectedInfo()
        {
            var item = DebtsList.SelectedItem as DebtItem;
            if (item != null)
            {
                SelectedDebtor.Text = $"{item.OwnerName}\n{item.Address}";
                TotalDebt.Text = $"{item.TotalDebt:N2} руб.";
            }
            else
            {
                SelectedDebtor.Text = "Не выбрано";
                TotalDebt.Text = "0 руб.";
            }
        }

        private void DebtsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedInfo();
        }

        private void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = DebtsList.SelectedItem as DebtItem;
                if (item == null)
                {
                    MessageBox.Show("Выберите задолженность для оплаты", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(AmountTextBox.Text, out double amount) || amount <= 0)
                {
                    MessageBox.Show("Введите корректную сумму оплаты", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    AmountTextBox.Focus();
                    return;
                }

                if (amount > item.TotalDebt)
                {
                    MessageBox.Show($"Сумма оплаты не может превышать задолженность ({item.TotalDebt:N2} руб.)",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    AmountTextBox.Focus();
                    return;
                }

                if (MethodComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите способ оплаты", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (DatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату оплаты", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Подтверждение
                string method = (MethodComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                var result = MessageBox.Show(
                    $"Подтвердите обработку платежа:\n\n" +
                    $"Владелец: {item.OwnerName}\n" +
                    $"Адрес: {item.Address}\n" +
                    $"Сумма: {amount:N2} руб.\n" +
                    $"Способ оплаты: {method}\n" +
                    $"Дата: {DatePicker.SelectedDate.Value:dd.MM.yyyy}",
                    "Подтверждение оплаты",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SavePayment(item, amount, method);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обработки платежа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SavePayment(DebtItem item, double amount, string method)
        {
            try
            {
                // Используем транзакцию для обеспечения целостности данных
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Добавляем запись о платеже
                        var payment = new Payment
                        {
                            ID_owner = item.OwnerId,
                            Period = DatePicker.SelectedDate.Value.ToString("MM.yyyy"),
                            Accrued = 0, // Для погашения задолженности начисление = 0
                            Paid_for = amount
                        };

                        _context.Payment.Add(payment);

                        // Обновляем задолженность
                        UpdateDebt(item.DebtID, amount);

                        // Сохраняем изменения
                        _context.SaveChanges();
                        transaction.Commit();

                        MessageBox.Show($"Платеж успешно обработан!\n\n" +
                                      $"Сумма: {amount:N2} руб.\n" +
                                      $"Способ: {method}\n" +
                                      $"Остаток задолженности: {(item.TotalDebt - amount):N2} руб.",
                                      "Успех",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);

                        // Очищаем поля и обновляем список
                        AmountTextBox.Clear();
                        LoadDebts();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сохранения платежа: {ex.Message}", ex);
            }
        }

        private void UpdateDebt(int debtId, double amount)
        {
            var debt = _context.Debt.Find(debtId);
            if (debt == null)
            {
                throw new Exception("Задолженность не найдена");
            }

            double remaining = amount;

            // Гасим задолженность по воде
            if (debt.Water.HasValue && debt.Water.Value > 0)
            {
                if (remaining >= debt.Water.Value)
                {
                    remaining -= debt.Water.Value;
                    debt.Water = 0;
                }
                else
                {
                    debt.Water -= remaining;
                    remaining = 0;
                }
            }

            // Гасим задолженность по электричеству
            if (remaining > 0 && debt.Electric_power.HasValue && debt.Electric_power.Value > 0)
            {
                if (remaining >= debt.Electric_power.Value)
                {
                    remaining -= debt.Electric_power.Value;
                    debt.Electric_power = 0;
                }
                else
                {
                    debt.Electric_power -= remaining;
                    remaining = 0;
                }
            }

            // Если после списания осталась задолженность - продолжаем хранение
            // Если задолженность погашена полностью - можно удалить запись или оставить с нулями
            if ((debt.Water ?? 0) + (debt.Electric_power ?? 0) == 0)
            {
                // Задолженность полностью погашена
                // Можно либо удалить запись, либо оставить с нулевыми значениями
                // _context.Debt.Remove(debt); // Удалить запись
            }
        }

        private void PayInFullButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = DebtsList.SelectedItem as DebtItem;
                if (item == null)
                {
                    MessageBox.Show("Выберите задолженность для оплаты", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MethodComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите способ оплаты", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (DatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату оплаты", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Устанавливаем сумму оплаты равной полной задолженности
                AmountTextBox.Text = item.TotalDebt.ToString("N2");

                // Автоматически обрабатываем платеж
                string method = (MethodComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                var result = MessageBox.Show(
                    $"Оплатить задолженность полностью?\n\n" +
                    $"Владелец: {item.OwnerName}\n" +
                    $"Адрес: {item.Address}\n" +
                    $"Сумма: {item.TotalDebt:N2} руб.\n" +
                    $"Способ оплаты: {method}\n" +
                    $"Дата: {DatePicker.SelectedDate.Value:dd.MM.yyyy}",
                    "Полная оплата задолженности",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SavePayment(item, item.TotalDebt, method);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при полной оплате: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _context?.Dispose();
                _context = new HousingStock();

                LoadDebts();
                MessageBox.Show("Список обновлен", "Обновление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}