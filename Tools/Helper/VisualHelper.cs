﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace EControl.Tools.Helper
{
    public static class VisualHelper
    {
        /// <summary>
        /// 尝试获取可视状态组
        /// </summary>
        /// <param name="d"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        internal static VisualStateGroup? TryGetVisualStateGroup(DependencyObject d, string groupName)
        {
            var root = GetImplementationRoot(d);
            if (root == null) return null;

            return VisualStateManager
                .GetVisualStateGroups(root)?
                .OfType<VisualStateGroup>()
                .FirstOrDefault(group => string.CompareOrdinal(groupName, group.Name) == 0);
        }

        /// <summary>
        /// 获取实现根目录
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        internal static FrameworkElement? GetImplementationRoot(DependencyObject d) =>
            1 == VisualTreeHelper.GetChildrenCount(d)
                ? VisualTreeHelper.GetChild(d, 0) as FrameworkElement
                : null;

        public static T? GetChild<T>(DependencyObject d) where T : DependencyObject
        {
            if (d == null) return default;
            if (d is T t) return t;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);

                var result = GetChild<T>(child);
                if (result != null) return result;
            }
            return default;
        }

        public static List<T>? GetChilds<T>(DependencyObject d) where T : DependencyObject
        {
            if (d == null) return default;

            List<T> results = new List<T>();

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);

                var result = GetChild<T>(child);
                if (result != null) results.Add(result);
            }
            return results.Count > 0 ? results : default;
        }

        public static T? GetParent<T>(DependencyObject d) where T : DependencyObject => d switch
        {
            null => default,
            T t => t,
            System.Windows.Window _ => null,
            _ => GetParent<T>(VisualTreeHelper.GetParent(d))
        };

        public static DependencyObject GetTopParent(DependencyObject d) =>
       VisualTreeHelper.GetParent(d) == null ? d : GetTopParent(VisualTreeHelper.GetParent(d));

        public static IntPtr GetHandle(this Visual visual) => (PresentationSource.FromVisual(visual) as HwndSource)?.Handle ?? IntPtr.Zero;

        internal static void HitTestVisibleElements(Visual visual, HitTestResultCallback resultCallback, HitTestParameters parameters) =>
            VisualTreeHelper.HitTest(visual, ExcludeNonVisualElements, resultCallback, parameters);

        private static HitTestFilterBehavior ExcludeNonVisualElements(DependencyObject potentialHitTestTarget)
        {
            if (!(potentialHitTestTarget is Visual)) return HitTestFilterBehavior.ContinueSkipSelfAndChildren;

            if (!(potentialHitTestTarget is UIElement uIElement) || uIElement.IsVisible && uIElement.IsEnabled)
                return HitTestFilterBehavior.Continue;

            return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
        }

        /// <summary>
        /// 输出所有父级 <see cref="Debug.WriteLine"/>
        /// </summary>
        public static void PingParent(DependencyObject d)
        {
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(d);

            while (parent != null)
            {
                Debug.WriteLine(parent);
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }
        }


        /// <summary>
        /// 输出所有子级 <see cref="Debug.WriteLine"/>
        /// </summary>
        public static void PingTopChild(DependencyObject d)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);

                Debug.WriteLine(child);
            }
        }
    }
}
