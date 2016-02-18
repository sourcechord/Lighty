using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace SourceChord.Lighty
{
    public class LightBox
    {
        public static void Show(UIElement owner, FrameworkElement content)
        {
            var adorner = GetAdorner(owner);
            adorner.AddDialog(content);
        }

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
                adorner.AllDialogClosed += (s, e) => { ClearAdorner(layer, adorner); };
                layer.Add(adorner);
                return adorner;
            }
        }


        private static void ClearAdorner(AdornerLayer layer, LightBoxAdorner adorner)
        {
            // null条件演算子でいいかも。
            if (layer != null && adorner != null)
            {
                layer.Remove(adorner);
            }
        }


        #region 色々と添付プロパティの定義

        public static ControlTemplate GetTemplate(DependencyObject obj)
        {
            return (ControlTemplate)obj.GetValue(TemplateProperty);
        }
        public static void SetTemplate(DependencyObject obj, ControlTemplate value)
        {
            obj.SetValue(TemplateProperty, value);
        }
        // Using a DependencyProperty as the backing store for Template.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TemplateProperty =
            DependencyProperty.RegisterAttached("Template", typeof(ControlTemplate), typeof(LightBox), new PropertyMetadata(null));


        public static ItemsPanelTemplate GetItemsPanel(DependencyObject obj)
        {
            return (ItemsPanelTemplate)obj.GetValue(ItemsPanelProperty);
        }
        public static void SetItemsPanel(DependencyObject obj, ItemsPanelTemplate value)
        {
            obj.SetValue(ItemsPanelProperty, value);
        }
        // Using a DependencyProperty as the backing store for ItemsPanel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsPanelProperty =
            DependencyProperty.RegisterAttached("ItemsPanel", typeof(ItemsPanelTemplate), typeof(LightBox), new PropertyMetadata(null));


        public static Style GetItemContainerStyle(DependencyObject obj)
        {
            return (Style)obj.GetValue(ItemContainerStyleProperty);
        }
        public static void SetItemContainerStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(ItemContainerStyleProperty, value);
        }
        // Using a DependencyProperty as the backing store for ItemContainerStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.RegisterAttached("ItemContainerStyle", typeof(Style), typeof(LightBox), new PropertyMetadata(null));

        #endregion
    }
}
