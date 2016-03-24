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
        private Action<FrameworkElement> _closedDelegate;

        public static EventHandler AllDialogClosed;

        public EventHandler CompleteInitializeLightBox;

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
        public static async void Show(UIElement owner, FrameworkElement content)
        {
            var adorner = GetAdorner(owner);
            if (adorner == null) { adorner = await CreateAdornerAsync(owner); }
            adorner.Root?.AddDialog(content);
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
            if (adorner == null) { adorner = await CreateAdornerAsync(owner); }
            await adorner.Root?.AddDialogAsync(content);
        }

        /// <summary>
        /// LightBoxをモーダル表示します。
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="content"></param>
        public static void ShowDialog(UIElement owner, FrameworkElement content)
        {
            var adorner = GetAdorner(owner);
            if (adorner == null) { adorner = CreateAdornerModal(owner); }

            var frame = new DispatcherFrame();
            LightBox.AllDialogClosed += (s, e) => { frame.Continue = false; };
            adorner.Root?.AddDialog(content);

            Dispatcher.PushFrame(frame);
        }

        private static LightBoxAdorner GetAdorner(UIElement element)
        {
            // Window系のクラスだったら、Contentプロパティを利用。それ以外の場合はそのまま利用。
            var win = element as Window;
            var target = win?.Content as UIElement ?? element;

            if (target == null) return null;
            var layer = AdornerLayer.GetAdornerLayer(target);
            if (layer == null) return null;

            var current = layer.GetAdorners(target)
                               ?.OfType<LightBoxAdorner>()
                               ?.SingleOrDefault();

            return current;
        }

        private static LightBoxAdorner CreateAdornerCore(UIElement element, LightBox lightbox)
        {
            // Window系のクラスだったら、Contentプロパティを利用。それ以外の場合はそのまま利用。
            var win = element as Window;
            var target = win?.Content as UIElement ?? element;

            if (target == null) return null;
            var layer = AdornerLayer.GetAdornerLayer(target);
            if (layer == null) return null;

            // ダイアログ用のAdornerが存在してないので、新規に作って設定して返す。
            var adorner = new LightBoxAdorner(target);
            adorner.SetRoot(lightbox);

            // Windowに対してAdornerを設定していた場合は、Content要素のMarginを打ち消すためのマージン設定を行う。
            if (win != null)
            {
                var content = win.Content as FrameworkElement;
                var margin = content.Margin;
                adorner.Margin = new Thickness(-margin.Left, -margin.Top, margin.Right, margin.Bottom);
            }

            // すべてのダイアログがクリアされたときに、Adornerを削除するための処理を追加
            LightBox.AllDialogClosed += (s, e) => { layer?.Remove(adorner); };
            layer.Add(adorner);
            return adorner;
        }

        protected static LightBoxAdorner CreateAdorner(UIElement element)
        {
            return CreateAdornerCore(element, new LightBox());
        }

        protected static Task<LightBoxAdorner> CreateAdornerAsync(UIElement element)
        {
            var tcs = new TaskCompletionSource<LightBoxAdorner>();

            var lightbox = new LightBox();
            var adorner = CreateAdornerCore(element, lightbox);
            lightbox.Loaded += (s, e) =>
            {
                // アニメーションを並列で実行する場合 or 
                // lightbox背景のアニメーションが無い場合は、
                // この非同期メソッドは即完了とする
                if (lightbox.IsParallelInitialize ||
                    lightbox.InitializeStoryboard == null)
                {
                    tcs.SetResult(adorner);
                }
                else
                {
                    lightbox.CompleteInitializeLightBox += (_s, _e) =>
                    {
                        tcs.SetResult(adorner);
                    };
                }
            };

            return tcs.Task;
        }

        protected static LightBoxAdorner CreateAdornerModal(UIElement element)
        {
            var lightbox = new LightBox();

            var adorner = CreateAdornerCore(element, lightbox);
            if (!lightbox.IsParallelInitialize)
            {
                var frame = new DispatcherFrame();
                lightbox.CompleteInitializeLightBox += (s, e) =>
                {
                    frame.Continue = false;
                };

                Dispatcher.PushFrame(frame);
            }

            return adorner;
        }

        #region ダイアログ表示関係の処理
        /// <summary>
        /// 引数で渡されたFrameworkElementを、表示中のダイアログ項目に追加します。
        /// </summary>
        /// <param name="dialog"></param>
        protected void AddDialog(FrameworkElement dialog)
        {
            this.Items.Add(dialog);

            // 追加したダイアログに対して、ApplicationCommands.Closeのコマンドに対するハンドラを設定。
            dialog.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, async (s, e) =>
            {
                await this.RemoveDialogAsync(dialog);
            }));

            // ItemsControlにもApplicationCommands.Closeのコマンドに対するハンドラを設定。
            // (ItemsContainerからもCloseコマンドを送って閉じられるようにするため。)
            var parent = dialog.Parent as FrameworkElement;
            parent.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, async (s, e) =>
            {
                await this.RemoveDialogAsync(e.Parameter as FrameworkElement);
            }));

            this.InvalidateVisual();
        }

        protected async Task<bool> AddDialogAsync(FrameworkElement dialog)
        {
            var tcs = new TaskCompletionSource<bool>();

            var closedHandler = new Action<FrameworkElement>((d) => { });
            closedHandler = new Action<FrameworkElement>((d) =>
            {
                if (d == dialog)
                {
                    tcs.SetResult(true);
                    this._closedDelegate -= closedHandler;
                }
            });
            this._closedDelegate += closedHandler;

            this.AddDialog(dialog);

            return await tcs.Task;
        }

        protected async Task RemoveDialogAsync(FrameworkElement dialog)
        {
            var index = this.Items.IndexOf(dialog);
            var count = this.Items.Count;

            if (this.IsParallelDispose)
            {
                this.ClosingDialog(dialog);
            }
            else
            {
                await this.ClosingDialog(dialog);
            }
            if (index != -1 && count == 1)
            {
                await this.Closing();
                // このAdornerを消去するように依頼するイベントを発行する。
                LightBox.AllDialogClosed?.Invoke(this, null);
            }

            this._closedDelegate?.Invoke(dialog);
        }

        #endregion


        #region アニメーション関係のStoryboardを実行するための各種メソッド

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.Opening();
        }

        public async Task Opening()
        {
            var animation = this.InitializeStoryboard;
            await animation.BeginAsync(this);
            this.CompleteInitializeLightBox?.Invoke(this, null);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var animation = this.OpenStoryboard;
                foreach (FrameworkElement item in e.NewItems)
                {
                    item.Loaded += (sender, args) =>
                    {
                        var container = this.ContainerFromElement(item) as FrameworkElement;
                        container.Focus();

                        var transform = new TransformGroup();
                        transform.Children.Add(new ScaleTransform());
                        transform.Children.Add(new SkewTransform());
                        transform.Children.Add(new RotateTransform());
                        transform.Children.Add(new TranslateTransform());
                        container.RenderTransform = transform;
                        container.RenderTransformOrigin = new Point(0.5, 0.5);

                        animation?.BeginAsync(container);
                    };
                }
            }
        }

        protected async Task<bool> Closing()
        {
            return await this.DisposeStoryboard.BeginAsync(this);
        }

        protected async Task<bool> ClosingDialog(FrameworkElement item)
        {
            var container = this.ContainerFromElement(item) as FrameworkElement;
            await this.CloseStoryboard.BeginAsync(container);
            this.Items.Remove(item);
            return true;
        }

        #endregion

        #region アニメーション関係のプロパティ

        public bool IsParallelInitialize
        {
            get { return (bool)GetValue(IsParallelInitializeProperty); }
            set { SetValue(IsParallelInitializeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsParallelInitialize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsParallelInitializeProperty =
            DependencyProperty.Register("IsParallelInitialize", typeof(bool), typeof(LightBox), new PropertyMetadata(false));


        public bool IsParallelDispose
        {
            get { return (bool)GetValue(IsParallelDisposeProperty); }
            set { SetValue(IsParallelDisposeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsParallelDispose.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsParallelDisposeProperty =
            DependencyProperty.Register("IsParallelDispose", typeof(bool), typeof(LightBox), new PropertyMetadata(false));




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
