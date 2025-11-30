using System;
using System.Windows;

namespace NotDefteri
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            try
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Uygulama başlatılırken bir hata oluştu:\n\n{ex}",
                    "Hata",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                
                // Hata durumunda uygulamayı kapat
                Current.Shutdown(1);
            }
        }
    }
}
