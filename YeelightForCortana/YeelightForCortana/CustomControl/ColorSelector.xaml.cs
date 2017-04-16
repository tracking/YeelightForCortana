using ColorMine.ColorSpaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace YeelightForCortana.CustomControl
{
    /// <summary>
    /// 颜色选择器
    /// </summary>
    public sealed partial class ColorSelector : UserControl
    {
        private Hsv hsv;
        private Rgb rgb;

        public Hsv Hsv { get => hsv; }
        public Rgb Rgb { get => rgb; }

        public ColorSelector()
        {
            this.InitializeComponent();
        }

        // 颜色选择框鼠标按下
        private void ColorViewer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // 设置可移动
            Dot.Tag = true;
            // 触发移动事件
            ColorViewer_PointerMoved(sender, e);
        }
        // 颜色选择框鼠标抬起
        private void ColorViewer_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            // 设置不可移动
            Dot.Tag = null;
        }
        // 颜色选择框鼠标移动
        private void ColorViewer_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            // 不可移动
            if (Dot.Tag == null)
                return;

            // 获取当前指针位置
            Point p = e.GetCurrentPoint(ColorViewer).Position;
            var margin = Dot.Margin;

            double dotWidthHalf = Dot.Width / 2;
            double dotHeightHalf = Dot.Height / 2;

            double minLeft = 0 - dotWidthHalf;
            double minTop = 0 - dotHeightHalf;
            double maxLeft = ColorViewer.ActualWidth - dotWidthHalf;
            double maxTop = ColorViewer.ActualHeight - dotHeightHalf;

            margin.Left = Math.Max(Math.Min(p.X - dotWidthHalf, maxLeft), minLeft);
            margin.Top = Math.Max(Math.Min(p.Y - dotHeightHalf, maxTop), minTop);
            margin.Right = 0;
            margin.Bottom = 0;
            Dot.Margin = margin;

            // 计算颜色
            CalculateColor();
        }

        // 计算颜色
        private void CalculateColor()
        {
            double left = Math.Max(Math.Min(Dot.Margin.Left, ColorViewer.ActualWidth), 0);
            double top = Math.Max(Math.Min(Dot.Margin.Top, ColorViewer.ActualHeight), 0);
            double h = (left / ColorViewer.ActualWidth) * 360;
            double s = (top / ColorViewer.ActualHeight) * 100;

            // 创建HSV
            hsv = new Hsv();
            hsv.V = 1;
            hsv.H = h;
            hsv.S = s / 100;

            // 计算颜色
            rgb = hsv.To<Rgb>();

            // 显示颜色
            ColorViewer.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)rgb.R, (byte)rgb.G, (byte)rgb.B));
        }
    }
}
