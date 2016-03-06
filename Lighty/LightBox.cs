using SourceChord.Lighty.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
            return new ContentControl();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
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
            await adorner.AddDialogAsync(content);
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
            var win = element as Window;
            var target = win?.Content as UIElement ?? element;

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

                // Windowに対してAdornerを設定していた場合は、Content要素のMarginを打ち消すためのマージン設定を行う。
                if (win != null)
                {
                    var content = win.Content as FrameworkElement;
                    var margin = content.Margin;
                    adorner.Margin = new Thickness(-margin.Left, -margin.Top, margin.Right, margin.Bottom);
                }

                // すべてのダイアログがクリアされたときに、Adornerを削除するための処理を追加
                adorner.AllDialogClosed += (s, e) => { layer?.Remove(adorner); };
                layer.Add(adorner);
                return adorner;
            }
        }


        #region アニメーション関係のStoryboardを実行するための各種メソッド

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var animation = this.InitializeStoryboard;
            animation?.Begin(this);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            var animation = this.OpenStoryboard;
            if (e.Action == NotifyCollectionChangedAction.Add &&
                animation != null)
            {
                foreach (FrameworkElement item in e.NewItems)
                {
                    item.Loaded += (sender, args) =>
                    {
                        var container = this.ContainerFromElement(item) as FrameworkElement;

                        var transform = new TransformGroup();
                        transform.Children.Add(new ScaleTransform());
                        transform.Children.Add(new SkewTransform());
                        transform.Children.Add(new RotateTransform());
                        transform.Children.Add(new TranslateTransform());
                        container.RenderTransform = transform;
                        container.RenderTransformOrigin = new Point(0.5, 0.5);

                        animation.Begin(container);
                    };
                }
            }
        }

        public async Task<bool> Closing()
        {
            return await this.DisposeStoryboard.BeginAsync(this);
        }

        public async Task<bool> ClosingDialog(FrameworkElement item)
        {
            var container = this.ContainerFromElement(item) as FrameworkElement;
            return await this.CloseStoryboard.BeginAsync(container); ;
        }

        #endregion

        #region アニメーション関係のプロパティ

        public Storyboard OpenStoryboard
        {
            get { return (Storyboard)GetValue(OpenStoryboardProperty); }
            set { SetValue(OpenStoryboardProperty, value); }
        }
        // Using a DependencyProperty as the backing store for OpenStoryboard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenStoryboardProperty =
            DependencyProperty.Register("OpenStoryboard", typeof(Storyboard), typeof(LightBox), new PropertyMetadata(null));


        public Storyboard CloseStoryboard
        {
            get { return (Storyboard)GetValue(CloseStoryboardProperty); }
            set { SetValue(CloseStoryboardProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CloseStoryboard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseStoryboardProperty =
            DependencyProperty.Register("CloseStoryboard", typeof(Storyboard), typeof(LightBox), new PropertyMetadata(null));


        public Storyboard InitializeStoryboard
        {
            get { return (Storyboard)GetValue(InitializeStoryboardProperty); }
            set { SetValue(InitializeStoryboardProperty, value); }
        }
        // Using a DependencyProperty as the backing store for InitializeStoryboard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InitializeStoryboardProperty =
            DependencyProperty.Register("InitializeStoryboard", typeof(Storyboard), typeof(LightBox), new PropertyMetadata(null));


        public Storyboard DisposeStoryboard
        {
            get { return (Storyboard)GetValue(DisposeStoryboardProperty); }
            set { SetValue(DisposeStoryboardProperty, value); }
        }
        // Using a DependencyProperty as the backing store for DisposeStoryboard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisposeStoryboardProperty =
            DependencyProperty.Register("DisposeStoryboard", typeof(Storyboard), typeof(LightBox), new PropertyMetadata(null));

        #endregion
    }
}
