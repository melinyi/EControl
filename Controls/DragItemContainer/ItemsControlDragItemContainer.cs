using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        /// <summary>
        /// 拖拽方向
        /// </summary>
        public EControl.Data.DragDirection DragDirection
        {
            get { return (EControl.Data.DragDirection)GetValue(DragDirectionProperty); }
            set { SetValue(DragDirectionProperty, value); }
        }

        /// <summary>
        /// 拖拽方向
        /// </summary>
        public static readonly DependencyProperty DragDirectionProperty =
            DependencyProperty.Register("DragDirection", typeof(EControl.Data.DragDirection), typeof(ItemsControlDragItemContainer), new PropertyMetadata(EControl.Data.DragDirection.TopAndBottom));

        /// <summary>
        /// 第一次鼠标按下的坐标（变量）
        /// </summary>
        private Point? DragFirstPoint;
        /// <summary>
        /// 记录第一次鼠标按下的坐标
        /// </summary>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            DragFirstPoint = e.GetPosition(this);
            base.OnPreviewMouseDown(e);
        }
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && DragFirstPoint != null)
            {
                // 获取按下鼠标后的偏移量，大于10则进行拖拽操作
                if (Math.Abs(e.GetPosition(this).X - DragFirstPoint.Value.X) > 10 || Math.Abs(e.GetPosition(this).Y - DragFirstPoint.Value.Y) > 10)
                {
                    // 获取当前鼠标按住的对象上下文
                    var from = EControl.Tools.Helper.DragMoveSortHelper.GetDataContext(this);
                    // 开始拖拽
                    DragDrop.DoDragDrop(GetItemsControl, from, DragDropEffects.Move);
                }
            }
            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            var point = e.GetPosition(this);

            if (DragDirection == Data.DragDirection.TopAndBottom)
            {
                // this.Height -= 3;
                if (point.Y > this.ActualHeight / 2)
                {
                    this.BorderThickness = new Thickness(0, 0, 0, 3);
                }
                else
                {
                    this.BorderThickness = new Thickness(0, 3, 0, 0);
                }
            }
            else
            {
                // this.Width -= 3;
                if (point.X > this.ActualWidth / 2)
                {
                    this.BorderThickness = new Thickness(0, 0, 3, 0);
                }
                else
                {
                    this.BorderThickness = new Thickness(3, 0, 0, 0);
                }
            }
            base.OnPreviewDragEnter(e);
        }

        protected override void OnPreviewDragOver(DragEventArgs e)
        {
            base.OnPreviewDragOver(e);
        }

        void ReStore()
        {
            this.BorderThickness = new Thickness(0, 0, 0, 0);
        }

        /// <summary>
        /// 离开拖拽范围
        /// </summary>
        protected override void OnPreviewDragLeave(DragEventArgs e)
        {
            ReStore();
            base.OnPreviewDragLeave(e);
        }

        /// <summary>
        /// 拖拽完成
        /// </summary>
        protected override void OnPreviewDrop(DragEventArgs e)
        {
            ReStore();
            if (GetItemsControl?.ItemsSource != null)
            {
                EControl.Tools.Helper.DragMoveSortHelper.ChangeItemIndex((System.Collections.IList)GetItemsControl!.ItemsSource, this, e, DragDirection);
            }
            base.OnPreviewDrop(e);
        }
    }
}
