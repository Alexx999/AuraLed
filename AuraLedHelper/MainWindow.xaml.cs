using System.Windows;
using MahApps.Metro.Controls.Dialogs;

namespace AuraLedHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void AboutClick(object sender, RoutedEventArgs e)
        {
            await this.ShowMessageAsync("About", "Created by Alexander Vostres\nalex.vostres@gmail.com\ngithub.com/Alexx999/AuraLed");
        }
    }
}
