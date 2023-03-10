using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EControl.Controls
{
    /// <summary>
    /// 支持拖拽的容器控件，操作UI显示拖拽的边框。实际为调整绑定合集的顺序。
    /// </summary>
    public class ItemsControlDragItemContainer : Border
    {
        public ItemsControlDragItemContainer()
        {
            this.AllowDrop = true;
            this.Padding = new Thickness(0, 0, 0, 0);
            this.Margin = new Thickness(0, 0, 0, 0);
            //this.BorderBrush = Brushes.Orange;
        }

        public ItemsControl? GetItemsControl => EControl.Tools.Helper.VisualHelper.GetParent<ItemsControl>(this);

        public EControl.Data.DragDirection DragDirection
        {
            get { return (EControl.Data.DragDirection)GetValue(DragDirectionProperty); }
            set { SetValue(DragDirectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for  DragDirection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragDirectionProperty =
            DependencyProperty.Register("DragDirection", typeof(EControl.Data.DragDirection), typeof(ItemsControlDragItemContainer), new PropertyMetadata(EControl.Data.DragDirection.TopAndBottom));

        private Point DragFirstPoint;
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e) => DragFirstPoint = e.GetPosition(this);
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Math.Abs(e.GetPosition(this).X - DragFirstPoint.X) > 10 || Math.Abs(e.GetPosition(this).Y - DragFirstPoint.Y) > 10)
                {
                    var from = EControl.Tools.Helper.DragMoveSortHelper.GetDataContext(this);
                    DragDrop.DoDragDrop(GetItemsControl, from, DragDropEffects.Move);
                }
            }
        }

        protected override void OnPreviewDragOver(DragEventArgs e)
        {
            var point = e.GetPosition(this);
            this.BorderThickness = (DragDirection == Data.DragDirection.TopAndBottom) ? (point.Y > this.ActualHeight / 2) ? new Thickness(0, 0, 0, 3) : new Thickness(0, 3, 0, 0) :
                (point.X > this.ActualWidth / 2) ? new Thickness(0, 0, 3, 0) : new Thickness(3, 0, 0, 0);
        }

        protected override void OnPreviewDragLeave(DragEventArgs e) => this.BorderThickness = new Thickness(0, 0, 0, 0);

        protected override void OnPreviewDrop(DragEventArgs e)
        {
            this.BorderThickness = new Thickness(0, 0, 0, 0);
            if (GetItemsControl?.ItemsSource != null)
            {
                EControl.Tools.Helper.DragMoveSortHelper.ChangeItemIndex((System.Collections.IList)GetItemsControl!.ItemsSource, this, e, DragDirection);
            }
        }
    }
}
