using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class PartnersPage : Page
    {
        private HousingStock _context;
        private List<Partner> partners;

        public class Partner
        {
            public int PartnerID { get; set; }
            public string PartnerName { get; set; }
            public string ContactPerson { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Services { get; set; }
            public DateTime? ContractDate { get; set; }
            public string ContractNumber { get; set; }
            public string Status { get; set; }
        }

        public PartnersPage()
        {
            InitializeComponent();
            Loaded += PartnersPage_Loaded;
            _context = new HousingStock();
        }

        private void PartnersPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPartners();
        }

        private void LoadPartners()
        {
            try
            {
                // Проверяем, доступна ли база данных и есть ли таблица Partners
                if (!_context.Database.Exists() || !_context.Partners.Any())
                {
                    // Используем тестовые данные, если БД недоступна или таблица пуста
                    CreateMockPartners();
                    MessageBox.Show(
                        "База данных не доступна или таблица Partners пуста.\n" +
                        "Используются тестовые данные для демонстрации.",
                        "Внимание",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    // Загружаем данные из базы
                    LoadPartnersFromDatabase();
                }

                UpdatePartnersDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке партнеров: {ex.Message}\n" +
                    "Будут использованы тестовые данные.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                CreateMockPartners();
                UpdatePartnersDisplay();
            }
        }

        private void LoadPartnersFromDatabase()
        {
            try
            {
                // Загружаем партнеров из базы данных через Entity Framework
                partners = _context.Partners
                    .OrderBy(p => p.PartnerName)
                    .Select(p => new Partner
                    {
                        PartnerID = p.PartnerID,
                        PartnerName = p.PartnerName,
                        ContactPerson = p.ContactPerson,
                        Phone = p.Phone,
                        Email = p.Email,
                        Services = p.Services,
                        ContractDate = p.ContractDate,
                        ContractNumber = p.ContractNumber,
                        Status = p.Status
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки данных: {ex.Message}", ex);
            }
        }

        private void CreateMockPartners()
        {
            partners = new List<Partner>
            {
                new Partner
                {
                    PartnerID = 1,
                    PartnerName = "ООО 'СтройМатериалы'",
                    ContactPerson = "Иванов А.В.",
                    Phone = "+7 (999) 111-22-33",
                    Email = "info@stroymat.ru",
                    Services = "Поставка строительных материалов",
                    ContractDate = new DateTime(2023, 1, 15),
                    ContractNumber = "СМ-2023-001",
                    Status = "Активен"
                },
                new Partner
                {
                    PartnerID = 2,
                    PartnerName = "ЗАО 'ЭнергоСервис'",
                    ContactPerson = "Петрова М.С.",
                    Phone = "+7 (999) 222-33-44",
                    Email = "service@energo.ru",
                    Services = "Обслуживание электрооборудования",
                    ContractDate = new DateTime(2023, 2, 20),
                    ContractNumber = "ЭС-2023-005",
                    Status = "Активен"
                },
                new Partner
                {
                    PartnerID = 3,
                    PartnerName = "ИП Сидоров В.П.",
                    ContactPerson = "Сидоров В.П.",
                    Phone = "+7 (999) 333-44-55",
                    Email = "sidorov@mail.ru",
                    Services = "Сантехнические работы",
                    ContractDate = new DateTime(2023, 3, 10),
                    ContractNumber = "СТ-2023-012",
                    Status = "Неактивен"
                }
            };
        }

        private void UpdatePartnersDisplay()
        {
            try
            {
                ApplyCurrentFilter();
                UpdateStatusDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении отображения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyCurrentFilter()
        {
            try
            {
                if (PartnersList == null || partners == null)
                {
                    return;
                }

                string selectedStatus = GetSelectedFilterStatus();

                if (string.IsNullOrEmpty(selectedStatus) || selectedStatus == "Все")
                {
                    PartnersList.ItemsSource = partners;
                }
                else
                {
                    var filteredPartners = partners
                        .Where(p => p.Status == selectedStatus)
                        .ToList();

                    PartnersList.ItemsSource = filteredPartners;
                }
            }
            catch (Exception ex)
            {
                if (PartnersList != null && partners != null)
                {
                    PartnersList.ItemsSource = partners;
                }

                throw new Exception($"Ошибка применения фильтра: {ex.Message}", ex);
            }
        }

        private string GetSelectedFilterStatus()
        {
            try
            {
                if (StatusFilter == null || StatusFilter.SelectedItem == null)
                {
                    return "Все";
                }

                var selectedItem = StatusFilter.SelectedItem;

                if (selectedItem is ComboBoxItem comboBoxItem)
                {
                    return comboBoxItem.Content?.ToString() ?? "Все";
                }

                return "Все";
            }
            catch
            {
                return "Все";
            }
        }

        private void UpdateStatusDisplay()
        {
            try
            {
                if (StatusText == null || partners == null)
                {
                    return;
                }

                var displayedItems = PartnersList?.ItemsSource as IEnumerable<Partner>;
                int displayedCount = displayedItems?.Count() ?? partners.Count;
                int totalCount = partners.Count;

                if (displayedCount == totalCount)
                {
                    StatusText.Text = $"Всего партнеров: {totalCount}";
                }
                else
                {
                    StatusText.Text = $"Показано: {displayedCount} из {totalCount} партнеров";
                }
            }
            catch
            {
                if (StatusText != null)
                {
                    StatusText.Text = "Ошибка обновления статуса";
                }
            }
        }

        // Обработчики событий кнопок
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EditPartnerWindow addWindow = new EditPartnerWindow();
                addWindow.Owner = Window.GetWindow(this);
                addWindow.Title = "Добавление нового партнера";

                bool? result = addWindow.ShowDialog();

                if (result == true)
                {
                    // Обновляем контекст и загружаем данные
                    _context?.Dispose();
                    _context = new HousingStock();
                    LoadPartners();

                    MessageBox.Show(
                        "Новый партнер успешно добавлен!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при добавлении партнера: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedPartner();
        }

        private void PartnersList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditSelectedPartner();
        }

        private void EditSelectedPartner()
        {
            try
            {
                Partner selectedPartner = PartnersList.SelectedItem as Partner;

                if (selectedPartner == null)
                {
                    MessageBox.Show(
                        "Пожалуйста, выберите партнера для редактирования.",
                        "Внимание",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                EditPartnerWindow editWindow = new EditPartnerWindow(selectedPartner);
                editWindow.Owner = Window.GetWindow(this);
                editWindow.Title = $"Редактирование партнера: {selectedPartner.PartnerName}";

                bool? result = editWindow.ShowDialog();

                if (result == true)
                {
                    // Обновляем контекст и загружаем данные
                    _context?.Dispose();
                    _context = new HousingStock();
                    LoadPartners();

                    MessageBox.Show(
                        $"Партнер '{selectedPartner.PartnerName}' успешно обновлен!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при редактировании партнера: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Partner selectedPartner = PartnersList.SelectedItem as Partner;

                if (selectedPartner == null)
                {
                    MessageBox.Show(
                        "Пожалуйста, выберите партнера для удаления.",
                        "Внимание",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                MessageBoxResult confirmation = MessageBox.Show(
                    $"Вы действительно хотите удалить партнера: {selectedPartner.PartnerName}?\n\n" +
                    $"Контактное лицо: {selectedPartner.ContactPerson}\n" +
                    $"Статус: {selectedPartner.Status}\n\n" +
                    "Это действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);

                if (confirmation == MessageBoxResult.Yes)
                {
                    // Удаляем из базы данных
                    DeletePartnerFromDatabase(selectedPartner.PartnerID);

                    // Удаляем из локального списка
                    partners.Remove(selectedPartner);

                    // Обновляем отображение
                    UpdatePartnersDisplay();

                    MessageBox.Show(
                        $"Партнер '{selectedPartner.PartnerName}' успешно удален!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при удалении партнера: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DeletePartnerFromDatabase(int partnerId)
        {
            try
            {
                var partner = _context.Partners.Find(partnerId);
                if (partner != null)
                {
                    _context.Partners.Remove(partner);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при удалении партнера: {ex.Message}", ex);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Обновляем контекст для получения свежих данных
                _context?.Dispose();
                _context = new HousingStock();

                LoadPartners();

                MessageBox.Show(
                    "Список партнеров успешно обновлен!",
                    "Обновление",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при обновлении списка партнеров: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UpdatePartnersDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при изменении фильтра: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPartner = PartnersList.SelectedItem as Partner;
            if (selectedPartner != null)
            {
                MessageBox.Show(
                    $"Детали партнера:\n\n" +
                    $"Название: {selectedPartner.PartnerName}\n" +
                    $"Контактное лицо: {selectedPartner.ContactPerson}\n" +
                    $"Телефон: {selectedPartner.Phone}\n" +
                    $"Email: {selectedPartner.Email}\n" +
                    $"Услуги: {selectedPartner.Services}\n" +
                    $"Номер договора: {selectedPartner.ContractNumber}\n" +
                    $"Дата договора: {selectedPartner.ContractDate:dd.MM.yyyy}\n" +
                    $"Статус: {selectedPartner.Status}",
                    "Детали партнера",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

    }
}