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
using System.Windows.Threading;

namespace SourceChord.Lighty
{
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SourceChord.Lighty"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SourceChord.Lighty;assembly=SourceChord.Lighty"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:LightBox/>
    ///
    /// </summary>
    public class LightBox : ItemsControl
    {
        static LightBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LightBox), new FrameworkPropertyMetadata(typeof(LightBox)));
        }


        protected override DependencyObject GetContainerForItemOverride()
        {
            //return base.GetContainerForItemOverride();
            return new ContentControl();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            //return base.IsItemItsOwnContainerOverride(item);
            return false;
        }

        /// <summary>
        /// LightBoxをモードレス表示します。
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="content"></param>
        public static void Show(UIElement owner, FrameworkElement content)
        {
            var adorner = GetAdorner(owner);
            adorner.AddDialog(content);
        }

        /// <summary>
        /// LightBoxを非同期でモードレス表示します。
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task ShowAsync(UIElement owner, FrameworkElement content)
        {
            var adorner = GetAdorner(owner);
            await adorner.ShowDialog(content);
        }

        /// <summary>
        /// LightBoxをモーダル表示します。
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="content"></param>
        public static void ShowDialog(UIElement owner, FrameworkElement content)
        {
            var adorner = GetAdorner(owner);

            var frame = new DispatcherFrame();
            adorner.AllDialogClosed += (s, e) => { frame.Continue = false; };
            adorner.AddDialog(content);

            Dispatcher.PushFrame(frame);
        }

        private static LightBoxAdorner GetAdorner(UIElement element)
        {
            // Window系のクラスだったら、Contentプロパティを利用。それ以外の場合はそのまま利用。
            var target = (element as Window)?.Content as UIElement ?? element;

            if (element == null) return null;
            var layer = AdornerLayer.GetAdornerLayer(target);
            if (layer == null) return null;

            var current = layer.GetAdorners(target)
                               ?.OfType<LightBoxAdorner>()
                               ?.SingleOrDefault();

            if (current != null)
            {
                return current;
            }
            else
            {
                // ダイアログ用のAdornerが存在してないので、新規に作って設定して返す。
                var adorner = new LightBoxAdorner(target, element);

                // すべてのダイアログがクリアされたときに、Adornerを削除するための処理を追加
                adorner.AllDialogClosed += (s, e) => { layer?.Remove(adorner); };
                layer.Add(adorner);
                return adorner;
            }
        }
    }
}
