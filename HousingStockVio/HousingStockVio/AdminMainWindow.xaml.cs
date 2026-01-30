using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class AdminMainWindow : Window
    {
        public AdminMainWindow()
        {
            InitializeComponent();
            UserNameText.Text = CurrentUser.FullName;
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            var dashboardPage = new AdminDashboardPage();
            MainFrame.Navigate(dashboardPage);
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboard();
        }

        private void ApplicationsButton_Click(object sender, RoutedEventArgs e)
        {
            var applicationsPage = new ApplicationsPage();
            MainFrame.Navigate(applicationsPage);
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historyPage = new ApplicationHistoryPage();
            MainFrame.Navigate(historyPage);
        }

        private void PartnersButton_Click(object sender, RoutedEventArgs e)
        {
            var partnersPage = new PartnersPage();
            MainFrame.Navigate(partnersPage);
        }

        private void CreateAccrualButton_Click(object sender, RoutedEventArgs e)
        {
            var createAccrualWindow = new CreateAccrualWindow();
            createAccrualWindow.Owner = this;
            createAccrualWindow.ShowDialog();
        }

        private void FinancialReportsButton_Click(object sender, RoutedEventArgs e)
        {
            var financialReportsPage = new FinancialReportsPage();
            MainFrame.Navigate(financialReportsPage);
        }

        private void CreateScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateScheduleWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void StaffManagementButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new Page();
            var textBlock = new TextBlock
            {
                Text = "Управление персоналом:\n\n" +
                      "• Просмотр сотрудников\n" +
                      "• Назначение на работы\n" +
                      "• Распределение по заявкам\n" +
                      "• Учет рабочего времени",
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            page.Content = textBlock;
            MainFrame.Navigate(page);
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