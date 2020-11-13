using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

namespace GraphicsUWP
{
    public class BasicNavPage : Page
    {
        //#1
        protected Color DDALineColor = Colors.SkyBlue;
        protected Color BresenhamLineColor = Colors.Yellow;

        //#2
        protected Color midPointCircleColor = Colors.MediumVioletRed;
        protected Color BresenhamCircleColor = Colors.OrangeRed;
        protected Color midPointEllipseColor = Colors.Pink;

        //#3
        protected Color polygonEdgeColor = Colors.DarkTurquoise;
        protected Color innerColor = Colors.DarkOrange;

        //#4
        protected Color clipRectColor = Colors.Brown;
        protected Color originalLineColor = Colors.Green;
        protected Color newLineColor = Colors.GreenYellow;

        protected int halfCanvasWidth;
        protected int halfCanvasHeight;

        //用于切换页面
        protected MainPage _parentWin;
        public MainPage ParentWindow
        {
            get { return _parentWin; }
            set { _parentWin = value; }
        }
        public BasicNavPage()
        {

        }

        //为两变量赋值
        public void GetMyCanvasSize()
        {
            halfCanvasWidth = (int)ParentWindow.myCanvas.ActualWidth / 2;
            halfCanvasHeight = (int)ParentWindow.myCanvas.ActualHeight / 2;
        }


        //画点
        protected void DrawPoint(int x, int y, Color myColor)
        {
            Ellipse myPoint = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(myColor),
                Stroke = new SolidColorBrush(myColor)
            };

            //关键句——设置作为点的椭圆位置
            //myPoint.SetValue(Canvas.ZIndexProperty, 1);
            myPoint.SetValue(Canvas.LeftProperty, x * 20 - 3 + halfCanvasWidth);        //在此处传入的参数为抽象坐标对应的实际坐标
            myPoint.SetValue(Canvas.TopProperty, y * -20 - 3 + halfCanvasHeight);       //-3取消点自身大小(6)造成的误差

            ParentWindow.myCanvas.Children.Add(myPoint);
        }


        ////DDA算法画线_普通版本
        //protected async void DDALine(int x1, int y1, int x2, int y2, Color myColor)
        //{
        //    double increx, increy, x, y;
        //    int steps = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));

        //    increx = (double)(x2 - x1) / steps;     //此两个变量中有一个为1
        //    increy = (double)(y2 - y1) / steps;
        //    x = x1;
        //    y = y1;

        //    for (int i = 0; i <= steps; i++)
        //    {
        //        await System.Threading.Tasks.Task.Delay(150);

        //        //此处需取绝对值，避免负数问题
        //        if (Math.Abs(Math.Abs(increx) - 1) <= 1e-6)  //斜率-1~1
        //            if (y >= 0)
        //                DrawPoint((int)x, (int)(y + 0.5), myColor);
        //            else
        //                DrawPoint((int)x, (int)Math.Ceiling(y - 0.5), myColor);  //Ceiling——取比当前数大的最小整数
        //        else
        //            if (x >= 0)
        //            DrawPoint((int)(x + 0.5), (int)y, myColor);
        //        else
        //            DrawPoint((int)Math.Ceiling(x - 0.5), (int)y, myColor);
        //        x += increx;
        //        y += increy;
        //    }
        //}


        //DDA算法画线_Async版本
        protected async Task DDALineAsync(int x1, int y1, int x2, int y2, Color myColor)
        {
            double increx, increy, x, y;
            int steps = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));

            increx = (double)(x2 - x1) / steps;     //此两个变量中有一个为1
            increy = (double)(y2 - y1) / steps;
            x = x1;
            y = y1;

            for (int i = 0; i <= steps; i++)
            {
                await Task.Delay(80);

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
        }
    }
}
