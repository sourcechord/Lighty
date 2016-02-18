using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SourceChord.Lighty
{
    /// <summary>
    /// 既存のUI上にLightBoxを被せて表示するためのAdorner定義
    /// </summary>
    public class LightBoxAdorner : Adorner
    {
        private static readonly ResourceDictionary defaultResource;
        private LightBoxItemsControl _root;

        public EventHandler AllDialogClosed;

        static LightBoxAdorner()
        {
            defaultResource = new ResourceDictionary();
            defaultResource.Source = new Uri("/Lighty;component/Styles.xaml", UriKind.Relative);
        }

        public LightBoxAdorner(UIElement adornedElement, UIElement element) : base(adornedElement)
        {
            var root = new LightBoxItemsControl();

            // ココで各種テンプレートなどの設定
            var template = LightBox.GetTemplate(element);
            root.Template = template ?? defaultResource["defaultTemplate"] as ControlTemplate;

            var itemsPanel = LightBox.GetItemsPanel(element);
            root.ItemsPanel = itemsPanel ?? defaultResource["defaultPanel"] as ItemsPanelTemplate;

            var itemContainerStyle = LightBox.GetItemContainerStyle(element);
            if (itemContainerStyle != null)
            {
                root.ItemContainerStyle = itemContainerStyle;
            }

            this.AddVisualChild(root);

            this._root = root;
        }

        public void AddDialog(FrameworkElement dialog)
        {
            this._root.Items.Add(dialog);

            // 追加したダイアログに対して、ApplicationCommands.Closeのコマンドに対するハンドラを設定。
            dialog.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e) =>
            {
                this.RemoveDialog(dialog);
            }));

            // ItemsControlにもApplicationCommands.Closeのコマンドに対するハンドラを設定。
            // (ItemsContainerからもCloseコマンドを送って閉じられるようにするため。)
            var parent = dialog.Parent as FrameworkElement;
            parent.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e) =>
            {
                this.RemoveDialog(e.Parameter as FrameworkElement);
            }));

            this.InvalidateVisual();
        }

        public void RemoveDialog(FrameworkElement dialog)
        {
            this._root.Items.Remove(dialog);

            if (this._root.Items.Count == 0)
            {
                // このAdornerを消去するように依頼するイベントを発行する。
                AllDialogClosed?.Invoke(this, null);
            }
        }

        protected override int VisualChildrenCount
        {
            get { return this._root == null ? 0 : 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException();
            return this._root;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // ルートのグリッドはAdornerを付けている要素と同じサイズになるように調整
            this._root.Width = constraint.Width;
            this._root.Height = constraint.Height;
            this._root.Measure(constraint);
            return this._root.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this._root.Arrange(new Rect(new Point(0, 0), finalSize));
            return new Size(this._root.ActualWidth, this._root.ActualHeight);
        }
    }

    class LightBoxItemsControl : ItemsControl
    {
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
    }
}
