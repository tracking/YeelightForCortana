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

        // 颜色改变事件
        public delegate void ColorChangeEvent(object sender);
        public event ColorChangeEvent ColorChange;

        public Hsv Hsv
        {
            get => hsv;
            set
            {
                this.hsv = value;
                this.hsv.V = 1;

                // 重新计算位置
                this.CalculatePoint();
            }
        }
        public Rgb Rgb { get => rgb; }

        public ColorSelector()
        {
            this.InitializeComponent();

            // 初始化默认颜色
            this.hsv = new Hsv() { H = 0, S = 0, V = 1 };
            this.rgb = this.hsv.To<Rgb>();
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
            // 触发事件
            ColorChange?.Invoke(sender);
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
            double left = Math.Max(Math.Min(Dot.Margin.Left + (Dot.Width / 2), ColorViewer.ActualWidth), 0);
            double top = Math.Max(Math.Min(Dot.Margin.Top + (Dot.Height / 2), ColorViewer.ActualHeight), 0);
            double h = (left / ColorViewer.ActualWidth) * 360;
            double s = top / ColorViewer.ActualHeight;

            // 创建HSV
            hsv = new Hsv();
            hsv.V = 1;
            hsv.H = h;
            hsv.S = s;

            // 计算颜色
            rgb = hsv.To<Rgb>();

            // 显示颜色
            ColorViewer.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)rgb.R, (byte)rgb.G, (byte)rgb.B));
        }
        /// <summary>
        /// 计算Point
        /// </summary>
        private void CalculatePoint()
        {
            double left = ((this.hsv.H / 360) * ColorViewer.ActualWidth) - (Dot.Width / 2);
            double top = (this.hsv.S * ColorViewer.ActualHeight) - (Dot.Height / 2);

            var margin = Dot.Margin;
            margin.Left = left;
            margin.Top = top;
            margin.Right = 0;
            margin.Bottom = 0;
            Dot.Margin = margin;

            // 重新渲染颜色
            CalculateColor();
        }
    }
}
