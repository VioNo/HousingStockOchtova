using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadUserInfo();
            LoadDefaultPage();
        }

        private void LoadUserInfo()
        {
            if (CurrentUser.IsAuthenticated)
            {
                UserInfoText.Text = $"{CurrentUser.FullName} ({CurrentUser.RoleName})";
            }
        }

        private void LoadDefaultPage()
        {
            var defaultPage = new Page();
            var textBlock = new TextBlock
            {
                Text = $"Добро пожаловать, {CurrentUser.FullName}!\nВаша роль: {CurrentUser.RoleName}",
                FontSize = 18,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            defaultPage.Content = textBlock;
            MainFrame.Navigate(defaultPage);
        }

        private void ApplicationsButton_Click(object sender, RoutedEventArgs e)
        {
            var applicationsPage = new ApplicationsPage();
            MainFrame.Navigate(applicationsPage);
            BackButton.Visibility = Visibility.Visible;
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historyPage = new ApplicationHistoryPage();
            MainFrame.Navigate(historyPage);
            BackButton.Visibility = Visibility.Visible;
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new Page();
            var textBlock = new TextBlock
            {
                Text = "Статистика:\n\nВсего заявок: 150\nАктивных: 25\nЗавершено: 125\nСреднее время выполнения: 2.3 дня",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            page.Content = textBlock;
            MainFrame.Navigate(page);
            BackButton.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
                if (!MainFrame.CanGoBack)
                {
                    BackButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                LoadDefaultPage();
                BackButton.Visibility = Visibility.Collapsed;
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