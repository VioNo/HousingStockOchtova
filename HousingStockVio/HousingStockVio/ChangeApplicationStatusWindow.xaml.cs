using System.Windows;

namespace HousingStockVio
{
    public partial class ChangeApplicationStatusWindow : Window
    {
        public ChangeApplicationStatusWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}