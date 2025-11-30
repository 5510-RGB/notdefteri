using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace NotDefteri
{
    public static class CustomCommands
    {
        public static readonly RoutedUICommand Zoom = new RoutedUICommand(
            "Zoom", 
            "Zoom", 
            typeof(CustomCommands),
            new InputGestureCollection
            {
                new KeyGesture(Key.Add, ModifierKeys.Control),
                new KeyGesture(Key.OemPlus, ModifierKeys.Control)
            });
    }

    public partial class MainWindow : Window
    {
        private bool isContentModified = false;
        private string currentFilePath = string.Empty;
        private bool isUpdatingUI = false;

        public MainWindow()
        {
            InitializeComponent();
            txtEditor.TextChanged += TxtEditor_TextChanged;
            this.Closing += MainWindow_Closing;
            
            // Klavye kısayolları
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            
            // Başlangıçta düğmeleri güncelle
            UpdateFormattingButtons();
        }

        private void TxtEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            isContentModified = true;
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (e == null) return;
            
            if (isContentModified)
            {
                var result = System.Windows.MessageBox.Show(
                    "Değişiklikler kaydedilmedi. Kaydetmek ister misiniz?",
                    "Uyarı",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    MenuItem_Kaydet_Click(this, new RoutedEventArgs());
                    if (isContentModified) // Hala kaydedilmediyse
                        e.Cancel = true;
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private bool CheckAndSaveBeforeAction()
        {
            if (isContentModified)
            {
                var result = System.Windows.MessageBox.Show(
                    "Değişiklikler kaydedilsin mi?",
                    "Kaydet",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    MenuItem_Kaydet_Click(this, new RoutedEventArgs());
                    return !isContentModified;
                }
                else if (result == MessageBoxResult.No)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private void MenuItem_Yeni_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckAndSaveBeforeAction())
                return;

            txtEditor.Document.Blocks.Clear();
            currentFilePath = string.Empty;
            isContentModified = false;
            this.Title = "Basit Not Defteri - Yeni Dosya";
        }

        private void MenuItem_Ac_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckAndSaveBeforeAction())
                return;

            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Zengin Metin Dosyası (*.rtf)|*.rtf|Tüm Dosyalar (*.*)|*.*";
            
            if (openFileDialog.ShowDialog() == true && openFileDialog.FileName != null)
            {
                try
                {
                    TextRange range = new TextRange(
                        txtEditor.Document.ContentStart,
                        txtEditor.Document.ContentEnd
                    );
                    using (FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open))
                    {
                        range.Load(fileStream, System.Windows.DataFormats.Rtf);
                    }
                    currentFilePath = openFileDialog.FileName;
                    isContentModified = false;
                    this.Title = $"Basit Not Defteri - {System.IO.Path.GetFileName(currentFilePath)}";
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Dosya açılırken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItem_Kaydet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(currentFilePath))
                {
                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                    saveFileDialog.Filter = "Zengin Metin Dosyası (*.rtf)|*.rtf|Tüm Dosyalar (*.*)|*.*";
                    saveFileDialog.DefaultExt = ".rtf";
                    
                    if (saveFileDialog.ShowDialog() == true && !string.IsNullOrEmpty(saveFileDialog.FileName))
                    {
                        currentFilePath = saveFileDialog.FileName;
                    }
                    else
                    {
                        return;
                    }
                }

                TextRange range = new TextRange(
                    txtEditor.Document.ContentStart,
                    txtEditor.Document.ContentEnd
                );
                using (FileStream fileStream = new FileStream(currentFilePath, FileMode.Create))
                {
                    range.Save(fileStream, System.Windows.DataFormats.Rtf);
                }
                
                isContentModified = false;
                this.Title = $"Basit Not Defteri - {System.IO.Path.GetFileName(currentFilePath)}";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Dosya kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItem_Cikis_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_YaziTipi_Click(object sender, RoutedEventArgs e)
        {
            // Basit bir yazı tipi büyütme/küçültme işlemi
            if (txtEditor.FontSize < 30)
                txtEditor.FontSize += 2;
            else
                txtEditor.FontSize = 12;
                
            isContentModified = true;
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Ctrl+K, Ctrl+I, Ctrl+U kısayol tuşları
            if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case System.Windows.Input.Key.K:
                        ToggleBold();
                        e.Handled = true;
                        break;
                    case Key.I:
                        ToggleItalic();
                        e.Handled = true;
                        break;
                    case Key.U:
                        ToggleUnderline();
                        e.Handled = true;
                        break;
                }
            }
        }

        #region Biçimlendirme Metotları

        private void ToggleBold()
        {
            if (txtEditor.Selection.GetPropertyValue(TextElement.FontWeightProperty) is FontWeight weight && weight == FontWeights.Bold)
                txtEditor.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            else
                txtEditor.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            
            UpdateFormattingButtons();
        }

        private void ToggleItalic()
        {
            if (txtEditor.Selection.GetPropertyValue(TextElement.FontStyleProperty) is System.Windows.FontStyle style && style == FontStyles.Italic)
                txtEditor.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
            else
                txtEditor.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
            
            UpdateFormattingButtons();
        }

        private void ToggleUnderline()
        {
            var currentDecorations = txtEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
            
            if (currentDecorations != null && currentDecorations.Count > 0)
                txtEditor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
            else
                txtEditor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            
            UpdateFormattingButtons();
        }

        private void UpdateFormattingButtons()
        {
            if (isUpdatingUI) return;
            
            isUpdatingUI = true;
            
            // Kalın buton durumunu güncelle
            var isBold = txtEditor.Selection.GetPropertyValue(TextElement.FontWeightProperty) is FontWeight weight && weight == FontWeights.Bold;
            MenuItem_Kalin.IsChecked = isBold;
            
            // İtalik buton durumunu güncelle
            var isItalic = txtEditor.Selection.GetPropertyValue(TextElement.FontStyleProperty) is System.Windows.FontStyle style && style == FontStyles.Italic;
            MenuItem_Italik.IsChecked = isItalic;
            
            // Altı çizili buton durumunu güncelle
            var decorations = txtEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
            var isUnderlined = decorations != null && decorations.Count > 0;
            MenuItem_AltıCizili.IsChecked = isUnderlined;
            
            isUpdatingUI = false;
        }

        private void MenuItem_Kalin_Click(object sender, RoutedEventArgs e)
        {
            ToggleBold();
        }

        private void MenuItem_Italik_Click(object sender, RoutedEventArgs e)
        {
            ToggleItalic();
        }

        private void MenuItem_AltıCizili_Click(object sender, RoutedEventArgs e)
        {
            ToggleUnderline();
        }

        private void MenuItem_YaziRengi_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                txtEditor.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
            }
        }

        private void MenuItem_ArkaplanRengi_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                txtEditor.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(color));
            }
        }

        private void ZoomCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ZoomCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Parameter is string zoomFactorStr && double.TryParse(zoomFactorStr, out double zoomFactor))
                {
                    var currentScale = txtEditor.LayoutTransform as ScaleTransform ?? new ScaleTransform(1.0, 1.0);
                    
                    if (zoomFactor == 1.0)
                    {
                        txtEditor.LayoutTransform = new ScaleTransform(1.0, 1.0);
                    }
                    else
                    {
                        double newScaleX = currentScale.ScaleX * zoomFactor;
                        double newScaleY = currentScale.ScaleY * zoomFactor;
                        
                        newScaleX = Math.Max(0.1, Math.Min(10.0, newScaleX));
                        newScaleY = Math.Max(0.1, Math.Min(10.0, newScaleY));
                        
                        txtEditor.LayoutTransform = new ScaleTransform(newScaleX, newScaleY);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Yakınlaştırma işlemi sırasında bir hata oluştu:\n\n{ex.Message}",
                    "Hata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion
    }
}