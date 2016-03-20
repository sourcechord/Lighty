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

namespace PhotoViewer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // DataContext変更時にViewModelプロパティの値が変わるように
            // イベントハンドラの設定
            this.ViewModel = this.DataContext as MainWindowViewModel;
            this.DataContextChanged += (s, e) =>
            {
                this.ViewModel = this.DataContext as MainWindowViewModel;
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                var ret = dlg.ShowDialog();
                if (ret != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                // 選択されたフォルダ内にある画像を列挙
                this.ViewModel.SetSourceDirectory(dlg.SelectedPath);
            }
        }

        private void OnItemDoubleClicked(object sender, MouseEventArgs e)
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(App.Current.MainWindow);
            var screen = System.Windows.Forms.Screen.FromHandle(helper.Handle);
            var height = screen.Bounds.Height;

            var path = this.lstPhoto.SelectedItem as Uri;
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = path;
            bmp.DecodePixelHeight = height;
            bmp.EndInit();

            var image = new Image() { Source = bmp};
            LightBox.Show(this, image);
        }
    }
}
