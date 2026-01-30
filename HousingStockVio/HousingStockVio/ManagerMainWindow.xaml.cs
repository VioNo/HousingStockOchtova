using System.Windows;
using System.Windows.Controls;

namespace HousingStockVio
{
    public partial class ManagerMainWindow : Window
    {
        public ManagerMainWindow()
        {
            InitializeComponent();
            UserNameText.Text = CurrentUser.FullName;
            LoadDashboardPage(); 
        }

        private void LoadDashboardPage()
        {
            var dashboardPage = new ManagerDashboardPage();
            MainFrame.Navigate(dashboardPage);
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboardPage();
        }

        private void FinanceButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSimplePage("Финансовое состояние",
                "Прибыль: 1,200,000 руб.\nРасходы: 850,000 руб.\nЧистая прибыль: 350,000 руб.\nРентабельность: 41.2%");
        }

        private void IncomeExpensesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSimplePage("Доходы и расходы",
                "Доходы:\n- Платежи жильцов: 900,000 руб.\n- Дополнительные услуги: 300,000 руб.\n\n" +
                "Расходы:\n- Зарплаты: 500,000 руб.\n- Коммунальные услуги: 200,000 руб.\n- Ремонты: 150,000 руб.");
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

        private void StaffManagementButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSimplePage("Управление персоналом",
                "Всего сотрудников: 15\nАктивных: 12\nВ отпуске: 3\n\n" +
                "Средняя загруженность: 78%\nВыполнение плана: 92%");
        }

        private void EfficiencyButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSimplePage("Эффективность компании",
                "Прибыль от использования ресурсов: 1,200,000 руб.\n" +
                "Суммарные затраты на ресурсы: 850,000 руб.\n" +
                "Рентабельность использования ресурсов: 41.2%\n\n" +
                "Эффективность: Высокая");
        }

        private void ShowSimplePage(string title, string content)
        {
            var page = new Page();
            var stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };

            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var contentText = new TextBlock
            {
                Text = content,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap
            };

            stackPanel.Children.Add(titleText);
            stackPanel.Children.Add(contentText);
            page.Content = stackPanel;

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