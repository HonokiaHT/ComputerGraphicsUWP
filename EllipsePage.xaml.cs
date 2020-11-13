using DocumentFormat.OpenXml.Vml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GraphicsUWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class EllipsePage : BasicNavPage
    {
        public EllipsePage()
        {
            this.InitializeComponent();
        }

            //圆
        //中点画圆
        private async void MidPointCircle(int r)
        {
            int x = 0, y = r;
            double d = 1.25 - r;
            
            CirclePoints_8(x, y, midPointCircleColor); //显示圆弧上的八个对称点

            while (x <= y)
            {
                if (d < 0)  //取右侧点
                    d += 2 * x + 3;
                else        //取右下侧点
                { 
                    d += 2 * (x - y) + 5;
                    y--;
                }
                x++;

                await System.Threading.Tasks.Task.Delay(300);

                CirclePoints_8(x, y, midPointCircleColor);
            }
            DrawEllipse(midPointCircleColor, r);
        }

        //Bresenham画圆
        private async void BresenhamCircle(int r)
        {
            int x = 0, y = r;
            int delta, deltaHD, deltaDV, direction;
            delta = 2 * (1 - r);    //△d的初始值
            int Limit = 0;          //Bresenham也许最好使用4对称

            while (y >= Limit)
            {
                await System.Threading.Tasks.Task.Delay(300);

                CirclePoints_4(x, y, BresenhamCircleColor); //显示圆弧上的八个对称点
                if (delta < 0)
                {
                    deltaHD = 2 * (delta + y) - 1;
                    
                    if (deltaHD <= 0)   //deltaHD小于0，说明到H距离小于到D，取H点
                        direction = 1;
                    else
                        direction = 2;  //否则取D点
                }
                else if (delta > 0)
                {
                    deltaDV = 2 * (delta - x) - 1;
                    
                    if (deltaDV < 0)    //deltaDV小于0，说明到D距离小于到V，取D点
                        direction = 2;
                    else
                        direction = 3;  //否则取V点
                }
                else
                    direction = 2;

                switch (direction)
                {
                    case 1:
                        x++;
                        delta += 2 * x + 1;
                        break;
                    case 2:
                        x++;
                        y--;
                        delta += 2 * (x - y + 1);
                        break;
                    case 3:
                        y--;
                        delta += (-2 * y + 1);
                        break;
                }/*switch */
            }/*while*/

            DrawEllipse(BresenhamCircleColor, r);
        }/*Bresenham_Circle*/


            //椭圆
        //中点画椭圆
        private async void MidPointEllipse(int a, int b)
        {
            int x = 0, y = b;
            int a2 = a * a, b2 = b * b;
            double d = b2 + a2 * (-b + 0.25);   //判别式


            CirclePoints_4(x, y, midPointEllipseColor); //显示椭圆弧上的4个对称点

            //上半圆弧
            while (b2 * (x + 1) < a2 * (y - 0.5))
            {
                //切线斜率 -1 ~ 0，判断取右侧点或右下侧点
                if (d < 0)  //中点在椭圆内，取右侧点
                    d += b2 * (2 * x + 3);
                else        //中点在椭圆外，取右下侧点
                {
                    d += b2 * (2 * x + 3) + a2 * (-2 * y + 2);
                    y--;
                }
                x++;

                await System.Threading.Tasks.Task.Delay(300);
                CirclePoints_4(x, y, midPointEllipseColor);
            }
            //下半圆弧
            d = b2 * (x + 0.5) * (x + 0.5) + a2 * (y - 1) * (y - 1) - a2 * b2;
            while (y >= 0) 
            {
                //切线斜率 -∞ ~ -1，判断取右下侧点或下侧点
                if (d > 0)   //中点在椭圆外，取下侧点
                    d += a2 * (-2 * y + 3);
                else         //中点在椭圆内，取右下侧点
                {
                    d += b2 * (2 * x + 2) + a2 * (-2 * y + 3);
                    x++;
                }
                y--;

                await System.Threading.Tasks.Task.Delay(300);
                CirclePoints_4(x, y, midPointEllipseColor);
            }

            DrawEllipse(midPointEllipseColor, a, b);
        }

        //库函数画圆/椭圆
        private void DrawEllipse(Color myColor, int a, int b = 0)
        {
            Ellipse myEllipse = new Ellipse
            {
                Width = 2 * a * 20,
                Height = 2 * a * 20,
                Stroke = new SolidColorBrush(myColor)
            };
            if (b != 0) //默认画圆，b不为零则画椭圆
                myEllipse.Height = 2 * b * 20;

            myEllipse.SetValue(Canvas.LeftProperty, halfCanvasWidth - myEllipse.Width / 2);        //在此处传入的参数为抽象坐标对应的实际坐标
            myEllipse.SetValue(Canvas.TopProperty, halfCanvasHeight - myEllipse.Height / 2);       //-3取消点自身大小(6)造成的误差

            ParentWindow.myCanvas.Children.Add(myEllipse);
        }


        //画圆对称部分_8对称
        private void CirclePoints_8(int x, int y, Color myColor)
        {
            DrawPoint(x, y, myColor);
            DrawPoint(x, -y, myColor);
            DrawPoint(-x, y, myColor);
            DrawPoint(-x, -y, myColor);
            DrawPoint(y, x, myColor);
            DrawPoint(y, -x, myColor);
            DrawPoint(-y, x, myColor);
            DrawPoint(-y, -x, myColor);
        }

        //画圆对称部分_4对称
        private void CirclePoints_4(int x, int y, Color myColor)
        {
            DrawPoint(x, y, myColor);
            DrawPoint(x, -y, myColor);
            DrawPoint(-x, y, myColor);
            DrawPoint(-x, -y, myColor);
        }


            //按钮
        //按钮Draw
        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(r_text.Text, out int r);
            int.TryParse(a_text.Text, out int a);
            int.TryParse(b_text.Text, out int b);
            string selectedDrawWhat = drawWhat.SelectedItem as string;
            string selectedDrawMode = drawMode.SelectedItem as string;
            
            if(selectedDrawWhat == "Circle")
            {
                if (r < 1 || r > 21)
                {
                    FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
                    return;
                }

                if (selectedDrawMode == "MidPoint")
                    MidPointCircle(r);
                else if (selectedDrawMode == "Bresenham")
                    BresenhamCircle(r);
                else if (selectedDrawMode == "库函数")
                    DrawEllipse(Colors.White, r);
            }
            else if (selectedDrawWhat == "Ellipse")
            {
                if (a < 1 || a > 21 || b < 1 || b > 21)
                {
                    FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
                    return;
                }

                if (selectedDrawMode == "MidPoint")
                    MidPointEllipse(a, b);
                else if (selectedDrawMode == "库函数")
                    DrawEllipse(Colors.White, a, b);
            }
        }

        private void DrawWhat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selected = e.AddedItems[0] as string;
            if (selected == "Circle")
            {
                drawMode.Items.Insert(1, "Bresenham");
                a_text.Visibility = Visibility.Collapsed;
                b_text.Visibility = Visibility.Collapsed;
                r_text.Visibility = Visibility.Visible;
            }
            else if(selected == "Ellipse")
            {
                drawMode.Items.Remove("Bresenham");
                a_text.Visibility = Visibility.Visible;
                b_text.Visibility = Visibility.Visible;
                r_text.Visibility = Visibility.Collapsed;
            }
        }

        private void DrawWhat_Loaded(object sender, RoutedEventArgs e)
        {
            drawWhat.SelectedItem = drawWhat.Items[0];
        }
    }
}
