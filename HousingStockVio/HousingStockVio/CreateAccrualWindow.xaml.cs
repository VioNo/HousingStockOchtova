using System;
using System.Linq;
using System.Windows;

namespace HousingStockVio
{
    public partial class CreateAccrualWindow : Window
    {
        private HousingStock _context;

        public CreateAccrualWindow()
        {
            InitializeComponent();
            _context = new HousingStock();
            LoadOwners();
            PeriodTextBox.Text = DateTime.Now.ToString("MM.yyyy");
        }

        private void LoadOwners()
        {
            try
            {
                // Загружаем владельцев из базы данных
                var owners = _context.Owners
                    .OrderBy(o => o.Name_owner)
                    .Select(o => new
                    {
                        Id = o.ID,
                        Name = o.Name_owner
                    })
                    .ToList();

                OwnerComboBox.Items.Clear();
                OwnerComboBox.Items.Add(new { Id = 0, Name = "-- Выберите владельца --" });

                foreach (var owner in owners)
                {
                    OwnerComboBox.Items.Add(owner);
                }

                OwnerComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки владельцев: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OwnerComboBox.SelectedIndex == 0)
                {
                    MessageBox.Show("Выберите владельца", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(PeriodTextBox.Text))
                {
                    MessageBox.Show("Введите период", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Введите корректную сумму", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dynamic selectedOwner = OwnerComboBox.SelectedItem;
                int ownerId = (int)selectedOwner.Id;
                string period = PeriodTextBox.Text;
                string serviceType = (ServiceComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();

                // Создаем новое начисление
                var newPayment = new Payment
                {
                    ID_owner = ownerId,
                    Period = period,
                    Accrued = (double)amount,
                    Paid_for = 0 // По умолчанию не оплачено
                };

                // Добавляем начисление в контекст
                _context.Payment.Add(newPayment);

                // Обновляем задолженность
                UpdateDebt(ownerId, amount, serviceType);

                // Сохраняем все изменения
                _context.SaveChanges();

                MessageBox.Show("Начисление успешно создано", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания начисления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDebt(int ownerId, decimal amount, string serviceType)
        {
            try
            {
                // Ищем существующую задолженность для владельца
                var existingDebt = _context.Debt
                    .FirstOrDefault(d => d.ID_owner == ownerId);

                // Приводим decimal к double для совместимости с EF
                double amountDouble = (double)amount;

                if (existingDebt != null)
                {
                    // Обновляем существующую запись
                    if (serviceType == "Водоснабжение")
                    {
                        existingDebt.Water = (existingDebt.Water ?? 0) + amountDouble;
                    }
                    else if (serviceType == "Электроснабжение")
                    {
                        existingDebt.Electric_power = (existingDebt.Electric_power ?? 0) + amountDouble;
                    }
                }
                else
                {
                    // Создаем новую запись
                    var newDebt = new Debt
                    {
                        ID_owner = ownerId,
                        Water = serviceType == "Водоснабжение" ? amountDouble : 0,
                        Electric_power = serviceType == "Электроснабжение" ? amountDouble : 0
                    };

                    _context.Debt.Add(newDebt);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления задолженности: {ex.Message}", ex);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}