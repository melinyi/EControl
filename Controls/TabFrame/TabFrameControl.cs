﻿using EControl.Controls.TabFrame.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EControl.Controls.TabFrame
{
    public class TabFrameControl : TabControl
    {

        #region 页面注册于管理

        /// <summary>
        /// 获取当前注册的页面合集
        /// </summary>
        public ItemCollection PageCollection => Items;

        public void RegisterPage<T>() where T : ITabPage, new()
        {
            if (PageCollection.FindPageFirstOrNull<T>() != null)
            {
                throw new InvalidOperationException($"页面类或名称 类名称：{typeof(T).FullName} 已注册过!");
            }

            if (System.Activator.CreateInstance(typeof(T)) is ITabPage Page)
            {
                Items.Add(Page);
            }
            else
            {
                throw new InvalidOperationException($"页面 {typeof(T).FullName} 未实现 ITabPage 接口!");
            }
        }

        /// <summary>
        /// 获取指定页
        /// </summary>
        /// <typeparam name="T">页</typeparam>
        /// <returns>返回指定页面</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public T GetPage<T>() where T : ITabPage, new()
        {
            if (Items.FindPageFirstOrNull<T>() is T res)
            {
                return res;
            }
            throw new InvalidOperationException($"页面类或名称 类名称：{typeof(T).FullName} 未注册过!");
        }

        /// <summary>
        /// 获取是否当前页面为指定页
        /// </summary>
        public bool IsCurrentPage<T>() => this.SelectedItem is T;

        public void Navigate<T>(object? param)
        {
            var pageItem = Items.FindPageFirstOrNull<T>();

            if (pageItem == null)
            {
                throw new InvalidOperationException($"页面 {typeof(T).FullName} 未注册!");
            }

            if (pageItem is ITabPage page)
            {
                if (CurrentPageCode.Key == page.Name && param == null) return;
                //if (CurrentPageCode.Key.Equals(type) && param != null && CurrentPageCode.Value.Equals(param)) return;

                //如果先前一页非最后一页，则删除当前可前进的条目
                if (HistoryIndex != HistoryList.Count - 1)
                {
                    HistoryList = HistoryList.Take(HistoryIndex + 1).ToList();
                }

                HistoryList.Add(new KeyValuePair<string, object?>(page.Name, param));
                HistoryIndex = HistoryList.Count - 1;

                SetPage(page, param);
            }
            else
            {
                throw new InvalidOperationException($"页面 {typeof(T).FullName} 未实现 ITabPage 接口!");
            }
        }

        //public void Navigate(string pageName, object param)
        //{
        //    var page = Items.FindPageFirstOrNull(pageName);

        //    if (page is ITabPage tabPage)
        //    {
        //        Navigate(tabPage, param);
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException($"页面 {pageName} 未注册!");
        //    }
        //}

        private void SetPage(ITabPage page, object? param)
        {
            SelectedItem = page;
            UpdateUI(new KeyValuePair<string, object?>(page.Name, param));
            //执行页面参数
            page.NavigateContinueWithInvoke?.Invoke(param);
        }

        #endregion

        #region 页面历史记录及前进后退判断
        /// <summary>
        /// Key Is nameof Token , Value object Is Param
        /// </summary>
        private List<KeyValuePair<string, object?>> HistoryList { get; set; } = new List<KeyValuePair<string, object?>>();

        private KeyValuePair<string, object?> CurrentPageCode;
        private int HistoryIndex { get; set; }

        public bool CanGoBack => HistoryIndex != 0 && HistoryList.Count > 1 ? true : false;

        public bool CanGoForward => HistoryIndex < HistoryList.Count - 1 ? true : false;


        private void PageJump(int index)
        {
            CurrentPageCode = HistoryList.ElementAt(index);

            var page = Items.FindPageFirstOrNull(CurrentPageCode.Key);

            if (page is ITabPage tabPage)
            {
                SetPage(tabPage, CurrentPageCode.Value);
            }
        }

        public void GoBack()
        {
            if (!CanGoBack) return;
            HistoryIndex -= 1;
            PageJump(HistoryIndex);
        }

        public void GoForward()
        {
            if (!CanGoForward) return;
            HistoryIndex += 1;
            PageJump(HistoryIndex);
        }


        private void UpdateUI(KeyValuePair<string, object?> HistoryItem)
        {
            CurrentPageCode = HistoryItem;
            if (GoBackControl != null) SetBackOrForwardControl(GoBackControl, CanGoBack);
            if (GoForwardControl != null) SetBackOrForwardControl(GoForwardControl, CanGoForward);
        }

        private void SetBackOrForwardControl(Control control, bool value)
        {
            if (control != null)
            {
                control.IsEnabled = value;
            }
        }



        private Control? _GoBackControl;
        /// <summary>
        /// 获取或设置后退控件，一般为Button
        /// </summary>
        public Control? GoBackControl
        {
            get { return _GoBackControl; }
            set
            {
                _GoBackControl = value;
                if (_GoBackControl != null)
                {
                    _GoBackControl.PreviewMouseDown -= GoBackControl_MouseDown;
                    _GoBackControl.PreviewMouseDown += GoBackControl_MouseDown;
                }
            }
        }

        private Control? _GoForwardControl;
        /// <summary>
        /// 获取或设置前进控件，一般为Button
        /// </summary>
        public Control? GoForwardControl
        {
            get { return _GoForwardControl; }
            set
            {
                _GoForwardControl = value;
                if (_GoForwardControl != null)
                {
                    _GoForwardControl!.PreviewMouseDown -= GoForwardControl_MouseDown;
                    _GoForwardControl.PreviewMouseDown += GoForwardControl_MouseDown;
                }
            }
        }

        private void GoBackControl_MouseDown(object sender, MouseButtonEventArgs e) => GoBack();
        private void GoForwardControl_MouseDown(object sender, MouseButtonEventArgs e) => GoForward();

        #endregion

        /// <summary>
        /// 初始化控件样式
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            BorderThickness = new System.Windows.Thickness(0);
            BorderBrush = null;
            Background = null;
            Foreground = null;

            var tabItemStyle = new System.Windows.Style()
            {
                TargetType = typeof(TabItem),
            };

            Padding = new System.Windows.Thickness(0);

            tabItemStyle.Setters.Add(new Setter(VisibilityProperty, Visibility.Collapsed));
            ItemContainerStyle = tabItemStyle;
        }

        public void ClearHistory()
        {
            if (GoBackControl != null) GoBackControl.IsEnabled = false;
            if (GoForwardControl != null) GoForwardControl.IsEnabled = false;

            HistoryList.Clear();
        }
    }

    /// <summary>
    /// 扩展
    /// </summary>
    public static class TabFrameControlExtension
    {
        public static object? FindPageFirstOrNull<T>(this ItemCollection value)
        {
            //eVideoHelper.eVideoHelper.ShowMainWindowGrowlMessage(nameofToken, Data.Enum.Base.MessageboxImageType.Waring);
            if (value?.Count > 0)
            {
                foreach (var item in value)
                {
                    if (item.GetType() == typeof(T))
                    {
                        return item;
                    }
                    //eVideoHelper.eVideoHelper.ShowMainWindowGrowlMessage(item.GetType().Name, Data.Enum.Base.MessageboxImageType.Waring);

                }
            }

            return null;
        }

        public static object? FindPageFirstOrNull(this ItemCollection value, string pageName)
        {
            //eVideoHelper.eVideoHelper.ShowMainWindowGrowlMessage(nameofToken, Data.Enum.Base.MessageboxImageType.Waring);
            if (value?.Count > 0)
            {
                foreach (var item in value)
                {
                    if (item is ITabPage page && page.Name == pageName)
                    {
                        return item;
                    }
                    //eVideoHelper.eVideoHelper.ShowMainWindowGrowlMessage(item.GetType().Name, Data.Enum.Base.MessageboxImageType.Waring);

                }
            }

            return null;
        }
    }
}
