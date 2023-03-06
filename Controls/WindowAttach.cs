using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace EControl.Controls
{
    public static class WindowAttach
    {

        // Using a DependencyProperty as the backing store for DialogEnableCloseButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DialogEnableHandyCloseButtonProperty =
            DependencyProperty.RegisterAttached("DialogEnableHandyCloseButton", typeof(bool), typeof(WindowAttach), new PropertyMetadata(false, OnDialogEnableHandyCloseButtonPropertyChanged));

        private static void OnDialogEnableHandyCloseButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if ((bool)e.NewValue)
                {
#pragma warning disable CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
                    window.ContentRendered += Window_ContentRendered_DialogEnableHandyCloseButtonProperty;
#pragma warning restore CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
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
#pragma warning disable CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
                            window.ContentRendered -= Window_ContentRendered_DialogEnableHandyCloseButtonProperty;
#pragma warning restore CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
                        }
                    }               
                }             
            }
        }

        public static bool GetDialogEnableCloseButton(DependencyObject obj)
        {
            return (bool)obj.GetValue(DialogEnableHandyCloseButtonProperty);
        }

        public static void SetDialogEnableCloseButton(DependencyObject obj, bool value)
        {
            obj.SetValue(DialogEnableHandyCloseButtonProperty, value);
        }



    }
}
