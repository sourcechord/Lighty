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
        public LightBox Root { get; private set; }

        static LightBoxAdorner()
        {
        }

        public LightBoxAdorner(UIElement adornedElement) : base(adornedElement)
        { }

        public void SetRoot(LightBox root)
        {
            this.AddVisualChild(root);
            this.Root = root;
        }

        protected override int VisualChildrenCount
        {
            get { return this.Root == null ? 0 : 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException();
            return this.Root;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // ルートのグリッドはAdornerを付けている要素と同じサイズになるように調整
            this.Root.Width = constraint.Width;
            this.Root.Height = constraint.Height;
            this.Root.Measure(constraint);
            return this.Root.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.Root.Arrange(new Rect(new Point(0, 0), finalSize));
            return new Size(this.Root.ActualWidth, this.Root.ActualHeight);
        }
    }
}
