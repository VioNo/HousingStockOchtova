using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Data;
using System.Text.RegularExpressions;

namespace HousingStockVio
{
    public partial class ResidentMainWindow : Window
    {
        public Users _currentUser;
        public ResidentMainWindow(Users user)
        {
            InitializeComponent();
            _currentUser = user;
            UserNameText.Text = _currentUser.FullName;
            LoadResidentApplications();
        }

        private void LoadResidentApplications()
        {
            try
            {
                using (var context = new HousingStock())
                {
                    var residentApplications = context.Applications
                        .Where(a => a.AssignedTo == _currentUser.UserID)
                        .OrderByDescending(a => a.CreateDate)
                        .ToList();

                    // Создаем страницу для отображения заявок
                    var page = new Page();
                    var stackPanel = new StackPanel
                    {
                        Margin = new Thickness(20)
                    };

                    var header = new TextBlock
                    {
                        Text = "Мои заявки",
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 20)
                    };
                    stackPanel.Children.Add(header);

                    if (residentApplications.Any())
                    {
                        // Создаем DataGrid для отображения заявок
                        var dataGrid = new DataGrid
                        {
                            AutoGenerateColumns = false,
                            IsReadOnly = true,
                            Margin = new Thickness(0, 0, 0, 20),
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                        };

                        // Настраиваем колонки
                        dataGrid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "ID",
                            Binding = new Binding("ID"),
                            Width = 50
                        });

                        dataGrid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Дата создания",
                            Binding = new Binding("CreateDate") { StringFormat = "dd.MM.yyyy HH:mm" },
                            Width = 120
                        });

