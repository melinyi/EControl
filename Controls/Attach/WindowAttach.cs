using EControl.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using static System.Net.Mime.MediaTypeNames;

namespace EControl.Controls
{
    public static partial class WindowAttach
    {
        /// <summary>
        /// 使用Dialog元素时，禁用 HandyControl.Window 窗体关闭按钮
        /// </summary>
        public static readonly DependencyProperty DialogEnableHandyCloseButtonProperty =
            DependencyProperty.RegisterAttached("DialogEnableHandyCloseButton", typeof(bool), typeof(WindowAttach), new PropertyMetadata(false, OnDialogEnableHandyCloseButtonPropertyChanged));

        private static void OnDialogEnableHandyCloseButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if (e.NewValue is true)
                {
                    window.ContentRendered += Window_ContentRendered_DialogEnableHandyCloseButtonProperty!;
                }
                else
                {
                    if (window.Template?.FindName("ButtonClose", window) is System.Windows.Controls.Button button)
                        button.SetBinding(System.Windows.Controls.Button.IsEnabledProperty, "");
                }

                void Window_ContentRendered_DialogEnableHandyCloseButtonProperty(object sender, EventArgs e)
                {
                    if (window.Template?.FindName("ButtonClose", window) is System.Windows.Controls.Button button)
                    {
                        if (Tools.Helper.VisualHelper.GetChild<System.Windows.Documents.AdornerDecorator>(window) is System.Windows.Documents.AdornerDecorator ado)
                        {
                            button.SetBinding(System.Windows.Controls.Button.IsEnabledProperty, new Binding() { Source = ado.Child, Path = new PropertyPath("IsEnabled") });
                            window.ContentRendered -= Window_ContentRendered_DialogEnableHandyCloseButtonProperty!;
                        }
                    }
                }
            }
        }

        public static bool GetDialogEnableHandyCloseButton(DependencyObject obj) => (bool)obj.GetValue(DialogEnableHandyCloseButtonProperty);

        public static void SetDialogEnableHandyCloseButton(DependencyObject obj, bool value) => obj.SetValue(DialogEnableHandyCloseButtonProperty, value);
    }

    public static partial class WindowAttach
    {
        /// <summary>
        /// 记忆窗口的位置及大小
        /// </summary>
        public static readonly DependencyProperty IsSaveWindowSizeProperty =
            DependencyProperty.RegisterAttached("IsSaveWindowSize", typeof(bool), typeof(WindowAttach), new PropertyMetadata(false, OnIsSaveWindowSizePropertyChanged));

        private static void OnIsSaveWindowSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if (e.NewValue is true)
                {
                    window.Closing += Window_Closed;
                    window.Initialized += Window_Initialized;
                }
                else
                {
                    window.Closing -= Window_Closed;
                    window.Initialized -= Window_Initialized;
                }

                void Window_Closed(object? sender, EventArgs e)
                {
                    try
                    {
                        if (sender is Window w)
                        {
                            window.WindowState = WindowState.Normal;
                            var content = System.Text.Json.JsonSerializer.Serialize(new WindowSizeAndPoint()
                            {
                                Height = w.Height,
                                Width = w.Width,
                                Left = w.Left,
                                Top = w.Top,
                            });
                            File.WriteAllText(GetJson(true), content);
                        }
                    }
                    catch { }
                }

                void Window_Initialized(object? sender, EventArgs e)
                {
                    try
                    {
                        if (sender is Window w)
                        {
                            var json = GetJson();
                            if (File.Exists(json))
                            {
                                if (System.Text.Json.JsonSerializer.Deserialize<WindowSizeAndPoint>(File.ReadAllText(json)) is WindowSizeAndPoint windowSize)
                                {
                                    
                                    w.Height = windowSize.Height > 0 && windowSize.Height<= SystemParameters.PrimaryScreenHeight ? windowSize.Height : w.Height;
                                    w.Width = windowSize.Width > 0 && windowSize.Height <= SystemParameters.PrimaryScreenWidth ? windowSize.Width : w.Width;
                                    //w.Top = windowSize.Top > 0 ? windowSize.Top : w.Top;
                                    //w.Left = windowSize.Left > 0 ? windowSize.Left : w.Left;
                                }
                            }
                        }
                    }
                    catch { }
                }

                string GetJson(bool isCreateFolder =false)
                {
                    var config = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WindowConfig");
                    if (isCreateFolder) Directory.CreateDirectory(config);
                    return Path.Combine(config, $"{window.GetType().Name}.json");
                }
            }
        }

        public static bool GetIsSaveWindowSize(DependencyObject obj) => (bool)obj.GetValue(IsSaveWindowSizeProperty);

        public static void SetIsSaveWindowSize(DependencyObject obj, bool value) => obj.SetValue(IsSaveWindowSizeProperty, value);

    }
}
