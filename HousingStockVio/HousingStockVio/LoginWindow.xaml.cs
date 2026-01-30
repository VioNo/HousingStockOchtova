using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace HousingStockVio
{
    public partial class LoginWindow : Window
    {
        private HousingStock _context;

        public LoginWindow()
        {
            InitializeComponent();
            LoginTextBox.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AuthenticateUser();
        }

        private async void AuthenticateUser()
        {
            try
            {
                string login = LoginTextBox.Text.Trim();
                string password = PasswordBox.Password;

                if (string.IsNullOrEmpty(login))
                {
                    ShowError("Введите логин");
                    LoginTextBox.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    ShowError("Введите пароль");
                    PasswordBox.Focus();
                    return;
                }

                LoginButton.Content = "Проверка...";
                LoginButton.IsEnabled = false;

                // Попробуем подключиться к базе
                using (_context = new HousingStock())
                {
                    try
                    {
                        // Проверяем доступность базы
                        if (!_context.Database.Exists())
                        {
                            throw new Exception("База данных недоступна");
                        }

                        // Ищем пользователя
                        var user = await _context.Users
                            .Include(u => u.Roles)
                            .FirstOrDefaultAsync(u => u.Login == login);

                        if (user == null)
                        {
                            ShowError("Пользователь с таким логином не найден");
                            PasswordBox.Password = "";
                            PasswordBox.Focus();
                            return;
                        }

                        // Проверяем пароль
                        if (user.Password != password)
                        {
                            ShowError("Неверный пароль");
                            PasswordBox.Password = "";
                            PasswordBox.Focus();
                            return;
                        }

                        if (user.Roles == null)
                        {
                            ShowError("Роль пользователя не определена");
                            return;
                        }

                        // Инициализируем текущего пользователя через статический класс
                        CurrentUser.Initialize(user.UserID, user.FullName, user.Roles.RoleName);

                        OpenRoleSpecificWindow(user);
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Ошибка базы данных: {ex.Message}");
                        UseTestAuthentication();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
                UseTestAuthentication();
            }
            finally
            {
                LoginButton.Content = "Войти";
                LoginButton.IsEnabled = true;
            }
        }

        private void UseTestAuthentication()
        {
            try
            {
                string login = LoginTextBox.Text.Trim();
                string password = PasswordBox.Password;

                // Тестовые учетные данные для демонстрации
                var testUsers = new[]
                {
                    new { Login = "admin", Password = "admin", FullName = "Администратор Системы", Role = "administrator", RoleId = 1 },
                    new { Login = "manager", Password = "manager", FullName = "Руководитель Отдела", Role = "руководитель", RoleId = 2 },
                    new { Login = "employee", Password = "employee", FullName = "Сотрудник Иванов", Role = "employee", RoleId = 3 },
                    new { Login = "resident", Password = "resident", FullName = "Житель Петров", Role = "resident", RoleId = 4 },
                    new { Login = "test", Password = "test", FullName = "Тестовый Пользователь", Role = "administrator", RoleId = 1 }
                };

                var testUser = testUsers.FirstOrDefault(u =>
                    u.Login.Equals(login, StringComparison.OrdinalIgnoreCase) &&
                    u.Password == password);

                if (testUser != null)
                {
                    // Инициализируем текущего пользователя через статический класс
                    CurrentUser.Initialize(testUser.RoleId, testUser.FullName, testUser.Role);

                    // Создаем временного пользователя для открытия окна
                    var user = new Users
                    {
                        UserID = testUser.RoleId,
                        FullName = testUser.FullName,
                        RoleID = testUser.RoleId,
                        Login = testUser.Login,
                        Password = testUser.Password,
                        Roles = new Roles { RoleName = testUser.Role }
                    };

                    OpenRoleSpecificWindow(user);
                    this.Close();
                }
                else
                {
                    ShowError("Неверный логин или пароль");
                    PasswordBox.Password = "";
                    PasswordBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка тестовой аутентификации: {ex.Message}");
            }
        }

        private void OpenRoleSpecificWindow(Users user)
        {
            try
            {
                Window mainWindow;
                string roleName = user.Roles?.RoleName?.Trim().ToLower() ?? "";
                string fullName = user.FullName ?? "Пользователь";

                switch (roleName)
                {
                    case "administrator":
                    case "администратор":
                        mainWindow = new AdminMainWindow();
                        mainWindow.Title = $"Панель администратора - {fullName}";
                        break;

                    case "руководитель":
                    case "manager":
                    case "менеджер":
                        mainWindow = new ManagerMainWindow();
                        mainWindow.Title = $"Панель руководителя - {fullName}";
                        break;

                    case "employee":
                    case "сотрудник":
                        mainWindow = new EmployeeMainWindow();
                        mainWindow.Title = $"Панель сотрудника - {fullName}";
                        break;

                    case "resident":
                    case "житель":
                        mainWindow = new ResidentMainWindow(user);
                        mainWindow.Title = $"Панель жителя - {fullName}";
                        break;

                    default:
                        MessageBox.Show($"Роль '{roleName}' не распознана. Используется общее окно.",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        mainWindow = new MainWindow();
                        mainWindow.Title = $"Управляющая компания - {fullName}";
                        break;
                }

                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowError(string message)
        {
            try
            {
                ErrorText.Text = message;
                ErrorBorder.Visibility = Visibility.Visible;
            }
            catch
            {
                // Игнорируем ошибки отображения
            }
        }

        private void LoginTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox.Focus();
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AuthenticateUser();
            }
        }

        private void LoginTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Скрываем ошибку при изменении логина
            ErrorBorder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Скрываем ошибку при изменении пароля
            ErrorBorder.Visibility = Visibility.Collapsed;
        }
    }
}