                        dataGrid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Адрес",
                            Binding = new Binding("Address"),
                            Width = 150
                        });

                        dataGrid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Описание",
                            Binding = new Binding("Description"),
                            Width = 250
                        });

                        dataGrid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Статус",
                            Binding = new Binding("Status"),
                            Width = 100
                        });

                        dataGrid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Приоритет",
                            Binding = new Binding("Priority"),
                            Width = 100
                        });

                        dataGrid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Ответственный",
                            Binding = new Binding("Responsible"),
                            Width = 150
                        });

                        dataGrid.ItemsSource = residentApplications;
                        stackPanel.Children.Add(dataGrid);

                        var countText = new TextBlock
                        {
                            Text = $"Всего заявок: {residentApplications.Count}",
                            FontSize = 14,
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 10, 0, 0)
                        };
                        stackPanel.Children.Add(countText);
                    }
                    else
                    {
                        var noApplicationsText = new TextBlock
                        {
                            Text = "У вас пока нет созданных заявок.\n\nНажмите кнопку 'Создать заявку' для создания новой заявки.",
                            FontSize = 16,
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 50, 0, 0),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        stackPanel.Children.Add(noApplicationsText);
                    }

                    page.Content = stackPanel;
                    MainFrame.Navigate(page);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем и показываем простое окно для создания заявки
            CreateApplicationWindow();
        }

        // Метод для проверки номера телефона
        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // Телефон не обязателен

            // Убираем все пробелы, скобки и дефисы
            string cleanPhone = Regex.Replace(phone, @"[^\d\+]", "");

            // Проверяем российские форматы телефонов
            // +7XXXXXXXXXX, 8XXXXXXXXXX, 7XXXXXXXXXX
            if (cleanPhone.StartsWith("+7") && cleanPhone.Length == 12)
                return true;

            if (cleanPhone.StartsWith("8") && cleanPhone.Length == 11)
                return true;

            if (cleanPhone.StartsWith("7") && cleanPhone.Length == 11)
                return true;

            // Проверяем городские номера (без кода страны)
            if (cleanPhone.Length >= 6 && cleanPhone.Length <= 10)
                return true;

            return false;
        }

        // Метод для форматирования телефона
        private string FormatPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;

            // Убираем все нецифровые символы кроме плюса
            string cleanPhone = Regex.Replace(phone, @"[^\d\+]", "");

            if (cleanPhone.StartsWith("+7") && cleanPhone.Length == 12)
            {
                return $"+7 ({cleanPhone.Substring(2, 3)}) {cleanPhone.Substring(5, 3)}-{cleanPhone.Substring(8, 2)}-{cleanPhone.Substring(10, 2)}";
            }
            else if (cleanPhone.StartsWith("8") && cleanPhone.Length == 11)
            {
                return $"+7 ({cleanPhone.Substring(1, 3)}) {cleanPhone.Substring(4, 3)}-{cleanPhone.Substring(7, 2)}-{cleanPhone.Substring(9, 2)}";
            }
            else if (cleanPhone.StartsWith("7") && cleanPhone.Length == 11)
            {
                return $"+7 ({cleanPhone.Substring(1, 3)}) {cleanPhone.Substring(4, 3)}-{cleanPhone.Substring(7, 2)}-{cleanPhone.Substring(9, 2)}";
            }

            return phone; // Возвращаем как есть, если формат не распознан
        }

        private void CreateApplicationWindow()
        {
            // Создаем окно
            var window = new Window
            {
                Title = "Создание новой заявки",
                Width = 500,
                Height = 550,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            var mainGrid = new Grid();
            window.Content = mainGrid;

            // Создаем ScrollViewer для прокрутки
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            mainGrid.Children.Add(scrollViewer);

            var contentGrid = new Grid { Margin = new Thickness(20) };
            scrollViewer.Content = contentGrid;

            // Определяем строки
            for (int i = 0; i < 11; i++)
            {
                contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Заголовок
            var titleLabel = new TextBlock
            {
                Text = "Новая заявка",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(titleLabel, 0);
            contentGrid.Children.Add(titleLabel);

            // Адрес
            var addressLabel = new TextBlock
            {
                Text = "Адрес *:",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetRow(addressLabel, 1);
            var addressTextBox = new TextBox
            {
                Height = 30,
                Margin = new Thickness(0, 0, 0, 15),
                ToolTip = "Введите полный адрес (улица, дом, квартира)",
                MaxLength = 200
            };
            Grid.SetRow(addressTextBox, 2);
            contentGrid.Children.Add(addressLabel);
            contentGrid.Children.Add(addressTextBox);

            // Телефон
            var phoneLabel = new TextBlock
            {
                Text = "Контактный телефон:",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetRow(phoneLabel, 3);

            var phoneStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 15)
            };

            var phonePrefixText = new TextBlock
            {
                Text = "+7",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            };

            var phoneTextBox = new TextBox
            {
                Height = 30,
                Width = 150,
                ToolTip = "Введите номер телефона в формате: (XXX) XXX-XX-XX\nПример: (912) 345-67-89",
                MaxLength = 20
            };

            phoneStackPanel.Children.Add(phonePrefixText);
            phoneStackPanel.Children.Add(phoneTextBox);

            Grid.SetRow(phoneStackPanel, 4);
            contentGrid.Children.Add(phoneLabel);
            contentGrid.Children.Add(phoneStackPanel);

            // Пример формата телефона
            var phoneExampleText = new TextBlock
            {
                Text = "Пример: (912) 345-67-89",
                FontSize = 11,
                FontStyle = FontStyles.Italic,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, -10, 0, 10)
            };
            Grid.SetRow(phoneExampleText, 5);
            contentGrid.Children.Add(phoneExampleText);

            // Приоритет
            var priorityLabel = new TextBlock
            {
                Text = "Приоритет:",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetRow(priorityLabel, 6);
            var priorityComboBox = new ComboBox
            {
                Height = 30,
                Margin = new Thickness(0, 0, 0, 15),
                ToolTip = "Выберите приоритет заявки"
            };
            priorityComboBox.Items.Add("Низкий");
            priorityComboBox.Items.Add("Средний");
            priorityComboBox.Items.Add("Высокий");
            priorityComboBox.Items.Add("Критический");
            priorityComboBox.SelectedIndex = 0;
            Grid.SetRow(priorityComboBox, 7);
            contentGrid.Children.Add(priorityLabel);
            contentGrid.Children.Add(priorityComboBox);

            // Описание
            var descriptionLabel = new TextBlock
            {
                Text = "Описание проблемы *:",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetRow(descriptionLabel, 8);
            var descriptionTextBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 15),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Height = 120,
                ToolTip = "Подробно опишите проблему\nМаксимум 1000 символов",
                MaxLength = 1000
            };
            Grid.SetRow(descriptionTextBox, 9);
            contentGrid.Children.Add(descriptionLabel);
            contentGrid.Children.Add(descriptionTextBox);

            // Примечание
            var noteText = new TextBlock
            {
                Text = "* - обязательные поля",
                FontSize = 12,
                FontStyle = FontStyles.Italic,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 15)
            };
            Grid.SetRow(noteText, 10);
            contentGrid.Children.Add(noteText);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            Grid.SetRow(buttonPanel, 11);
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var cancelButton = new Button
            {
                Content = "Отмена",
                Width = 120,
                Height = 35,
                Margin = new Thickness(0, 0, 20, 0)
            };

            var saveButton = new Button
            {
                Content = "Создать заявку",
                Width = 120,
                Height = 35,
                Background = System.Windows.Media.Brushes.LightGreen,
                FontWeight = FontWeights.Bold
            };

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(saveButton);
            contentGrid.Children.Add(buttonPanel);

            // Устанавливаем фокус на первое поле
            addressTextBox.Focus();

            // Обработчики событий
            saveButton.Click += (s, args) =>
            {
                // Сбрасываем ошибки
                addressTextBox.BorderBrush = System.Windows.Media.Brushes.Gray;
                phoneTextBox.BorderBrush = System.Windows.Media.Brushes.Gray;
                descriptionTextBox.BorderBrush = System.Windows.Media.Brushes.Gray;

                bool hasErrors = false;
                var errorMessages = new List<string>();

                // Проверяем обязательные поля
                if (string.IsNullOrWhiteSpace(addressTextBox.Text))
                {
                    addressTextBox.BorderBrush = System.Windows.Media.Brushes.Red;
                    errorMessages.Add("• Введите адрес");
                    hasErrors = true;
                }
                else if (addressTextBox.Text.Trim().Length < 5)
                {
                    addressTextBox.BorderBrush = System.Windows.Media.Brushes.Orange;
                    errorMessages.Add("• Адрес слишком короткий (минимум 5 символов)");
                    hasErrors = true;
                }
                else if (addressTextBox.Text.Trim().Length > 200)
                {
                    addressTextBox.BorderBrush = System.Windows.Media.Brushes.Orange;
                    errorMessages.Add("• Адрес слишком длинный (максимум 200 символов)");
                    hasErrors = true;
                }

                // Проверяем телефон
                if (!string.IsNullOrWhiteSpace(phoneTextBox.Text))
                {
                    if (!IsValidPhoneNumber(phoneTextBox.Text))
                    {
                        phoneTextBox.BorderBrush = System.Windows.Media.Brushes.Red;
                        errorMessages.Add("• Неверный формат телефона\n  Используйте формат: (XXX) XXX-XX-XX или +7XXXXXXXXXX");
                        hasErrors = true;
                    }
                }

                // Проверяем описание
                if (string.IsNullOrWhiteSpace(descriptionTextBox.Text))
                {
                    descriptionTextBox.BorderBrush = System.Windows.Media.Brushes.Red;
                    errorMessages.Add("• Введите описание проблемы");
                    hasErrors = true;
                }
                else if (descriptionTextBox.Text.Trim().Length < 10)
                {
                    descriptionTextBox.BorderBrush = System.Windows.Media.Brushes.Orange;
                    errorMessages.Add("• Описание слишком короткое (минимум 10 символов)");
                    hasErrors = true;
                }
                else if (descriptionTextBox.Text.Trim().Length > 1000)
                {
                    descriptionTextBox.BorderBrush = System.Windows.Media.Brushes.Orange;
                    errorMessages.Add("• Описание слишком длинное (максимум 1000 символов)");
                    hasErrors = true;
                }

                // Если есть ошибки, показываем их
                if (hasErrors)
                {
                    string errorMessage = "Пожалуйста, исправьте следующие ошибки:\n\n" +
                                         string.Join("\n\n", errorMessages);

                    MessageBox.Show(errorMessage, "Ошибки в данных",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // Форматируем телефон, если он введен
                    string formattedPhone = string.Empty;
                    if (!string.IsNullOrWhiteSpace(phoneTextBox.Text))
                    {
                        formattedPhone = FormatPhoneNumber(phoneTextBox.Text);
                    }

                    // Сохраняем заявку в базу данных
                    using (var context = new HousingStock())
                    {
                        var application = new Applications
                        {
                            Address = addressTextBox.Text.Trim(),
                            Description = descriptionTextBox.Text.Trim(),
                            ApplicantName = _currentUser.FullName,
                            Phone = formattedPhone,
                            Status = "Новая",
                            CreateDate = DateTime.Now,
                            Priority = priorityComboBox.SelectedItem?.ToString() ?? "Низкий",
                            AssignedTo = _currentUser.UserID, // Привязываем к текущему пользователю
                            AssignedEmployee = "Не назначен",
                            Responsible = "Не назначен",
                            CategoryID = null,
                            CompleteDate = null
                        };

                        context.Applications.Add(application);
                        context.SaveChanges();

                        MessageBox.Show($"✅ Заявка успешно создана!\n\n" +
                                       $"Номер заявки: {application.ID}\n" +
                                       $"Статус: {application.Status}\n" +
                                       $"Дата создания: {application.CreateDate:dd.MM.yyyy HH:mm}\n\n" +
                                       $"Вы можете отслеживать статус заявки в разделе 'Мои заявки'.",
                                       "Заявка создана",
                                       MessageBoxButton.OK,
                                       MessageBoxImage.Information);

                        window.Close();

                        // Обновляем список заявок
                        LoadResidentApplications();
                    }
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    var validationErrors = new List<string>();
                    foreach (var validationErrorsItem in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrorsItem.ValidationErrors)
                        {
                            validationErrors.Add($"Поле '{validationError.PropertyName}': {validationError.ErrorMessage}");
                        }
                    }

                    string fullErrorMessage = "Ошибки валидации данных:\n\n" +
                                             string.Join("\n", validationErrors);

                    MessageBox.Show(fullErrorMessage, "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException dbUpEx)
                {
                    string errorMessage = "Ошибка при сохранении заявки в базу данных.\n\n";

                    if (dbUpEx.InnerException != null)
                    {
                        errorMessage += dbUpEx.InnerException.Message;

                        // Проверяем нарушение ограничений БД
                        if (dbUpEx.InnerException.Message.Contains("FK_") ||
                            dbUpEx.InnerException.Message.Contains("foreign key"))
                        {
                            errorMessage += "\n\nВозможно, нарушены связи между таблицами.";
                        }
                    }

                    MessageBox.Show(errorMessage, "Ошибка базы данных",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    string detailedError = $"Ошибка: {ex.Message}";

                    if (ex.InnerException != null)
                    {
                        detailedError += $"\n\nДетали: {ex.InnerException.Message}";
                    }

                    MessageBox.Show(detailedError, "Ошибка при создании заявки",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            cancelButton.Click += (s, args) =>
            {
                var result = MessageBox.Show("Вы действительно хотите отменить создание заявки?\nВсе введенные данные будут потеряны.",
                    "Подтверждение отмены",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    window.Close();
                }
            };

            // Обработчик нажатия Enter в полях
            addressTextBox.KeyDown += (s, args) =>
            {
                if (args.Key == System.Windows.Input.Key.Enter)
                {
                    phoneTextBox.Focus();
                    args.Handled = true;
                }
            };

            phoneTextBox.KeyDown += (s, args) =>
            {
                if (args.Key == System.Windows.Input.Key.Enter)
                {
                    descriptionTextBox.Focus();
                    args.Handled = true;
                }
            };

            // Автоматическое форматирование телефона при потере фокуса
            phoneTextBox.LostFocus += (s, args) =>
            {
                if (!string.IsNullOrWhiteSpace(phoneTextBox.Text) && IsValidPhoneNumber(phoneTextBox.Text))
                {
                    string formatted = FormatPhoneNumber(phoneTextBox.Text);
                    if (formatted != phoneTextBox.Text)
                    {
                        phoneTextBox.Text = formatted;
                    }
                }
            };

            // Показываем окно
            window.Show();
        }

        private void MyApplicationsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadResidentApplications();
        }

        private void MyPaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var page = new Page();
                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(20)
                };

                var header = new TextBlock
                {
                    Text = "Мои платежи и задолженности",
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 20)
                };

                stackPanel.Children.Add(header);

                using (var context = new HousingStock())
                {
                    var owner = context.Owners
                        .FirstOrDefault(o => o.Name_owner.Contains(_currentUser.FullName));

                    if (owner != null)
                    {
                        var debt = context.Debt
                            .Where(d => d.ID_owner == owner.ID)
                            .Select(d => (decimal?)(d.Water + d.Electric_power))
                            .Sum();

                        decimal totalDebt = debt ?? 0;

                        var debtText = new TextBlock
                        {
                            Text = totalDebt > 0 ?
                                $"Текущая задолженность: {totalDebt:C}\nРекомендуется оплатить в ближайшее время." :
                                "Задолженность отсутствует. Все платежи оплачены вовремя.",
                            FontSize = 16,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 0, 0, 10)
                        };
                        stackPanel.Children.Add(debtText);
                    }
                    else
                    {
                        var infoText = new TextBlock
                        {
                            Text = "Информация о платежах не найдена.\nЕсли у вас есть вопросы по оплате, обратитесь в управляющую компанию.",
                            FontSize = 16,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 0, 0, 10)
                        };
                        stackPanel.Children.Add(infoText);
                    }
                }

                var paymentsExample = new TextBlock
                {
                    Text = "\nПримерные ежемесячные платежи:\n" +
                           "• Коммунальные услуги: 2,500 руб.\n" +
                           "• Содержание дома: 1,500 руб.\n" +
                           "• Капитальный ремонт: 500 руб.\n" +
                           "• Вывоз ТБО: 200 руб.\n\n" +
                           "Итого: ~4,700 руб./мес.",
                    FontSize = 14,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                stackPanel.Children.Add(paymentsExample);

                page.Content = stackPanel;
                MainFrame.Navigate(page);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки платежей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var page = new Page();
                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(20)
                };

                var header = new TextBlock
                {
                    Text = $"Уведомления для {_currentUser.FullName}",
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 20)
                };

                stackPanel.Children.Add(header);

                using (var context = new HousingStock())
                {
                    var userApplications = context.Applications
                        .Where(a => a.AssignedTo == _currentUser.UserID)
                        .OrderByDescending(a => a.CreateDate)
                        .Take(5)
                        .ToList();

                    if (userApplications.Any())
                    {
                        var notificationsText = new TextBlock
                        {
                            FontSize = 14,
                            TextWrapping = TextWrapping.Wrap
                        };

                        string notifications = "";
                        foreach (var app in userApplications)
                        {
                            string statusInfo = "";
                            if (app.Status == "Выполнено" && app.CompleteDate.HasValue)
                            {
                                statusInfo = $" (Завершено: {app.CompleteDate.Value:dd.MM.yyyy})";
                            }
                            notifications += $"• Заявка #{app.ID} ({app.CreateDate:dd.MM.yyyy}): {app.Status}{statusInfo}\n";
                        }

                        notifications += "\nДля уточнения деталей обращайтесь в управляющую компанию.";
                        notificationsText.Text = notifications;
                        stackPanel.Children.Add(notificationsText);
                    }
                    else
                    {
                        var noNotificationsText = new TextBlock
                        {
                            Text = "Новых уведомлений нет.",
                            FontSize = 16,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 10, 0, 10)
                        };
                        stackPanel.Children.Add(noNotificationsText);
                    }
                }

                page.Content = stackPanel;
                MainFrame.Navigate(page);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки уведомлений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы действительно хотите выйти из системы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CurrentUser.Clear();
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}