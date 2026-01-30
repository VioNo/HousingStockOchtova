using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class EditApplicationWindow : Window
    {
        private HousingStock _context;
        private ApplicationsPage.RepairApplication application;
        private bool isEditMode = false;
        private List<Employee> employees;

        public class Employee
        {
            public int EmployeeID { get; set; }
            public string FullName { get; set; }
            public string Position { get; set; }
        }

        public EditApplicationWindow()
        {
            InitializeComponent();
            _context = new HousingStock();
            InitializeNewApplication();
            LoadEmployees();
        }

        public EditApplicationWindow(ApplicationsPage.RepairApplication existingApplication)
        {
            InitializeComponent();
            _context = new HousingStock();
            isEditMode = true;
            application = existingApplication;
            LoadEmployees();
            LoadApplicationData();
        }

        private void LoadEmployees()
        {
            try
            {
                // Загружаем сотрудников из базы данных
                employees = _context.Employees
                    .Where(e => e.Status == "Активен")
                    .OrderBy(e => e.FullName)
                    .Select(e => new Employee
                    {
                        EmployeeID = e.EmployeeID,
                        FullName = e.FullName,
                        Position = e.Position
                    })
                    .ToList();

                // Очищаем ComboBox и добавляем сотрудников из базы данных
                ResponsibleBox.Items.Clear();
                foreach (var employee in employees)
                {
                    ResponsibleBox.Items.Add(new ComboBoxItem { Content = employee.FullName, Tag = employee.EmployeeID });
                }

                if (ResponsibleBox.Items.Count > 0)
                    ResponsibleBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка сотрудников: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Если не удалось загрузить сотрудников, используем статические данные
                ResponsibleBox.Items.Clear();
                ResponsibleBox.Items.Add(new ComboBoxItem { Content = "Иванов И.И." });
                ResponsibleBox.Items.Add(new ComboBoxItem { Content = "Петров П.П." });
                ResponsibleBox.Items.Add(new ComboBoxItem { Content = "Сидоров С.С." });
                ResponsibleBox.Items.Add(new ComboBoxItem { Content = "Козлов К.К." });

                if (ResponsibleBox.Items.Count > 0)
                    ResponsibleBox.SelectedIndex = 0;
            }
        }

        private void InitializeNewApplication()
        {
            HeaderText.Text = "Новая заявка на ремонт";
            DateBox.SelectedDate = DateTime.Now;
            StatusBox.SelectedIndex = 0;
        }

        private void LoadApplicationData()
        {
            try
            {
                HeaderText.Text = $"Редактирование заявки #{application.Id}";

                AddressBox.Text = application.Address;
                NameBox.Text = application.ApplicantName;
                PhoneBox.Text = application.Phone;
                DescriptionBox.Text = application.Description;
                DateBox.SelectedDate = application.CreateDate;

                // Устанавливаем ответственного
                foreach (ComboBoxItem item in ResponsibleBox.Items)
                {
                    if (item.Content.ToString() == application.Responsible)
                    {
                        ResponsibleBox.SelectedItem = item;
                        break;
                    }
                }

                // Устанавливаем статус
                foreach (ComboBoxItem item in StatusBox.Items)
                {
                    if (item.Content.ToString() == application.Status)
                    {
                        StatusBox.SelectedItem = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных заявки: {ex.Message}",
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
                        UpdateApplication();
                    }
                    else
                    {
                        CreateApplication();
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

            if (string.IsNullOrWhiteSpace(AddressBox.Text))
            {
                errorMessage += "• Введите адрес\n";
                AddressBox.Focus();
            }

            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                errorMessage += "• Введите ФИО заявителя\n";
                if (string.IsNullOrEmpty(errorMessage)) NameBox.Focus();
            }

            if (string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                errorMessage += "• Введите контактный телефон\n";
                if (string.IsNullOrEmpty(errorMessage)) PhoneBox.Focus();
            }
            else if (!IsValidPhone(PhoneBox.Text))
            {
                errorMessage += "• Введите корректный номер телефона\n";
                if (string.IsNullOrEmpty(errorMessage)) PhoneBox.Focus();
            }

            if (string.IsNullOrWhiteSpace(DescriptionBox.Text))
            {
                errorMessage += "• Введите описание проблемы\n";
                if (string.IsNullOrEmpty(errorMessage)) DescriptionBox.Focus();
            }

            if (ResponsibleBox.SelectedItem == null)
            {
                errorMessage += "• Выберите ответственного исполнителя\n";
                if (string.IsNullOrEmpty(errorMessage)) ResponsibleBox.Focus();
            }

            if (StatusBox.SelectedItem == null)
            {
                errorMessage += "• Выберите статус заявки\n";
                if (string.IsNullOrEmpty(errorMessage)) StatusBox.Focus();
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show($"Обнаружены ошибки:\n\n{errorMessage}\nПожалуйста, исправьте указанные поля.",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool IsValidPhone(string phone)
        {
            try
            {
                string digitsOnly = new string(phone.Where(c => char.IsDigit(c)).ToArray());

                if (digitsOnly.Length >= 10)
                {
                    return true;
                }

                string pattern = @"^[\d\s\(\)\+-]+$";
                return Regex.IsMatch(phone, pattern);
            }
            catch
            {
                return false;
            }
        }

        private void CreateApplication()
        {
            try
            {
                string responsible = (ResponsibleBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string status = (StatusBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                var newApplication = new Applications
                {
                    Address = AddressBox.Text.Trim(),
                    ApplicantName = NameBox.Text.Trim(),
                    Phone = PhoneBox.Text.Trim(),
                    Description = DescriptionBox.Text.Trim(),
                    Responsible = responsible,
                    AssignedEmployee = responsible,
                    Status = status,
                    CreateDate = DateBox.SelectedDate ?? DateTime.Now,
                    Priority = "Средний" // Значение по умолчанию
                };

                _context.Applications.Add(newApplication);
                _context.SaveChanges();

                MessageBox.Show("Заявка успешно создана!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания заявки: {ex.Message}", ex);
            }
        }

        private void UpdateApplication()
        {
            try
            {
                // Находим существующую заявку
                var applicationEntity = _context.Applications.Find(application.Id);

                if (applicationEntity == null)
                {
                    throw new Exception("Заявка не найдена в базе данных");
                }

                string responsible = (ResponsibleBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string status = (StatusBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string oldStatus = applicationEntity.Status;

                // Обновляем данные заявки
                applicationEntity.Address = AddressBox.Text.Trim();
                applicationEntity.ApplicantName = NameBox.Text.Trim();
                applicationEntity.Phone = PhoneBox.Text.Trim();
                applicationEntity.Description = DescriptionBox.Text.Trim();
                applicationEntity.Responsible = responsible;
                applicationEntity.AssignedEmployee = responsible;
                applicationEntity.Status = status;
                applicationEntity.CreateDate = DateBox.SelectedDate ?? DateTime.Now;

                // Обработка даты завершения
                if (status == "Завершена" && oldStatus != "Завершена")
                {
                    // Если статус изменился на "Завершена", устанавливаем дату завершения
                    applicationEntity.CompleteDate = DateTime.Now;
                }
                else if (status != "Завершена" && oldStatus == "Завершена")
                {
                    // Если статус изменился с "Завершена" на другой, очищаем дату завершения
                    applicationEntity.CompleteDate = null;
                }

                // Сохраняем изменения
                _context.SaveChanges();

                

                MessageBox.Show($"Заявка #{application.Id} успешно обновлена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления заявки: {ex.Message}", ex);
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