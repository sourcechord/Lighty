using SourceChord.Lighty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LightySample
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnClickShowButton(object sender, RoutedEventArgs e)
        {
            LightBox.Show(this, new SampleDialog());
            MessageBox.Show("Hello.");
        }

        private void OnClickShowDialogButton(object sender, RoutedEventArgs e)
        {
            LightBox.ShowDialog(this, new SampleDialog());
            MessageBox.Show("Hello.");
        }

        private async void OnClickShowAsyncButton(object sender, RoutedEventArgs e)
        {
            await LightBox.ShowAsync(this, new SampleDialog());
            MessageBox.Show("Hello.");
        }

        private async void OnClickShowMultiple(object sender, RoutedEventArgs e)
        {
            LightBox.Show(this, new SampleDialog());

            await Task.Delay(1000);
            LightBox.Show(this, new SampleDialog());

            await Task.Delay(1000);
            LightBox.Show(this, new SampleDialog());
        }

        private void OnClickShowUserControl(object sender, RoutedEventArgs e)
        {
            LightBox.Show(this, new SampleDialog());
        }
        private void OnClickShowImage(object sender, RoutedEventArgs e)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri("Images/1.jpg", UriKind.Relative));
            LightBox.Show(this, image);
        }
    }
}
