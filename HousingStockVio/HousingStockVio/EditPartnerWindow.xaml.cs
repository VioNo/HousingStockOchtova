using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class EditPartnerWindow : Window
    {
        private HousingStock _context;
        private PartnersPage.Partner partner;
        private bool isEditMode = false;

        public EditPartnerWindow()
        {
            InitializeComponent();
            _context = new HousingStock();
            InitializeNewPartner();
        }

        public EditPartnerWindow(PartnersPage.Partner existingPartner)
        {
            InitializeComponent();
            _context = new HousingStock();
            isEditMode = true;
            partner = existingPartner;
            LoadPartnerData();
        }

        private void InitializeNewPartner()
        {
            HeaderText.Text = "Новый партнер";
            ContractDateBox.SelectedDate = DateTime.Now;
            StatusBox.SelectedIndex = 0;
        }

        private void LoadPartnerData()
        {
            try
            {
                HeaderText.Text = $"Редактирование партнера: {partner.PartnerName}";

                NameBox.Text = partner.PartnerName;
                ContactBox.Text = partner.ContactPerson;
                PhoneBox.Text = partner.Phone;
                EmailBox.Text = partner.Email;
                ServicesBox.Text = partner.Services;
                ContractNumberBox.Text = partner.ContractNumber;

                if (partner.ContractDate.HasValue)
                {
                    ContractDateBox.SelectedDate = partner.ContractDate.Value;
                }

                // Устанавливаем статус
                foreach (ComboBoxItem item in StatusBox.Items)
                {
                    if (item.Content.ToString() == partner.Status)
                    {
                        StatusBox.SelectedItem = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных партнера: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                try
                {
                    if (isEditMode)
                    {
                        UpdatePartner();
                    }
                    else
                    {
                        CreatePartner();
                    }

                    DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateData()
        {
            string errorMessage = "";

            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                errorMessage += "• Введите название партнера\n";
                NameBox.Focus();
            }

            if (string.IsNullOrWhiteSpace(ContactBox.Text))
            {
                errorMessage += "• Введите контактное лицо\n";
                if (string.IsNullOrEmpty(errorMessage)) ContactBox.Focus();
            }

            if (StatusBox.SelectedItem == null)
            {
                errorMessage += "• Выберите статус партнера\n";
                if (string.IsNullOrEmpty(errorMessage)) StatusBox.Focus();
            }

            // Проверка формата email
            if (!string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                if (!IsValidEmail(EmailBox.Text))
                {
                    errorMessage += "• Введите корректный email адрес\n";
                    if (string.IsNullOrEmpty(errorMessage)) EmailBox.Focus();
                }
            }

            // Проверка формата телефона
            if (!string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                if (!IsValidPhone(PhoneBox.Text))
                {
                    errorMessage += "• Введите корректный номер телефона\n";
                    if (string.IsNullOrEmpty(errorMessage)) PhoneBox.Focus();
                }
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show($"Обнаружены ошибки:\n\n{errorMessage}\nПожалуйста, исправьте указанные поля.",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            try
            {
                var regex = new Regex(@"^[\d\s\(\)\+-]+$");
                return regex.IsMatch(phone);
            }
            catch
            {
                return false;
            }
        }

        private void CreatePartner()
        {
            try
            {
                var newPartner = new Partners
                {
                    PartnerName = NameBox.Text.Trim(),
                    ContactPerson = ContactBox.Text.Trim(),
                    Phone = PhoneBox.Text.Trim(),
                    Email = EmailBox.Text.Trim(),
                    Services = ServicesBox.Text.Trim(),
                    ContractNumber = ContractNumberBox.Text.Trim(),
                    ContractDate = ContractDateBox.SelectedDate,
                    Status = (StatusBox.SelectedItem as ComboBoxItem)?.Content.ToString()
                };

                _context.Partners.Add(newPartner);
                _context.SaveChanges();

                MessageBox.Show("Партнер успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания партнера: {ex.Message}", ex);
            }
        }

        private void UpdatePartner()
        {
            try
            {
                // Находим существующего партнера в базе данных
                var partnerEntity = _context.Partners.Find(partner.PartnerID);

                if (partnerEntity == null)
                {
                    throw new Exception("Партнер не найден в базе данных");
                }

                // Обновляем данные партнера
                partnerEntity.PartnerName = NameBox.Text.Trim();
                partnerEntity.ContactPerson = ContactBox.Text.Trim();
                partnerEntity.Phone = PhoneBox.Text.Trim();
                partnerEntity.Email = EmailBox.Text.Trim();
                partnerEntity.Services = ServicesBox.Text.Trim();
                partnerEntity.ContractNumber = ContractNumberBox.Text.Trim();
                partnerEntity.ContractDate = ContractDateBox.SelectedDate;
                partnerEntity.Status = (StatusBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                // Сохраняем изменения
                _context.SaveChanges();

                MessageBox.Show($"Партнер '{partnerEntity.PartnerName}' успешно обновлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления партнера: {ex.Message}", ex);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}