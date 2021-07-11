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
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Numerics;
// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GraphicsUWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LinePage : BasicNavPage
    {
        public LinePage()
        {
            this.InitializeComponent();
        }

        private void DrawLine(int x1, int y1, int x2, int y2, int lineWidth, Color myColor, double op)
        {
            Line newLine = new Line
            {
                X1 = x1 * 20 + halfCanvasWidth,
                X2 = x2 * 20 + halfCanvasWidth,
                Y1 = y1 * -20 + halfCanvasHeight,
                Y2 = y2 * -20 + halfCanvasHeight,
                StrokeThickness = lineWidth,
                Opacity = op,
                Stroke = new SolidColorBrush(myColor)
            };
            ParentWindow.myCanvas.Children.Add(newLine);
        }

        private async void DDALine(int x1, int y1, int x2, int y2, Color myColor)
        {
            double increx, increy, x, y;
            int steps = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));

            increx = (double)(x2 - x1) / steps;     //此两个变量中有一个为1
            increy = (double)(y2 - y1) / steps;
            x = x1;
            y = y1;

            //Polyline polyline1 = new Polyline
            //{
            //    Stroke = new SolidColorBrush(Colors.SkyBlue),
            //    StrokeThickness = 1
            //};     //绘制多段线

            //var points = new PointCollection(); //创建点集并加入点

            for (int i = 0; i <= steps; i++)
            {
                //PutPixel((int)x, (int)y, Colors.White); //在(x，y)处，以color色画点

                //points.Add(new Point((int)x, (int)y));
                await System.Threading.Tasks.Task.Delay(150);

                //此处需取绝对值，避免负数问题
                if (Math.Abs(Math.Abs(increx) - 1) <= 1e-6)  //斜率-1~1
                    if (y >= 0)
                        DrawPoint((int)x, (int)(y + 0.5), myColor);
                    else
                        DrawPoint((int)x, (int)Math.Ceiling(y - 0.5), myColor);  //Ceiling——取比当前数大的最小整数
                else
                    if (x >= 0)
                    DrawPoint((int)(x + 0.5), (int)y, myColor);
                else
                    DrawPoint((int)Math.Ceiling(x - 0.5), (int)y, myColor);
                x += increx;
                y += increy;
            }
            DrawLine(x1, y1, x2, y2, 1, myColor, 1);

            //polyline1.Points = points;
            //ParentWindow.myCanvas.Children.Add(polyline1);
        }

        private async void BresenhamLine(int x1, int y1, int x2, int y2, Color myColor)
        {
            int increx, increy;
            int x = x1, y = y1;

            increx = (x2 - x1) / Math.Abs(x2 - x1); //记录正负
            increy = (y2 - y1) / Math.Abs(y2 - y1);

            int dx = Math.Abs(x2 - x1), dy = Math.Abs(y2 - y1);
            int e;
            if (dx >= dy) e = -1 * dx;
            else e = -1 * dy;

            if (dx >= dy)   //斜率-1~1
            {
                for (int i = 0; i <= dx; i++)
                {
                    await System.Threading.Tasks.Task.Delay(150);
                    DrawPoint(x, y, myColor);

                    x += increx;    //1
                    e += 2 * dy;
                    
                    //解决正反向画线不一致
                    if (e > 0 || e == 0 && increy > 0)
                    {
                        y += increy;
                        e -= 2 * dx;
                    }
                }
            }
            else
            {
                for (int i = 0; i <= dy; i++)
                {
                    await System.Threading.Tasks.Task.Delay(150);
                    DrawPoint(x, y, myColor);

                    y += increy;
                    e += 2 * dx;
                    if (e > 0 || e == 0 && increx > 0)
                    {
                        x += increx;
                        e -= 2 * dy;
                    }
                }
            }
            DrawLine(x1, y1, x2, y2, 1, myColor, 1);
        }

        private void DrawLineButton_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(x1_control.Text, out int x1);
            int.TryParse(y1_control.Text, out int y1);
            int.TryParse(x2_control.Text, out int x2);
            int.TryParse(y2_control.Text, out int y2);

            if (x1 < -21 || x2 < -21 || y1 < -21 || y2 < -21 || x1 > 21 || x2 > 21 || y1 > 21 || y2 > 21)
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
                return;
            }

            if (drawMode.SelectedItem.ToString() == "DDA")
                DDALine(x1, y1, x2, y2, DDALineColor);
            else if (drawMode.SelectedItem.ToString() == "Bresenham")
                BresenhamLine(x1, y1, x2, y2, BresenhamLineColor);
            else if (drawMode.SelectedItem.ToString() == "库函数")
                DrawLine(x1, y1, x2, y2, 1, Colors.White, 1);
        }

        //private void Combo3_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        //{
        //    throw new NotImplementedException();
        //}
        //private void displayModeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}
        //private void NavLinksList_ItemClick(object sender, ItemClickEventArgs e)        //某个被选中后发生什么
        //{
        //    throw new NotImplementedException();
        //}
    }
}
