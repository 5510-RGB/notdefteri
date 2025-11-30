using System;
using System.Windows;

namespace NotDefteri
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Uygulama başlatılırken bir hata oluştu:\n\n{ex}",
                    "Hata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
