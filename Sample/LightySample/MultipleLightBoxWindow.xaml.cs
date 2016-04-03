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
using System.Windows.Shapes;

namespace LightySample
{
    /// <summary>
    /// MultipleLightBoxWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MultipleLightBoxWindow : Window
    {
        public MultipleLightBoxWindow()
        {
            InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            LightBox.Show(this, new SampleDialog());

            await Task.Delay(1000);
            LightBox.Show(this, new SampleDialog());

            await Task.Delay(1000);
            LightBox.Show(this, new SampleDialog());
        }
    }
}
